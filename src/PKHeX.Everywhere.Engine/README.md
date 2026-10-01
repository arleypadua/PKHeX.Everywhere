# PKHeX.Everywhere.Engine

The save-editing API that JavaScript calls. It has no Blazor dependency.

JS calls one `[JSExport]`, `EngineExports.Call(name, argsJson)`. Arguments are a JSON array, and every response is `{ ok: true, value }` or `{ ok: false, error: { code, message } }`. Error codes are listed in `ErrorCodes.cs`.

The loaded save lives in `Session.Current`, which `GameService` in PKHeX.Web uses too.

## Adding a query

1. Add a DTO record under `Dtos/`, with a mapping from the Facade type. Only types in the `PKHeX.Everywhere.Engine` namespaces can cross the boundary.
2. Add a static method with `[Query("entity.name")]` to `Handlers/<Entity>Handlers.cs`. A `Game` parameter receives the loaded save (or fails with `no-save`). Other parameters come from the JSON arguments.
3. Build. The source generator (`PKHeX.Everywhere.Engine.Generators`) adds the call to the dispatcher and writes its JSON code. `PKHeX.Everywhere.Engine.CodeGen` then updates the TypeScript in `PKHeX.Everywhere.Engine.Sdk/packages/engine/src/generated`.
4. Commit the generated TypeScript. CI fails if it is out of date.

Contract tests live in `PKHeX.Everywhere.Engine.Tests`.
