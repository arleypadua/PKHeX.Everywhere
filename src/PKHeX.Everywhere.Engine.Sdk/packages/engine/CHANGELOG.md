# @pkhex-everywhere/engine

## 0.2.1

### Patch Changes

- 92c350e: Document the public types, so editors and the API reference describe each type and field.
- 55b1b68: Engines release their host listeners when nothing subscribes to them and once boot settles, so engines dropped by StrictMode or HMR no longer stay registered.
- 92c350e: Link the READMEs and npm pages to the new documentation site.

## 0.2.0

### Minor Changes

- fb0df12: Binary inputs take `Uint8Array`, `ArrayBuffer`, `Blob` or `File`, and binary outputs return `Uint8Array`. `game.load(data, fileName?)` takes the name from a `File`. Generated methods and hooks document whether they need a loaded save or an open draft. A missing draft fails with the new `no-draft` error code instead of `not-found`.
- 938f28c: `createEngine()` starts downloading the runtime as soon as it's created in a browser (`{ lazy: true }` opts out) and reports `engine.status` with file download progress through `engine.onStatusChange`. `wasmHost()` returns one shared runtime per page, so several engines on a page all get changes and events.
- eab17a2: `react` adds `useEngineStatus()` and `<RequireGame fallback>`, and `useQuery` rewrites `no-save` and `no-draft` messages to name the call and the fix. `engine` no longer exports `createClient`, `affects`, `queryTopics`, `Invoke`, `AssemblyExports` or `EngineExports`, and `engine.call` is gone.

### Patch Changes

- 38e32c9: Exporting or editing no longer rewrites the handler of Pokémon already in the save. `game.file` now reports only `Party` and `Box`.

## 0.1.0

### Minor Changes

- c1bc8fa: First release.
