# @pkhex-everywhere/react

## 0.2.0

### Minor Changes

- fb0df12: Binary inputs take `Uint8Array`, `ArrayBuffer`, `Blob` or `File`, and binary outputs return `Uint8Array`. `game.load(data, fileName?)` takes the name from a `File`. Generated methods and hooks document whether they need a loaded save or an open draft. A missing draft fails with the new `no-draft` error code instead of `not-found`.
- 938f28c: `createEngine()` starts downloading the runtime as soon as it's created in a browser (`{ lazy: true }` opts out) and reports `engine.status` with file download progress through `engine.onStatusChange`. `wasmHost()` returns one shared runtime per page, so several engines on a page all get changes and events.
- eab17a2: `react` adds `useEngineStatus()` and `<RequireGame fallback>`, and `useQuery` rewrites `no-save` and `no-draft` messages to name the call and the fix. `engine` no longer exports `createClient`, `affects`, `queryTopics`, `Invoke`, `AssemblyExports` or `EngineExports`, and `engine.call` is gone.

### Patch Changes

- Updated dependencies [fb0df12]
- Updated dependencies [938f28c]
- Updated dependencies [38e32c9]
- Updated dependencies [eab17a2]
  - @pkhex-everywhere/engine@0.2.0

## 0.1.0

### Minor Changes

- c1bc8fa: First release.

### Patch Changes

- Updated dependencies [c1bc8fa]
  - @pkhex-everywhere/engine@0.1.0
