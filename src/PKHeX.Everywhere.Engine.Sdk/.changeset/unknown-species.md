---
'@pkhex-everywhere/engine': minor
---

Pokémon summaries and details have a required `isUnknown` and `editable`. A species PKHeX has no id for, such as a ROM hack's own species, now has `speciesId: null` (`species: null` in details, and in party members) instead of 0, and is named like `Unknown (#706)`. Such a Pokémon isn't editable: show it with `pokemon.details()`, since `pokemon.edit()`, `pokemon.clone()` and `pokemon.update()` fail with `unknown-species`.
