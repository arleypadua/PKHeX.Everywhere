# E2E suite covers critical user journeys only

`PKHeX.Web.E2E` drives the published PKHeX.Web in headless Chromium. Browser tests are slow and brittle, so the suite covers critical user journeys only: load a save, browse it, edit a Pokémon, export, and edit items. It stays at about 10 tests or 3 minutes of CI job time, whichever comes first.

A new E2E test needs one of:

- a new critical user journey, or
- a regression that shipped and that `PKHeX.Facade.Tests` couldn't have caught.

All other coverage goes in `PKHeX.Facade.Tests`.

## Considered Options

- **Block outbound traffic in the browser context instead of a config switch.** Sentry's DSN and the Google Analytics ID are hardcoded in `Program.cs`, and there is no switch to turn them off. Each browser context aborts every request to a host other than the local one. The tests stay hermetic and don't send telemetry to production, and the app code doesn't change.
- **Test the Release publish output, not a Debug build.** In Debug, `Load.razor` auto-loads `emerald.sav` and redirects home, which hides the real load flow. Release also runs trimming and Release-only code paths that a Debug build skips.
