---
'@pkhex-everywhere/engine': minor
---

The save summary lists what the save supports in `capabilities`, and names its save format in `format` when PKHeX can't read it on its own. `game.version()` and the game events carry the format id. Calls behind a missing capability fail with the new `not-supported` error code, and `legality` in `pokemon.details()` is null when the save doesn't support it.
