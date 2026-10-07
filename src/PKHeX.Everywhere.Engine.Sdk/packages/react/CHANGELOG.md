# @pkhex-everywhere/react

## 0.18.0

### Minor Changes

- db3c07e: A transfer to an older game keeps a copy of the Pokémon, so a later transfer forward restores what the older game dropped. Each offer has `keepsCopy`, and `transfer.commit` returns a `keptCopy` for each Pokémon that moved to an older game. Pass stored copies in the offer's new `keptCopies`: a copy with the Pokémon's `identityKey` puts back its ball, met data, origin game, ribbons and ability, with the new reason `restored`. `PokemonSummary` and `PokemonPreview` gain `identityKey`, built from the PID, trainer ID and secret ID, and null in Gen 1 and 2.

### Patch Changes

- Updated dependencies [db3c07e]
  - @pkhex-everywhere/engine@0.18.0

## 0.17.0

### Minor Changes

- 881c351: The `trade` namespace is now `transfer`. `trade.*` calls become `transfer.*`, `useTrade` becomes `useTransfer`, every `Trade*` type becomes `Transfer*`, `TradedPokemon` becomes `TransferredPokemon`, the `trade` topic becomes `transfer`, and the `trade-refused` and `no-trade` errors become `transfer-refused` and `no-transfer`. The `link` route and the `link` and `tradeEvolution` change reasons keep their names.
- 2251534: Transfers take the new `unofficial` route between any two saves no game connects, such as Platinum to Ruby. It strips the moves, item, ball and ability the destination doesn't have, with the reason `notInGame`, and reports every other change with the reason `unofficial`. Each offer has a `route`, `TransferField` gains `level`, `nature`, `gender`, `shiny`, `language`, `originalTrainer`, `trainerId`, `originGame` and `metDate`, and a Pokémon PKHeX can't convert is refused with `conversionFailed`. `noRoute` is left for ROM hack formats and Let's Go.
  
  `box.previewFile(bytes)` shows how a Pokémon file would arrive, with `unofficial: true` when no game can move it. `box.addFromFile(bytes, { allowUnofficial: true })` adds such a file; without the option it still fails with `conversion-failed`.

### Patch Changes

- Updated dependencies [1572d36]
- Updated dependencies [881c351]
- Updated dependencies [2251534]
  - @pkhex-everywhere/engine@0.17.0

## 0.16.0

### Patch Changes

- Updated dependencies [9dc40ec]
- Updated dependencies [43d34e0]
  - @pkhex-everywhere/engine@0.16.0

## 0.15.0

### Patch Changes

- Updated dependencies [c9c1738]
  - @pkhex-everywhere/engine@0.15.0

## 0.14.0

### Patch Changes

- Updated dependencies [9e75b2b]
  - @pkhex-everywhere/engine@0.14.0

## 0.13.0

### Patch Changes

- Updated dependencies [47ae851]
- Updated dependencies [ee36074]
  - @pkhex-everywhere/engine@0.13.0

## 0.12.0

### Patch Changes

- Updated dependencies [c4eb3f1]
- Updated dependencies [d03fb4d]
  - @pkhex-everywhere/engine@0.12.0

## 0.11.0

### Patch Changes

- Updated dependencies [f8b9dc7]
  - @pkhex-everywhere/engine@0.11.0

## 0.10.0

### Patch Changes

- Updated dependencies [db46550]
  - @pkhex-everywhere/engine@0.10.0

## 0.9.0

### Patch Changes

- Updated dependencies [92d747a]
  - @pkhex-everywhere/engine@0.9.0

## 0.8.1

### Patch Changes

- 45aa5c8: New `trade` calls move Pokémon between the loaded save and a partner save, the way link trades, the Time Capsule, Pal Park and Poké Transfer do. `trade.open()` loads the partner and reports the routes and empty box slots, `trade.preview()` shows what each Pokémon arrives as and refuses what the games refuse, and `trade.commit()` moves them and returns both saves. New `useTrade()` hook and `trade` topic.
- Updated dependencies [45aa5c8]
  - @pkhex-everywhere/engine@0.8.1

## 0.8.0

### Patch Changes

- Updated dependencies [c5cd974]
- Updated dependencies [d03bb1c]
- Updated dependencies [5bfab1b]
- Updated dependencies [d8b8e64]
- Updated dependencies [74c1996]
- Updated dependencies [e6f46d6]
  - @pkhex-everywhere/engine@0.8.0

## 0.7.2

### Patch Changes

- 73a0afe: New `trainer.badges()` lists the gym badges by name with whether each is earned, and `trainer.setBadges(earned)` sets them. Both work on Generation 1, 2 and 3 saves, but not ROM hacks. `useTrainer()` gains `setBadges`.
- Updated dependencies [f74dcdd]
- Updated dependencies [40f058d]
- Updated dependencies [73a0afe]
  - @pkhex-everywhere/engine@0.7.2

## 0.7.1

### Patch Changes

- Updated dependencies [56d8b03]
  - @pkhex-everywhere/engine@0.7.1

## 0.7.0

### Patch Changes

- de5202d: New `game.enableFormat(id)` turns on a save format the host registered off. Until then, `game.formats()` leaves it out, `game.load()` with its id fails with `not-found`, and a save it recognizes fails with `invalid-save`.
- Updated dependencies [de5202d]
- Updated dependencies [344875d]
- Updated dependencies [4118f50]
- Updated dependencies [5cd1cac]
- Updated dependencies [e850702]
- Updated dependencies [3bbcace]
- Updated dependencies [4888600]
- Updated dependencies [fda0ba5]
- Updated dependencies [563b057]
  - @pkhex-everywhere/engine@0.7.0

## 0.6.0

### Minor Changes

- bf74352: Unknown items, such as a ROM hack's own items, now have ids of their own. `OwnedItem` gets `isUnknown`, and `inventory.setItem` can change the count of an unknown item or remove it in its pouch. `pokemon.options()` returns `heldItems`, typed as the new `ItemChoice` with `isUnknown`, which lists the unknown item only for the Pokémon already holding it. `EditablePokemon.unknownHeldItem` is replaced by `heldItemIsUnknown`, and setting `heldItem` to 0 now removes an unknown held item.

### Patch Changes

- Updated dependencies [9781875]
- Updated dependencies [bf74352]
- Updated dependencies [a9062a7]
- Updated dependencies [52d8956]
  - @pkhex-everywhere/engine@0.6.0

## 0.5.0

### Patch Changes

- Updated dependencies [f327518]
  - @pkhex-everywhere/engine@0.5.0

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
