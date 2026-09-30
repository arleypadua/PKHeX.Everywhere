# Keep the E2E suite small

`PKHeX.Web.E2E` drives the published app in a real browser. That is slow and brittle compared to `PKHeX.Facade.Tests`, so the E2E suite only covers critical user journeys: load a save, browse it, edit a Pokémon, export, and edit items. The cap is about 10 tests or 3 minutes of CI job time, whichever comes first.

A new E2E test needs one of:

- a new critical user journey, or
- a regression that shipped and that facade tests couldn't have caught.

All other coverage goes in `PKHeX.Facade.Tests`. If the cap is reached, drop or merge a test before adding one.

## Why block outbound network traffic

The Sentry DSN and Google Analytics ID are hardcoded in `src/PKHeX.Web/Program.cs`, with no config switch. The tests abort every request to a host other than the local one. This keeps them hermetic and out of production telemetry without app changes. Adding a config switch was rejected: it changes app code only for tests, and a missed service would still leak. Blocking at the browser catches any third-party call. Firebase is turned off at build time with `VITE_FIREBASE_ENABLED=false`.

## Why a Release build

In DEBUG builds `Load.razor` auto-loads `emerald.sav` and redirects home, which would hide the real load flow. Release also exercises trimming and other Release-only code paths, which is what users get.
