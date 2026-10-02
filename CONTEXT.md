# PKHeX.Everywhere

Save editing for Pokémon games, in the browser and on the command line. The web UI is a React app, while all save editing stays in C# and runs in .NET WebAssembly.

## Language

**Facade**:
The domain model over PKHeX.Core: games, trainers, Pokémon, boxes and items, with save-format quirks hidden. It holds the editing rules.
_Avoid_: wrapper, adapter

**Engine**:
The UI-free .NET layer that holds the loaded save and answers queries and commands from JavaScript by calling the Facade. It has no editing rules of its own.
_Avoid_: backend, interop

**SDK**:
The TypeScript packages that call the Engine: a client package and its React bindings, partly generated from the Engine.
_Avoid_: API client

**Handle**:
Where something sits in the save, such as a Pokémon's party or box slot, or a pouch item as `{ pouch, itemId }`. A Pokémon crosses to JavaScript as `{ id, at }`, with its opaque id and its Handle.
_Avoid_: pointer, reference

**Draft**:
The one unsaved Pokémon the Session holds while the editor changes it. It is addressed as a Handle with source `draft` and reaches the save only on commit. See [ADR 0005](docs/adr/0005-editing-goes-through-an-engine-held-draft-slot.md).
_Avoid_: working copy, scratch Pokémon

**Topic**:
A hierarchical path, such as `party`, `box/3` or `inventory`, naming a part of the save that queries read and commands write. A Topic covers every path beneath it.
_Avoid_: cache key, channel

**Engine event**:
A typed record a command raises through the Session, published after the command succeeds. `PlugInRan` is the exception: it is published as soon as the hook runs. JS receives events next to Topics.
_Avoid_: notification, message

**Entity hook**:
A generated React hook, such as `useParty()` or `usePokemon(at)`, that returns an entity's data together with its commands.
_Avoid_: query hook, data hook

**Plug-in host**:
The Engine-side registry and runtime for plug-ins built against SDK v2. It loads a plug-in from its assembly bytes and runs its hooks, and it publishes `PlugInRan` after each run.
_Avoid_: plug-in runtime, plug-in manager

**Page module**:
A JS module embedded in a plug-in assembly that renders one of the plug-in's pages through `mount(element, ctx)`.
_Avoid_: plug-in component, plug-in page component
