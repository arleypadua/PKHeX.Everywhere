# PKHeX.Web.E2E

Smoke tests that drive the Release build of PKHeX.Web in headless Chromium.

The test fixture publishes PKHeX.Web, serves it on a random local port and blocks every request to other hosts. Tests are tagged `Category=E2E`. `default.runsettings` filters them out when no `--filter` is given, so a plain `dotnet test` skips them. IDEs that honour `RunSettingsFilePath` skip them too; run them from the CLI.

## Run locally

Build the JS assets once:

```sh
cd src/PKHeX.Web/_js
npm ci
npm run build
```

Build the project and install Chromium (needs PowerShell):

```sh
dotnet build src/PKHeX.Web.E2E
pwsh src/PKHeX.Web.E2E/bin/Debug/net10.0/playwright.ps1 install --only-shell chromium
```

Run the suite:

```sh
dotnet test src/PKHeX.Web.E2E --filter Category=E2E
```

When a test fails, its trace, screenshot and browser console log are written to `src/PKHeX.Web.E2E/bin/Debug/net10.0/playwright-artifacts/`. Open a trace with `pwsh src/PKHeX.Web.E2E/bin/Debug/net10.0/playwright.ps1 show-trace <file>.zip` or at https://trace.playwright.dev.
