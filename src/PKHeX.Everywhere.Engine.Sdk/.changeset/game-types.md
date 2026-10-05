---
"@pkhex-everywhere/engine": patch
---

New `game.types()` names the type ids in `EditablePokemon.types`, Normal (0) to Fairy (17), in every save. Gen 1 and 2 saves now give those same ids in `types`, where they used to give the games' own: a Charmander in Yellow is `[9]` (Fire), not `[20]`.
