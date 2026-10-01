# React pages inside Blazor, over a .NET engine

The web UI moves from AntDesign Blazor to AntDesign React one page at a time, and all save editing stays in C#. Blazor stays the host and keeps the layout, menu and router. A migrated page is a Blazor route that mounts a React page. React pages read and write the save only through the Engine, using the generated SDK.

## Why Blazor stays the host

Spike pkhex-web/issue-tracker#44 put React in charge and mounted Blazor pages as islands. It worked, but React would have had to rebuild `MainLayout` before the first page shipped: the menu, header, theme toggle, plug-in menu entries, cookie consent and navigation tracking. Every route would also have needed the navigation bridge between React Router and Blazor's `NavigationManager`, which caused most of the spike's bugs. With Blazor as the host, the UX doesn't change, and a React page reaches a Blazor page with one call to `NavigateTo`.

## Why the layout swap comes last

A React layout replaces `MainLayout` once most pages are React. By then few Blazor pages need islands, and the catch-all island and navigation bridge from the spike only have to cover those. `useNavigate()` and `useTheme()` keep their call sites across the swap, so React pages don't change.

## The Engine pattern

- **One dispatcher.** The Engine has a single `[JSExport]` entry point. It takes a call name and JSON arguments, and returns `{ ok: true, value } | { ok: false, error }`. Queries and commands are attributed static methods, with handlers in one file per entity. A source generator builds the registry and JSON context, so nothing uses reflection and Release trimming stays safe.
- **Hand-written DTOs and Handles.** Only DTO records cross the boundary, each mapped from Facade types by a small function per entity. No `PKHeX.Core` or Facade type leaks, so a Facade refactor can't silently change the TypeScript API. Each Pokémon carries `{ id, at }`, an opaque id and its Handle. Commands take the Handle, and routes and React keys use the id.
- **Hierarchical Topics declared on handlers.** A query declares the Topics it reads. A command writes the Topics derived from its Handle, plus any declared on its attribute. Invalidating a Topic invalidates everything under it, and loading or closing a save invalidates everything. A test fails when a command changes data outside its declared Topics.
- **Codegen into a dependency-free SDK.** A tool reads the built Engine assembly and writes DTO types, the typed client, Topics, error codes and Entity hooks into checked-in TypeScript. CI fails when the output drifts. `@pkhex-everywhere/engine` has no runtime dependencies and boots through a host adapter, so it can later run without Blazor or in a worker. Every call is async for the same reason.
- **Our own React bindings.** `@pkhex-everywhere/react` is a small cache keyed by call and arguments, built on `useSyncExternalStore` and Topic subscriptions, with React as a peer dependency.
- **Generated Entity hooks.** Collection hooks such as `useParty()` return the data and the collection's commands. Item hooks such as `usePokemon(at)` return the data and commands with the Handle bound. Wrap a generated hook in a hand-written one when an entity needs more. Never edit generated code.
- **Suspense and typed errors.** Queries suspend on first load, keep old data while a refetch runs, and throw errors to the page's error boundary. Commands reject with a typed `EngineError` whose code is a generated union. A rejection nobody handles shows a notification.

## Rejected alternatives

- **React as the shell first.** This is what the spike built. We rejected it for the reasons in "Why Blazor stays the host".
- **TanStack Query.** Topic invalidation, Suspense and deduplication fit in a small cache on `useSyncExternalStore`. TanStack Query would add a dependency to a package meant to be published, and we would still have to map its cache keys to Topics.
- **Writable proxies.** Assigning `pokemon.level = 50` would hide that a write is async, can fail, and invalidates Topics. Explicit commands make all three visible.
- **One `[JSExport]` per call.** Each call would need its own interop glue, and the exports would grow into one long list. With one dispatcher, the interop is written once and codegen covers each new call.
- **Mapperly.** Generated mapping follows Facade shapes, so a Facade refactor could change a DTO without anyone noticing. Hand-written mapping makes each DTO change a deliberate edit.
