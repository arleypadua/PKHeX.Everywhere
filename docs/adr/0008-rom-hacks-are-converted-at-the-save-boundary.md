# ROM hacks are converted at the save boundary

Only `PKHeX.Everywhere.RomHacks` knows that a save is a ROM hack. It converts hack values into PKHeX's model when it reads, and back when it writes. Everything above it treats the save like any other. Context: pkhex-web/issue-tracker#162.

This supersedes the parts of [ADR 0007](0007-rom-hack-saves-adapt-to-pkhex-types.md) about `IUnmappedValues`, `IUnmappedItems` and `IMoveList`. The rest of 0007 stands.

## Decision

- The Facade learns about a save only through its Save format: its Capabilities and its Game data source. Nothing in the Facade checks `Context` or `Generation` to decide what a save can store.
- The Engine and the UI read DTOs and Capabilities. They don't know a save is a hack.
- A species, item or move that PKHeX has no id or name for is Unknown.
  - An unknown item or move gets an id from a range PKHeX never uses. The range is private to `RomHacks`, and nothing above it decodes it. The id travels through `PKM.HeldItem`, `Move1` to `Move4` and `InventoryPouch` like any other id, and turns back into the raw index on write.
  - The Game data source names unknown ids, such as "Unknown item #61".
  - An unknown item or move can be changed or cleared where it already is, and is never offered anywhere else.
  - An unknown species has no PKHeX id. Its Pokémon is read-only: it can be moved, released and exported, but not edited or cloned.
- The Game data source lists the values a save can store, declares its Locked fields, and says whether stats are approximate. Official saves get a default built from PKHeX's data. `IMoveList` goes away.
- `IUnmappedValues`, `IUnmappedItems`, `Inventory.UnknownItems`, `ItemDefinition.Unmapped` and the Engine's merging of unknown items go away.

An architecture test in `PKHeX.Everywhere.Engine.Tests` fails when anything outside `RomHacks` references it. The host that registers the formats and the test projects are exempt.

## Why

With `IUnmappedValues`, `IUnmappedItems` and `IMoveList`, every layer had to know about hacks, and each one handled them differently. The Engine appended unknown items to its own DTO, SDK consumers read `speciesId: 0` as both "unknown" and "empty", and the web app guessed whether stats were approximate. The next hack would have touched the Facade, the Engine and the UI. Converting at the boundary means a new hack only adds a Save format.

## Rejected alternatives

- **Keep the side channels from ADR 0007.** Each new kind of unknown value would need another interface, and every caller would have to check it.
- **Decode the unknown id range above `RomHacks`.** It would leak the hack's encoding into the Facade. The Game data source names the ids instead.
