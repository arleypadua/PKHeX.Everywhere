---
"@pkhex-everywhere/engine": minor
---

`game.get` and `game.version` name Colosseum, XD and Box Ruby & Sapphire saves as those games instead of Emerald, with ids 62, 63 and 61. `game.blankVersions` lists Colosseum, XD and Box on their own, and the shared Colosseum/XD entry (15) is gone, so `game.loadBlank(15)` now fails with `not-found`. `pokemon.read` with version 15 fails with `bad-arguments`, as any combined version does.
