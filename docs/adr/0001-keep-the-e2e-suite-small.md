# Keep the E2E suite small

`PKHeX.Web.E2E` drives the production build in a real browser. That is slow and brittle compared to `PKHeX.Facade.Tests`, so the E2E suite only covers critical user journeys: load a save, browse it, edit a Pokémon, export, and edit items. The cap is about 10 tests or 3 minutes of CI job time, whichever comes first.

A new E2E test needs one of:

- a new critical user journey, or
- a regression that shipped and that facade tests couldn't have caught.

All other coverage goes in `PKHeX.Facade.Tests`. If the cap is reached, drop or merge a test before adding one.

## Why block outbound network traffic

`index.html` loads AdSense in every build, and a build flag could still miss a service. The tests abort every request to a host other than the local one. This keeps them hermetic and out of production telemetry without app changes. Blocking at the browser catches any third-party call.

## Why the production build

Dev builds auto-load `emerald.sav` and redirect home, which would hide the real load flow. `npm run build` also publishes the Engine host in Release, so the tests exercise trimming, which is what users get.
