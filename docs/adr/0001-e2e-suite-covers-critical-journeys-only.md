# E2E suite covers critical user journeys only

`PKHeX.Web.E2E` drives the production build of the web app (`src/PKHeX.Web.React/dist`) in headless Chromium. Browser tests are slow and brittle, so the suite covers critical user journeys only: load a save, browse it, edit a Pokémon, export, and edit items. It stays at about 10 tests or 3 minutes of CI job time, whichever comes first.

A new E2E test needs one of:

- a new critical user journey, or
- a regression that shipped and that `PKHeX.Facade.Tests` couldn't have caught.

All other coverage goes in `PKHeX.Facade.Tests`.

## Considered Options

- **Block outbound traffic in the browser context instead of a config switch.** `index.html` loads AdSense in every build, and a build flag could still miss a service. Each browser context aborts every request to a host other than the local one. The tests stay hermetic and don't send telemetry to production, and the app code doesn't change.
- **Test `npm run build` output, not the dev server.** Dev builds auto-load `emerald.sav` and redirect home, which hides the real load flow. The production build also publishes the Engine host in Release, so trimming runs.
