---
"@pkhex-everywhere/engine": patch
---

New `box.list()` returns every box in the save, empty ones included, with its number, name and slot count. `name` is null when the save doesn't store box names, such as Generation 1 saves.
