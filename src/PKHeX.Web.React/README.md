# PKHeX.Web.React

React pages mounted inside PKHeX.Web through the `<ReactPage Name="...">` Blazor component.

- `npm start` runs `dotnet watch` for PKHeX.Web and the Vite dev server. Open http://localhost:5062. React pages load from Vite with HMR, Razor files hot reload through `dotnet watch`. `./dev.sh stop` stops both, from any terminal.
- `npm run build` writes ES modules to `../PKHeX.Web/wwwroot/react`, which Release builds load. Run it before `dotnet publish`.
- Google Analytics runs only when the build sets `VITE_GOOGLE_ANALYTICS=true`. Only the GitHub Pages deploy sets it, so local and PR preview builds send nothing. Blazor sends the events it still raises through `track` in `src/googleAnalytics.ts`.
- Sentry runs only when the build sets `VITE_SENTRY=true`, which only the GitHub Pages deploy does. `index.html` loads `react/sentry.js` while Blazor boots, so it reports a failed boot. `src/sentry.ts` adds the route and loaded game to every event. Blazor reports .NET exceptions through `captureBlazorError`.

To add a page, create a folder for it under `src/pages` (such as `src/pages/party/PartyPage.tsx`), register it in `src/pages.ts`, and render `<ReactPage Name="..." />` from a Blazor route.

Put anything another page could use in its own file: components under `src/components`, hooks under `src/hooks`. Use `App.useApp()` for notifications and modals; the page shell provides antd's `App`.

Give every antd `Table` `scroll={{ x: 'max-content' }}`. AntBlazor's `Responsive` flag stacked rows into cards on narrow screens. antd for React has no such flag, so a table without `scroll` grows past the window on phones.

Pages get navigation, the theme and the calculator URL from `src/host.ts`. `changeTheme` and `changeCalculatorUrl` write them to localStorage through `src/settings.ts`, under the keys Blazor used. The calculator presets live in `src/calculators.ts` and the release notes in `src/news.ts`. Blazor reads the theme and news through `ReactApp`.

When a Blazor page passes its actions to `ButtonOrMenu` or a `DropdownButton`, port them to `ButtonOrMenu` in `src/components`. Do not lay them out as separate buttons, since a row of buttons runs off the screen on phones. `PokemonPage.razor` (Save with Export *.pk and plug-in actions in the menu) is the next page this applies to.
