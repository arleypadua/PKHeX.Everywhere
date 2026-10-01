# PKHeX.Web.React

React pages mounted inside PKHeX.Web through the `<ReactPage Name="...">` Blazor component.

- `npm start` runs `dotnet watch` for PKHeX.Web and the Vite dev server. Open http://localhost:5062. React pages load from Vite with HMR, Razor files hot reload through `dotnet watch`. `./dev.sh stop` stops both, from any terminal.
- `npm run build` writes ES modules to `../PKHeX.Web/wwwroot/react`, which Release builds load. Run it before `dotnet publish`.

To add a page, create a folder for it under `src/pages` (such as `src/pages/party/PartyPage.tsx`), register it in `src/pages.ts`, and render `<ReactPage Name="..." />` from a Blazor route.

Put anything another page could use in its own file: components under `src/components`, hooks under `src/hooks`. Use `App.useApp()` for notifications and modals; the page shell provides antd's `App`.

Give every antd `Table` `scroll={{ x: 'max-content' }}`. AntBlazor's `Responsive` flag stacked rows into cards on narrow screens. antd for React has no such flag, so a table without `scroll` grows past the window on phones.

Pages get host state from `src/host.ts`: navigation, the theme and the calculator URL from the user's general settings.
