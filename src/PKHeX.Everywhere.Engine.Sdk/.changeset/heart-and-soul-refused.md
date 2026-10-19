---
"@pkhex-everywhere/engine": patch
---

A Pokémon Heart & Soul save fails to load with `invalid-save` instead of loading as Emerald. The `heart-and-soul` format is registered off, and `game.formats()` leaves it out.
