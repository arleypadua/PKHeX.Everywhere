# PKHeX.Web.React

The PKHeX.Web UI. Vite owns `index.html`, and `src/index.ts` mounts the app into `#root`. The Engine runs in `PKHeX.Everywhere.Engine.Host`, which `wasmHost` from the Engine SDK boots from `/_framework`. `src/layout` holds the shell: the router, the layouts, the menu and the app-level notification host.

- `npm run dev` builds the Engine host and serves the app and its `_framework` from Vite at http://localhost:5173, with HMR. Dev builds load the demo save on start. `./dev.sh stop` stops it, from any terminal.
- `npm run build` publishes the Engine host in Release and writes the app to `dist`, with the host's `_framework` copied in. `npm run preview` serves `dist`.
- `public/` holds the static files: `404.html`, `ads.txt`, `robots.txt`, the Google verification file, the icon, the font, the demo save and the blog. `npm run build:blog` builds the blog into `public/blog`, and `npm run build` runs it.
- Production deploys `dist` to GitHub Pages and PR previews deploy it to Cloudflare Pages. `PKHeX.Web.E2E` tests it.
- Google Analytics runs only when the build sets `VITE_GOOGLE_ANALYTICS=true`. Only the GitHub Pages deploy sets it, so local and PR preview builds send nothing.
- Sentry runs only when the build sets `VITE_SENTRY=true`, which only the GitHub Pages deploy does. `src/index.ts` starts it before the app, so it reports an Engine host that fails to boot. `src/sentry.ts` adds the route and loaded game to every event.

To add a page, create a folder for it under `src/pages` (such as `src/pages/party/PartyPage.tsx`) and add a `<Route>` for it in `src/layout/AppShell.tsx`. Build its URLs in `src/routes.ts`, and link with React Router's `Link`. A plain `<a href>` reloads the app and drops the loaded save.

Put anything another page could use in its own file: components under `src/components`, hooks under `src/hooks`. Use `App.useApp()` for notifications and modals. The shell provides one antd `App`, so a notification outlives the page that raised it.

Give every antd `Table` `scroll={{ x: 'max-content' }}`. A table without `scroll` grows past the window on phones.

Pages get navigation, the theme and the calculator URL from `src/host.ts`. `changeTheme` and `changeCalculatorUrl` write them to localStorage through `src/settings.ts`, under the keys the Blazor app used. The calculator presets live in `src/calculators.ts` and the release notes in `src/news.ts`.

Put a page's actions in `ButtonOrMenu` from `src/components`, not in a row of separate buttons, which runs off the screen on phones.
