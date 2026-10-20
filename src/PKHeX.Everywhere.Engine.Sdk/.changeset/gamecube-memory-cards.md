---
"@pkhex-everywhere/engine": patch
---

`game.load` opens a GameCube memory card and loads its Colosseum, XD or Box save, and `game.export` returns the whole card. A Colosseum or XD file with no story saved fails with `invalid-save`.
