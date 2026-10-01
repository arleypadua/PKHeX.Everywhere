---
name: run-web-app
description: Start and stop PKHeX.Web with its React dev server. Use when running the web app, checking a change in the browser, or taking a screenshot of a page.
---

# Run the web app

`src/PKHeX.Web.React/dev.sh` is the only entry point. It runs `dotnet watch` for PKHeX.Web and the Vite dev server, and cleans up both however it is stopped. Start PKHeX.Web through it every time, including for Blazor-only changes.

## 1. Start

From `src/PKHeX.Web.React`, run `npm start` as a background task. The first start builds the JS assets and installs packages, and the first build takes a minute or two.

The ports are fixed (5062 for the app, 5173 for Vite), so only one dev server can run on the machine. When `dev.sh` refuses to start:

- **"already running"** means this checkout's server is up. Use it.
- **"port … is in use"** means another checkout owns it, usually another agent's worktree. Leave it running. Tell the user, and check your change with an E2E test instead (see below).

Done when `curl -sf http://localhost:5062` succeeds. Open http://localhost:5062.

## 2. Stop

Run `./dev.sh stop` from `src/PKHeX.Web.React` as soon as you are done with the browser, and always before your final message.

Done when `src/PKHeX.Web.React/.dev.pid` is gone and `lsof -iTCP:5062 -sTCP:LISTEN` prints nothing.

## E2E tests

`src/PKHeX.Web.E2E` publishes a Release build and serves it on a random port, so it runs beside any dev server. Setup is in its README. Each run publishes PKHeX.Web, which is CPU-heavy and slow when other worktrees are building:

- Run one E2E command at a time and wait for it to finish. Send its output to a file you can read while it runs (`> e2e.log 2>&1`).
- When a run is slow, read its log. A second run only competes with the first for CPU and the publish folder.
