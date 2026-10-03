# @pkhex-everywhere/engine

## 0.3.1

### Patch Changes

- bff935c: The Engine reads the version of the `PKHeX.Everywhere.PlugIns` reference to tell SDK 3 plug-ins from SDK 2. It runs only SDK 2 for now, so `plugins.register` lists an SDK 3 plug-in with `needsReinstall`, and `plugins.isSupported` returns `false` for it.

## 0.3.0

### Minor Changes

- 79bb0ce: `game.load` fails with the new `format-choice-required` error code when a save might be in a format PKHeX doesn't know, such as Pokémon Radical Red. The `EngineError` lists those formats in `candidates`. Pass one of their ids as `formatId` to load the save with it, or `pkhex` to load it with PKHeX's own detection.
- cb21c83: The save summary lists what the save supports in `capabilities`, and names its save format in `format` when PKHeX can't read it on its own. `game.version()` and the game events carry the format id. Calls behind a missing capability fail with the new `not-supported` error code, and `legality` in `pokemon.details()` is null when the save doesn't support it.
- 670a8ed: Add `unknownHeldItem` to `EditablePokemon` and the `unknown-species` error code, for Pokémon whose species or held item PKHeX can't identify, such as a ROM hack's own.

### Patch Changes

- c6812ee: `game.formats()` lists the save formats PKHeX doesn't know, such as ROM hacks, by id and name. `game.load()` takes an optional `formatId` that loads the save with that format and skips detection. An unknown id fails with `not-found`. Client methods and hook commands now carry the engine's doc comments, and `useLoadedGame().load()` takes `formatId` too.
- dde2b58: `game.load` opens Pokémon Unbound saves, with `format.id` set to `unbound`. Exporting one keeps its file size, including a flashcart's 16-byte RTC trailer.
- f11f9a8: `inventory.get()` lists items PKHeX can't identify, such as a ROM hack's own, with id 0 and a name like `Unknown item #79`. `inventory.setItem` can't change them. Edits to more than one pouch in a session no longer undo each other.

## 0.2.3

### Patch Changes

- 90d4434: Point the READMEs and npm pages at docs.pkhex-everywhere.fyi.

## 0.2.2

### Patch Changes

- 109cd2c: Pokémon caught in a Fast Ball or any later ball now show that ball's name instead of an unrelated item's.

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
