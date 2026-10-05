---
"@pkhex-everywhere/engine": patch
---

`events.get()` now works for Generation 1 saves. PKHeX has no labels for them, so `flags` and `work` are empty, but `events.flag()`, `events.setFlag()` and `events.setWork()` reach every index up to `flagCount` and `workCount`. `hasEvents` is true for these saves.
