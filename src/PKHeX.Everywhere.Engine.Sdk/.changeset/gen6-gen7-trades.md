---
"@pkhex-everywhere/engine": minor
---

`transfer` takes two Gen 6 saves or two Gen 7 saves by the `link` route. `transfer.preview` refuses a Pokémon with a move, held item or ability the receiving game doesn't have, with the new `notInGame` reason, such as an Omega Ruby move traded to X. `game.load` and `transfer.open` now read a blank Gen 6 or 7 save exported from `game.loadBlank`.
