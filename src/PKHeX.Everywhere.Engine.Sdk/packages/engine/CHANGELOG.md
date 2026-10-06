# @pkhex-everywhere/engine

## 0.9.0

### Minor Changes

- 92d747a: Trades now follow the receiving game's rules. `trade.preview()` and `trade.commit()` apply trade evolutions, reset friendship to 70 on Generation 2 to 4 link trades, and revert Giratina, Shaymin and Rotom forms leaving Platinum. Platinum refuses with `bagFull` when its bag can't take back the Griseous Orb. New change reasons: `received`, `tradeEvolution`, `itemUsed` and `formReverted`. `TradeSaveChange` now has `save` and `label` instead of `species`, and new kinds `itemReturned` and `eventVar`.

## 0.8.1

### Patch Changes

- 45aa5c8: New `trade` calls move Pokémon between the loaded save and a partner save, the way link trades, the Time Capsule, Pal Park and Poké Transfer do. `trade.open()` loads the partner and reports the routes and empty box slots, `trade.preview()` shows what each Pokémon arrives as and refuses what the games refuse, and `trade.commit()` moves them and returns both saves. New `useTrade()` hook and `trade` topic.

## 0.8.0

### Minor Changes

- e6f46d6: `TrainerCard.gender` is now `null` for Red, Green, Blue, Yellow, Gold, Silver and Pokémon Stadium, which have no trainer gender, and `trainer.setGender` fails with `not-in-game` for them. `game.loadBlank` no longer throws for Gold, Silver and Crystal.

### Patch Changes

- c5cd974: New `box.list()` returns every box in the save, empty ones included, with its number, name and slot count. `name` is null when the save doesn't store box names, such as Generation 1 saves, Let's Go and ROM hacks.
- d03bb1c: `game.export()` now writes back only the Pokémon that changed. Exporting a Yellow, FireRed or Let's Go save with no edits used to rewrite stored stats and checksums of Pokémon nobody touched.
- 5bfab1b: Gen 3 and 4 saves no longer lock `nature`. `pokemon.update({ nature })` gives the Pokémon a new PID with that nature, keeping its gender, ability and shininess, and a Gen 3 Unown's letter.
- d8b8e64: Emerald Imperium no longer locks `ability`. A Pokémon's ability choices are its species' abilities, the only ones the save can store.
- 74c1996: Radical Red saves read Hakamo-o, which showed as Dhelmise. Red- and Blue-Striped Basculin no longer read as each other, and Ice and Shadow Rider Calyrex and Terastal Terapagos read as their form instead of the base form.

## 0.7.2

### Patch Changes

- f74dcdd: New `game.progress()` returns the save's play time, gym badges and Pokédex seen and caught counts. Each is null when the engine can't read it for the game. ROM hack saves give only play time.
- 40f058d: `events.get()` now works for Generation 1 saves. PKHeX has no labels for them, so `flags` and `work` are empty, but `events.flag()`, `events.setFlag()` and `events.setWork()` reach every index up to `flagCount` and `workCount`. `hasEvents` is true for these saves.
- 73a0afe: New `trainer.badges()` lists the gym badges by name with whether each is earned, and `trainer.setBadges(earned)` sets them. Both work on Generation 1, 2 and 3 saves, but not ROM hacks. `useTrainer()` gains `setBadges`.

## 0.7.1

### Patch Changes

- 56d8b03: New `game.types()` names the type ids in `EditablePokemon.types`, Normal (0) to Fairy (17), in every save. Gen 1 and 2 saves now give those same ids in `types`, where they used to give the games' own: a Charmander in Yellow is `[9]` (Fire), not `[20]`.

## 0.7.0

### Minor Changes

- 4118f50: `pokemon.update`, `pokemon.edit` and `pokemon.clone` accept Unbound's Shadow Warrior, Zygarde Cell and Zygarde Core. Their `speciesId` is an id the save defines, so a sprite lookup by id can miss, and `isUnknown` is false. `isUnknown` now means neither PKHeX nor the save knows the species.

### Patch Changes

- de5202d: New `game.enableFormat(id)` turns on a save format the host registered off. Until then, `game.formats()` leaves it out, `game.load()` with its id fails with `not-found`, and a save it recognizes fails with `invalid-save`.
- 344875d: A Gen 3 save whose party or boxes hold a species or move the base game doesn't have, such as a Run & Bun save, now fails to load with `invalid-save` instead of loading as the base game. Loading it with the `pkhex` format still works.
- 5cd1cac: Unbound's Shadow Warrior, Zygarde Cell and Zygarde Core show by name, with their own types, stats, ability and gender, and `species.list` offers them. Radical Red's Chillet and Galarian Mime Jr. show by name. They all stay read-only, with `isUnknown` true.
- e850702: An Emerald Imperium save's bag reads and edits through `inventory.get` and `inventory.setItem`: Items, MegaStones, KeyItems, Balls, TMHMs and Berries, up to 999 of an item. Imperium's own items show as Unknown and can be changed or removed where they are.
- 3bbcace: An Emerald Imperium save loads with the `emerald-imperium` format instead of as Emerald, without a format id or `game.enableFormat`. `game.formats()` lists it.
- 4888600: An Emerald Imperium 1.x save loads. Its party and 14 boxes list, a Pokémon's level, nature, moves, held item, nickname and ball can be edited, and export keeps every byte it didn't change. Imperium's own forms show by name and are read-only. Its own moves and items show as Unknown.
- fda0ba5: An Emerald Imperium save shows the trainer's name, gender, IDs and money, and the name, gender and money can be edited. A Gen 3 ROM hack save shows its trainer ID and secret ID as Gen 3 does, not in the six-digit format.
- 563b057: Radical Red saves read Mime Jr., Porygon-Z, Type: Null, Sirfetch'd, Mr. Rime and the female and Galarian variants that showed as unknown. Forms such as Paldean Tauros, Ogerpon's masks, Pumpkaboo's sizes and Deerling's seasons read as their form instead of the base form.

## 0.6.0

### Minor Changes

- 9781875: The engine runs plug-ins built against plug-in SDK 3 only. `plugins.isSupported` returns false for an SDK 2 plug-in, `plugins.register` lists it with `needsReinstall`, and `plugins.newestCompatible` picks the newest version with `Sdk: 3`.
- bf74352: Unknown items, such as a ROM hack's own items, now have ids of their own. `OwnedItem` gets `isUnknown`, and `inventory.setItem` can change the count of an unknown item or remove it in its pouch. `pokemon.options()` returns `heldItems`, typed as the new `ItemChoice` with `isUnknown`, which lists the unknown item only for the Pokémon already holding it. `EditablePokemon.unknownHeldItem` is replaced by `heldItemIsUnknown`, and setting `heldItem` to 0 now removes an unknown held item.
- a9062a7: Move slots have a required `isUnknown`. In a ROM hack save, a move PKHeX has no id for now shows in its slot as `Unknown move #n` instead of an empty slot. It can stay in that slot, be replaced or be cleared, and clearing it removes it from the save.
- 52d8956: Pokémon summaries and details have a required `isUnknown` and `editable`. A species PKHeX has no id for, such as a ROM hack's own species, now has `speciesId: null` (`species: null` in details, and in party members) instead of 0, and is named like `Unknown (#706)`. Such a Pokémon isn't editable: show it with `pokemon.details()`, since `pokemon.edit()`, `pokemon.clone()` and `pokemon.update()` fail with `unknown-species`.

## 0.5.0

### Minor Changes

- f327518: `game.get()` returns `statsApproximate`, which is true when computed stats may differ from the game's, as in ROM hacks. Unbound and Radical Red now only offer what their saves can store: their own species, items, moves and 26 balls, and Gen 3 languages and origin games. Both lock `nature`, `ability` and `gender`. Radical Red names met locations with FireRed's places. Unbound shows them as `Location #n` and locks `metLocation`.

## 0.4.0

### Minor Changes

- 794b7bc: `pokemon.options()` returns `locked`, the fields the save can't change, typed as the new `PokemonField` union. Gen 3 and Gen 4 saves lock `nature`, since it comes from the PID. An update that changes a locked field fails with `invalid-patch`. Gen 1 encounters now show their location as `(Unknown)` instead of an unrelated Alola name.

### Patch Changes

- a468f4c: In Unbound saves, Antique Sinistea and Polteageist, female Indeedee, Alcremie with a sweet, Gigantamax Pokémon and the Manaphy Egg now load as their species instead of `Unknown (#n)`, and can be edited. Radical Red's Gigantamax Pokémon from the official games are now flagged as Gigantamax too.

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
