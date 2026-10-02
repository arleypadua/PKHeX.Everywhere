# PKHeX.Everywhere.Engine

The save-editing API that JavaScript calls. It has no Blazor dependency.

Handlers stay thin and call `PKHeX.Facade` for every editing rule. See [ADR 0004](../../docs/adr/0004-domain-logic-lives-in-the-facade.md).

JS calls one `[JSExport]`, `EngineExports.Call(name, argsJson)`, which returns a promise. Arguments are a JSON array, and every response is `{ ok: true, value }` or `{ ok: false, error: { code, message } }`. Error codes are listed in `ErrorCodes.cs`.

The loaded save lives in `Session.Current`, which `GameService` in PKHeX.Web uses too.

When the save changes, `Session` raises `Changed` with the changed Topics, and JS receives them through `globalThis.pkhexEngineOnChange`. Loading a save changes `*`, which covers every Topic. Topics are listed in `Topics.cs`.

A command announces domain changes as Engine events, records implementing `IEngineEvent` (see `Events.cs`). `Session` publishes them once the command succeeds, and JS receives each one as JSON with a camelCase `type`, such as `{ type: 'itemChanged', itemId, count }`, through `globalThis.pkhexEngineOnEvent`. The SDK passes them to `engine.onEvent(listener)`, typed as the `EngineEvent` union. Events hold only primitives and DTOs, and the generator writes their JSON. `PlugInRan` is published as soon as the hook runs, even when the command then fails. `Session.Load` and `Session.Close` raise `GameLoaded` and `GameClosed`, so the app follows a loaded or closed save however it happened.

## Adding a query

1. Add a DTO record under `Dtos/`, with a mapping from the Facade type. Only types in the `PKHeX.Everywhere.Engine` namespaces can cross the boundary.
2. Add a static method with `[Query("entity.name", Topics.Entity)]` to `Handlers/<Entity>Handlers.cs`, listing the Topics it reads. A query that doesn't read the save lists none, and clients never refetch it. A `Game` parameter receives the loaded save (or fails with `no-save`). Other parameters come from the JSON arguments.
3. Build the solution. The source generator (`PKHeX.Everywhere.Engine.Generators`) adds the call to the dispatcher and writes its JSON code. Building `PKHeX.Everywhere.Engine.CodeGen` then updates the TypeScript in `PKHeX.Everywhere.Engine.Sdk/packages/engine/src/generated`.
4. Commit the generated TypeScript. CI fails if it is out of date.

## Adding a command

1. Add a static method with `[Command("entity.verb")]` to `Handlers/<Entity>Handlers.cs`. A command writes the Topic of each `IHandle` argument it takes, such as the Pokémon at `at` (unless the argument is null), plus any Topics listed on the attribute. A command that changes nothing in the save lists none. When the save decides the Topics, such as a Let's Go party member that also sits in a box, pass them to `session.AlsoWrote`. Return a value only when the command creates something.
2. Throw `EngineException` with a code from `ErrorCodes.cs` for expected failures. JS receives it as a rejected `EngineError`. `engine.onCallFailed(listener)` in the SDK sees every failed call, with its name.
3. A handler that awaits returns `Task` or `Task<T>`. Don't block on a task: the browser runs .NET on a single thread, so blocking on an unfinished task fails.
4. Build and commit the generated TypeScript, as for queries.
5. Add sample arguments for the call to `CommandTopicTests`. That test runs every command against the test saves and fails when a query's result changes outside the Topics the command reported.

## Entity hooks

CodeGen groups calls by entity, the part of the call name before the dot, and writes a React hook for each entity with a `get` query to `PKHeX.Everywhere.Engine.Sdk/packages/react/src/generated/hooks.ts`:

- A `get` without arguments makes a collection hook, such as `useParty()`, returning `{ party }` plus the entity's commands.
- A `get` taking one handle makes an item hook, such as `usePokemon(at)`, returning `{ pokemon }` plus the entity's commands with the handle bound.

The hook is named `use{Entity}` unless the handler class has `[EntityHook("...")]`. Wrap a generated hook in a hand-written one when an entity needs more; don't edit generated code.

## Handlers in another assembly

A project other than the Engine can declare handlers, so the Engine never references it:

1. Reference the Engine, and `PKHeX.Everywhere.Engine.Generators` with `OutputItemType="Analyzer" ReferenceOutputAssembly="false"`. The generator writes an internal `HandlerRegistry` in a namespace named after the assembly.
2. Pass `HandlerRegistry.TryInvoke` and `HandlerRegistry.TryWriteEvent` to `session.AddHandlers` when the project attaches to a Session. The Engine's own calls win a name clash.
3. Add the project as an `EngineHandlerAssembly` in `PKHeX.Everywhere.Engine.CodeGen.csproj`, so the SDK covers its calls. CodeGen fails when two assemblies declare the same call.

DTOs follow the same rules as the Engine's, and must live under a `PKHeX.Everywhere.Engine` namespace. Topics and error codes come from the Engine.

`PKHeX.Everywhere.Engine.Tests.Handlers` is an example.

`byte[]` crosses the boundary as a base64 string, typed `Base64` in TypeScript.

A nullable record parameter with a default, such as the fields of `PokemonPatch`, is optional in TypeScript. A missing property reads as null.

## The draft

`pokemon.edit(at)` opens a draft Pokémon in the Session, addressed as `{ source: 'draft', slot: 0 }` (`draftHandle` in the SDK) with the Topic `draft`. Pokémon calls resolve Handles through `session.Find`, which covers the draft. A call that only applies to a saved slot uses `game.FindSaved`, which rejects the draft with `draft-not-allowed`.

`pokemon.clone(at)` opens a draft from a copy with a new PID and no nickname. A clone has no slot, so `pokemon.commit()` rejects it with `no-slot`; `pokemon.addToBox()` writes it to the first empty box slot instead, or fails with `box-full`. See [ADR 0005](../../docs/adr/0005-editing-goes-through-an-engine-held-draft-slot.md).

Contract tests live in `PKHeX.Everywhere.Engine.Tests`.
