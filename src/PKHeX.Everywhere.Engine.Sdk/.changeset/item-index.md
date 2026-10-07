---
"@pkhex-everywhere/engine": minor
---

New `heldItemIndex` on `EditablePokemon`, and `index` on `OwnedItem`, `AddableItem` and `ItemChoice`: the item as the save stores it, such as a ROM hack's own index. It's set for unknown items too, so it can index the game's own tables, such as its item icons.
