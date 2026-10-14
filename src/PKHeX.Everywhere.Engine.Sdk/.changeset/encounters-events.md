---
'@pkhex-everywhere/engine': minor
'@pkhex-everywhere/react': minor
---

Add `encounters.events(species?)`, which lists the event Pokémon a save can hold, whatever game each names as its origin. A FireRed save now lists the WISHMKR Jirachi from Ruby, and a Gen 2 save lists the Gen 1 events that reach it through the Time Capsule. Each row is an `EventRow`, which is an `EncounterRow` plus `trainerName`, `trainerId` and `language`. Each of the three is null when the event doesn't fix it. Pass a row's `index` to `box.addEncounter`. Encounters from another save format, such as Colosseum gifts on a Ruby save, now convert to the save's format when added.
