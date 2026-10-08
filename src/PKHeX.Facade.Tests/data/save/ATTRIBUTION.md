# Test save attribution

`unbound.sav` (Pokémon Unbound 2.0), `radicalred.sav` (Pokémon Radical Red) and `firered.sav` (Pokémon FireRed) come from [OpenHome](https://github.com/andrewbenington/OpenHome) by Andrew Benington, licensed under GPL-3.0. They were copied unchanged from `src/core/save/__test__/save-files` at commit `9f1f1f623a8da9c02caaceefbcb4340623741596`.

`unbound-unknown-species.sav` is `unbound.sav` with the Pokémon in box 23, slot 19 changed to species index 706 (Shadow Warrior), which PKHeX has no species for.

`imperium.sav` (Emerald Imperium 1.3) is `Emerald Imperium 1.3.sav` from [Dynamic-Calc-Decomps](https://github.com/hzla/Dynamic-Calc-Decomps) by hzla, copied unchanged from `cypress/fixtures/saves` at commit `bebf34ffe8827089b355e7534df48ba95eecc3c6` (blob `fa56843dfd9c229fc53bcae3b8e8b6fd99791d5c`). That repository has no license.

`emerald-legacy.sav` (Pokémon Emerald Legacy) was contributed by a player of the hack and anonymised:
every trainer name in it, including the ones in its Pokémon and its trainer name records, was replaced
with a same-length placeholder, and the sector checksums were recomputed. Ids and PIDs are untouched,
since a Gen 3 Pokémon's substructures are encrypted with its PID and OT id.

`unbound-2.1.sav` (Pokémon Unbound 2.1.1.1) is `Pokemon - Unbound (v2.1.1.1).srm` from [Unbound-2.1.1.1-living-dex](https://github.com/RickHalden/Unbound-2.1.1.1-living-dex) by RickHalden, copied unchanged at commit `6337b2131e829cbc539c0a179e3a31e8fb24a9bc` (blob `2c647823d76d2685484b1dbdc9e2b686e7d03b73`). That repository has no license.
