# ROM hack saves adapt to PKHeX's own types

ROM hack saves load through PKHeX's extension points instead of new Facade interfaces. A CFRU save is a `SaveFile` subclass and a CFRU Pokémon is a `PKM` subclass that reports the Gen 9 context. Context: pkhex-web/issue-tracker#143, gate pkhex-web/issue-tracker#146.

[ADR 0008](0008-rom-hacks-are-converted-at-the-save-boundary.md) supersedes the parts about `IUnmappedValues`, `IUnmappedItems` and `IMoveList`.

## Decision

The adapter approach holds. The gate loaded OpenHome's Unbound 2.0 save through `Game.LoadFrom` and the Engine, with no change to the Engine and three changes to the Facade:

- `ISaveFormat`, a save format that detects a file as `Certain`, `Possible` or `No`.
- `SaveFormats`, the registry the host fills at startup.
- `Game.LoadFrom` asks the registry for a `Certain` match before PKHeX's own detection.

`PKHeX.Everywhere.RomHacks` holds the formats. `Cfru` has the save slots, block checksums, box stream, party, trainer and money. `Cfru.Unbound` has the signatures and the species table. The move table is shared by both hacks, so it lives in `Cfru`.

The Engine tests in `UnboundSaveTests` cover the gate:

- The party and all 25 boxes list with the right species, levels and moves.
- Pokémon details open for party and box Pokémon.
- Edits to moves, IVs and the held item survive export and reload, with valid checksums.
- Export without edits returns the file byte for byte.
- The vanilla FireRed fixture still loads as `SAV3FRLG`.

## Features that don't apply to a hack

- **Legality** runs and reports "Internal error" as invalid. The save doesn't change.
- **Showdown** works.
- **Encounters** list Scarlet and Violet only. Searching FireRed fails with `bad-arguments`, and adding a Scarlet or Violet encounter fails because the Pokémon type doesn't match. The save doesn't change.
- **AutoLegality** has side effects. It reports success and writes back a Pokémon with a zero PID, a blank OT and a garbage nickname. ALM builds a Pokémon from PKHeX's encounter data, which no hack has, so no `PKM` override can fix it. #147 must turn it off, plug-in actions included, before #150 registers the format in the host.

## What the PKM overrides do

These are rules of the format, not workarounds for the Facade:

- Species, form, moves and the held item translate between hack indices and national or modern IDs. Writing back the value a getter returned keeps the raw index, so duplicate indices (Unbound has four Gourgeist) and unmapped values survive edits to other fields.
- An unmapped species or held item reads as 0. The Pokémon reports the raw value through the Facade's `IUnmappedValues`, so the Facade shows it as "Unknown (#n)" or "Unknown item #n", keeps it in the party and boxes, and refuses to edit a Pokémon with an unknown species.
- PKHeX counts the party by species, so the save overrides `SetPartySlotAtIndex` to count an unmapped member.
- The save lists its held items through `HeldItems` and its moves through the Facade's `IMoveList`. Species come from the personal table's `IsSpeciesInGame`, which the Facade checks for every save.
- The party checksum is always written as 0.
- An empty party slot keeps "no mail" (`0xFF`), and the save doesn't compute party stats for a blank. Without this, export changed bytes in empty party slots.
- Box Pokémon expand from 58 bytes to the party layout and compress back without loss. PP isn't stored in boxes, so it is refilled with Gen 9 values on load.
- The bag is a `PlayerBag` whose pouches translate item indices to modern IDs. An unmapped item stays out of the pouch's items and keeps its slot, and the pouch reports it through the Facade's `IUnmappedItems`, so the Facade lists it as "Unknown item #n". Each pocket's items are the hack's items that modern games keep in that pocket.
- CFRU saves its 450 main, 75 key, 50 ball, 128 TM and 75 berry slots as one run: block 13 from `0xAD8`, then sector 30. The main pocket ends in sector 30 at `0x1F0`, where the key items start. The key items pocket isn't editable yet.

## Known gaps for later slices

- Max PP and base stats come from national data, not the hack.
- The save summary still reports FireRed and generation 9. Since #147 it also names the save format, which the UI shows instead.

## Rejected alternatives

- **Facade interfaces for saves and Pokémon.** Every Facade class would need a second path. The gate showed PKHeX's types are enough. #148 and #149 added three narrow ones, `IUnmappedValues`, `IMoveList` and `IUnmappedItems`, for what PKHeX's types can't say.
- **PKHeX's `CustomSaveReaders`.** It can't express a `Possible` match or a user's choice of format, which Radical Red needs.
