---
"@pkhex-everywhere/engine": minor
---

New `speciesIndex` on `PokemonSummary`, `EditablePokemon`, `PokemonOverview` and `PartyMember`: the species as the save stores it, such as Gen 3's internal index or a ROM hack's own index. It's set for unknown species too, so it can index the game's own tables, such as its sprites.
