---
"@pkhex-everywhere/engine": patch
---

New `game.enableFormat(id)` turns on a save format the host registered off. Until then, `game.formats()` leaves it out, `game.load()` with its id fails with `not-found`, and a save it recognizes fails with `invalid-save`.
