# A React app over a .NET WASM engine

Supersedes [ADR 0002](0002-react-pages-inside-blazor-over-a-dotnet-engine.md). Every page is now React, so React owns the shell: the layout, the menu, the router and the notifications. Blazor only boots .NET and signals that the Engine is ready. The Engine pattern from ADR 0002 still holds. Context: pkhex-web/issue-tracker#91.

## Decision

- `App.razor` renders nothing. After its first render it calls `SignalReady()` and starts the React app, which mounts its own root next to the Blazor one.
- React Router v7 in declarative mode serves every route with the paths and query strings Blazor used. `routes.ts` builds the URLs.
- One antd `App` at the root hosts every notification, so a notification outlives the page that raised it. The same host shows plug-in outcomes from `PlugInRan`.
- Loading or closing a save raises the `gameLoaded` or `gameClosed` Engine event, and the app navigates to `/` or `/load`.
- Plug-in page modules mount inside the routed page, like any other React component.

## Amendment: the Engine host

Blazor no longer boots .NET for the React app. `PKHeX.Everywhere.Engine.Host` (`Microsoft.NET.Sdk.WebAssembly`) has no UI: its `Main` attaches the plug-in host and calls `SignalReady()`. `wasmHost` in the Engine SDK loads `_framework/dotnet.js` and runs `Main`. Vite owns `index.html` and copies the host's `_framework` into `dist`. The host copies Blazor's trim defaults, because plug-ins load with `Assembly.Load(bytes)` and were verified against them. Context: pkhex-web/issue-tracker#108.

## Why now

ADR 0002 kept Blazor as the host until most pages were React, to avoid the island and the navigation bridge between React Router and Blazor's `NavigationManager`. With no Blazor page left, nothing needs an island, and React Router is the only router, so there is no bridge to keep in sync.

## Rejected alternatives

- **A catch-all Blazor route as a fallback.** No Blazor page is left for it to serve.
- **Data mode or framework mode of React Router.** Pages load their own data through Entity hooks and Suspense, so route loaders would duplicate the query cache.
