---
"@pkhex-everywhere/engine": minor
---

Add the `team` namespace for the registered teams of Pokémon Stadium, Pocket Monsters Stadium and Stadium 2 saves. `team.list()` lists them with their cup and how many slots are filled, and returns `[]` for other saves. `team.get(team)` returns a team's Pokémon, `team.place(from, to)` copies a Pokémon into a team slot, and `team.clear(at)` empties a slot and moves the rest up. `PokemonHandle` gains the `team` source and a `team` number, so the `pokemon` calls read and edit team members. Changes notify the new `team` topic. `game.loadBlank` now makes blank Stadium saves.
