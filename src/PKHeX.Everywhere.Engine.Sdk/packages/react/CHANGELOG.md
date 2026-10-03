# @pkhex-everywhere/react

## 0.4.0

### Minor Changes

- 794b7bc: `pokemon.options()` returns `locked`, the fields the save can't change, typed as the new `PokemonField` union. Gen 3 and Gen 4 saves lock `nature`, since it comes from the PID. An update that changes a locked field fails with `invalid-patch`. Gen 1 encounters now show their location as `(Unknown)` instead of an unrelated Alola name.

### Patch Changes

- Updated dependencies [a468f4c]
- Updated dependencies [794b7bc]
  - @pkhex-everywhere/engine@0.4.0

## 0.3.1

### Patch Changes

- Updated dependencies [bff935c]
  - @pkhex-everywhere/engine@0.3.1

## 0.3.0

### Patch Changes

- c6812ee: `game.formats()` lists the save formats PKHeX doesn't know, such as ROM hacks, by id and name. `game.load()` takes an optional `formatId` that loads the save with that format and skips detection. An unknown id fails with `not-found`. Client methods and hook commands now carry the engine's doc comments, and `useLoadedGame().load()` takes `formatId` too.
- Updated dependencies [c6812ee]
- Updated dependencies [79bb0ce]
- Updated dependencies [cb21c83]
- Updated dependencies [dde2b58]
- Updated dependencies [f11f9a8]
- Updated dependencies [670a8ed]
  - @pkhex-everywhere/engine@0.3.0

## 0.2.3

### Patch Changes

- 90d4434: Point the READMEs and npm pages at docs.pkhex-everywhere.fyi.
- Updated dependencies [90d4434]
  - @pkhex-everywhere/engine@0.2.3

## 0.2.2

### Patch Changes

- Updated dependencies [109cd2c]
  - @pkhex-everywhere/engine@0.2.2

## 0.2.1

### Patch Changes

- 92c350e: Document the public types, so editors and the API reference describe each type and field.
- 4fe8568: Require `@pkhex-everywhere/engine` from the matching minor as a peer, instead of any version.
- 92c350e: Link the READMEs and npm pages to the new documentation site.
- Updated dependencies [92c350e]
- Updated dependencies [55b1b68]
- Updated dependencies [92c350e]
  - @pkhex-everywhere/engine@0.2.1

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
