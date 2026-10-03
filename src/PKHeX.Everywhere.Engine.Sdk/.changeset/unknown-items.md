---
'@pkhex-everywhere/engine': minor
'@pkhex-everywhere/react': minor
---

Unknown items, such as a ROM hack's own items, now have ids of their own. `OwnedItem` gets `isUnknown`, and `inventory.setItem` can change the count of an unknown item or remove it in its pouch. `pokemon.options()` returns `heldItems`, typed as the new `ItemChoice` with `isUnknown`, which lists the unknown item only for the Pokémon already holding it. `EditablePokemon.unknownHeldItem` is replaced by `heldItemIsUnknown`, and setting `heldItem` to 0 now removes an unknown held item.
