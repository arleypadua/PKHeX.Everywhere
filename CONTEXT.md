# PKHeX.Everywhere

Save editing for Pokémon games, in the browser and on the command line. The web UI is moving from Blazor to React page by page, while all save editing stays in C#.

## Language

**Engine**:
The Blazor-free .NET layer that holds the loaded save and answers queries and commands from JavaScript through a single dispatcher.
_Avoid_: backend, interop, facade. The Facade is the layer the Engine calls.

**SDK**:
The generated TypeScript packages that call the Engine: `@pkhex-everywhere/engine` for the typed client and `@pkhex-everywhere/react` for the hooks.
_Avoid_: bindings, API client

**Handle**:
Where an entity sits in the save, such as a party slot or a box slot. Every entity that crosses to JavaScript carries `{ id, at }`, where `id` is its opaque, stable identity and `at` is its Handle. Commands take the Handle.
_Avoid_: location, reference, pointer

**Topic**:
A hierarchical path, such as `party`, `box/3` or `items/balls`, naming a part of the save that queries read and commands write. A topic covers every path beneath it.
_Avoid_: cache key, channel, event

**Entity hook**:
A generated React hook, such as `useParty()` or `usePokemon(at)`, that returns an entity's data together with its commands.
_Avoid_: query hook, data hook

**React page**:
A page written in React and mounted inside a Blazor route, which keeps the Blazor layout, menu and router around it.
_Avoid_: island, which is a Blazor component mounted inside React. Also avoid micro-frontend.
