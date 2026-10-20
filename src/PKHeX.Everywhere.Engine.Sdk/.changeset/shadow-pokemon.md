---
"@pkhex-everywhere/engine": minor
---

`pokemon.details` reports shadow Pokémon in Colosseum and XD saves: `isShadow`, `shadowId` and `purification`, the heart gauge. They are null in other saves. `pokemon.update` takes `purification` to set the heart gauge of a shadow Pokémon, and `isShadow: false` to purify it.

Blank Colosseum and XD saves from `game.loadBlank` can now be exported and loaded back.
