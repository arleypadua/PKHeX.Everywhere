# PKHeX.Web.React

React pages mounted inside PKHeX.Web through the `<ReactPage Name="...">` Blazor component.

- `npm start` runs `dotnet watch` for PKHeX.Web and the Vite dev server. Open http://localhost:5062. React pages load from Vite with HMR, Razor files hot reload through `dotnet watch`.
- `npm run build` writes ES modules to `../PKHeX.Web/wwwroot/react`, which Release builds load. Run it before `dotnet publish`.

To add a page, create it under `src/pages`, register it in `src/pages.ts`, and render `<ReactPage Name="..." />` from a Blazor route.

Pages get host state from `src/host.ts`: navigation, the theme and the calculator URL from the user's general settings.
