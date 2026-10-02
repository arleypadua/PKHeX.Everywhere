# PKHeX.Web.React

The PKHeX.Web UI. PKHeX.Web boots .NET and calls `start` in `src/main.tsx`, which mounts the app into `#root`. `src/layout` holds the shell: the router, the layouts, the menu and the app-level notification host.

- `npm start` runs `dotnet watch` for PKHeX.Web and the Vite dev server. Open http://localhost:5062. The React app loads from Vite with HMR. `./dev.sh stop` stops both, from any terminal.
- `npm run build` writes ES modules to `../PKHeX.Web/wwwroot/react`, which Release builds load. Run it before `dotnet publish`.
- Google Analytics runs only when the build sets `VITE_GOOGLE_ANALYTICS=true`. Only the GitHub Pages deploy sets it, so local and PR preview builds send nothing. Blazor sends `game_loaded` through `track` in `src/googleAnalytics.ts`.
- Sentry runs only when the build sets `VITE_SENTRY=true`, which only the GitHub Pages deploy does. `index.html` loads `react/sentry.js` while Blazor boots, so it reports a failed boot. `src/sentry.ts` adds the route and loaded game to every event. Blazor reports .NET exceptions through `captureBlazorError`.

To add a page, create a folder for it under `src/pages` (such as `src/pages/party/PartyPage.tsx`) and add a `<Route>` for it in `src/layout/AppShell.tsx`. Build its URLs in `src/routes.ts`, and link with React Router's `Link`. A plain `<a href>` reloads the app and drops the loaded save.

Put anything another page could use in its own file: components under `src/components`, hooks under `src/hooks`. Use `App.useApp()` for notifications and modals. The shell provides one antd `App`, so a notification outlives the page that raised it.

Give every antd `Table` `scroll={{ x: 'max-content' }}`. AntBlazor's `Responsive` flag stacked rows into cards on narrow screens. antd for React has no such flag, so a table without `scroll` grows past the window on phones.

Pages get navigation, the theme and the calculator URL from `src/host.ts`. `changeTheme` and `changeCalculatorUrl` write them to localStorage through `src/settings.ts`, under the keys Blazor used. The calculator presets live in `src/calculators.ts` and the release notes in `src/news.ts`.

Put a page's actions in `ButtonOrMenu` from `src/components`, not in a row of separate buttons, which runs off the screen on phones.
