# CLAUDE.md

## Conventions

### Commits

Conventional Commits (`feat:`, `fix:`, `chore:`, ...). Single-line messages only, no body.

### Writing

Run the `unslop` skill on any text we produce: PR descriptions, issue text, UI copy, docs. Be brief.

### Code comments

**Do not add comments by default.** Only comment when the code makes a hacky or non-obvious decision a future reader couldn't infer. No comments restating what the code does, no section headers, no "added X" notes.

Exception: the public API of the npm packages gets doc comments, written for the people using them. That covers exported types and functions in `src/PKHeX.Everywhere.Engine.Sdk/packages`, plus the C# DTOs, events and handlers the engine codegen turns into TypeScript, whose `///` comments end up in the generated types. These comments show in editors and in the docs site's API reference.

### Layers

Domain logic goes in `PKHeX.Facade`, and the Engine stays thin: resolve Handles, call the Facade, map DTOs, report Topics. If an Engine handler needs a rule or a save-type check, add it to the Facade instead. See [ADR 0004](docs/adr/0004-domain-logic-lives-in-the-facade.md).

Only `PKHeX.Everywhere.RomHacks` knows about ROM hacks. The Facade learns about a save through its Save format, and an architecture test fails if anything but the host and the tests references `RomHacks`. See [ADR 0008](docs/adr/0008-rom-hacks-are-converted-at-the-save-boundary.md).

### Facade API

`src/PKHeX.Facade/PublicAPI.*.txt` track the Facade's public API, and the build fails when they're out of date. Add new members to `PublicAPI.Unshipped.txt`. Removing or changing a shipped member breaks plug-ins and bumps the plug-in SDK. See [ADR 0009](docs/adr/0009-a-breaking-change-to-the-facades-api-bumps-the-plug-in-sdk.md).

### Tests

New coverage goes in `PKHeX.Facade.Tests`, or in `PKHeX.Everywhere.Engine.Tests` when it tests the Engine. Only add an E2E test when [ADR 0001](docs/adr/0001-keep-the-e2e-suite-small.md) allows it.

### Naming

New projects use `PKHeX.Everywhere.*` and new npm packages use `@pkhex-everywhere/*`. Existing projects keep their names.

### Changesets

A PR that changes a published package (`@pkhex-everywhere/engine`, `react` or `plugin-sdk`) adds a changeset: run `npx changeset` in `src/PKHeX.Everywhere.Engine.Sdk` and commit the file. Engine changes that alter the runtime or the generated client count as changes to `engine`. Pre-1.0, breaking changes are `minor` bumps.

### Submodules

`external/PKHeX` and `external/PKHeX-Plugins` are submodules pointing at our forks. Stay as close to upstream as possible: prefer solving problems in this repo. When a change to a fork is unavoidable, don't make it silently; open an issue labelled `ready-for-human` describing the change so a human can handle it (and upstreaming).

## Agent skills

### Issue tracker

GitHub Issues in the separate repo `pkhex-web/issue-tracker` (always pass `-R pkhex-web/issue-tracker` to `gh issue`). See `docs/agents/issue-tracker.md`.

### Triage labels

Default five-role vocabulary (`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`). See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: one `CONTEXT.md` + `docs/adr/` at the repo root. See `docs/agents/domain.md`.
