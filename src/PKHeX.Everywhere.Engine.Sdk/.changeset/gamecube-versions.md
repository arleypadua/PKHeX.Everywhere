---
"@pkhex-everywhere/engine": minor
---

Colosseum, XD and Box Ruby & Sapphire saves are no longer reported as Emerald. `game.get` and `game.version` name them as those games, with ids 62, 63 and 61, and `game.file` reports `COLO`, `XD` or `RSBOX`. `encounters.versions` defaults to Colosseum/XD (15) for a Colosseum or XD save.

`game.blankVersions` lists Colosseum, XD and Box separately. The shared Colosseum/XD entry (15) is gone, so `game.loadBlank(15)` fails with `not-found`, and `pokemon.read` with version 15 fails with `bad-arguments`, like any combined version.
