---
"@pkhex-everywhere/engine": patch
---

An Emerald Imperium save fails to load with `invalid-save` instead of loading as Emerald. The `emerald-imperium` format is registered off, and `game.formats()` leaves it out.
