---
'@pkhex-everywhere/engine': patch
---

`game.formats()` lists the save formats PKHeX doesn't know, such as ROM hacks, by id and name. `game.load()` takes an optional `formatId` that loads the save with that format and skips detection. An unknown id fails with `not-found`. Client methods now carry the engine's doc comments.
