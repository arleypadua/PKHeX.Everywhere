---
'@pkhex-everywhere/engine': patch
---

`inventory.get()` lists items PKHeX can't identify, such as a ROM hack's own, with id 0 and a name like `Unknown item #79`. `inventory.setItem` can't change them. Edits to more than one pouch in a session no longer undo each other.
