---
"@pkhex-everywhere/engine": minor
"@pkhex-everywhere/react": minor
---

Transfers take the new `unofficial` route between any two saves no game connects, such as Platinum to Ruby. It strips the moves, item, ball and ability the destination doesn't have, with the reason `notInGame`, and reports every other change with the reason `unofficial`. Each offer has a `route`, `TransferField` gains `level`, `nature`, `gender`, `shiny`, `language`, `originalTrainer`, `trainerId`, `originGame` and `metDate`, and a Pokémon PKHeX can't convert is refused with `conversionFailed`. `noRoute` is left for ROM hack formats and Let's Go.

`box.previewFile(bytes)` shows how a Pokémon file would arrive, with `unofficial: true` when no game can move it. `box.addFromFile(bytes, { allowUnofficial: true })` adds such a file; without the option it still fails with `conversion-failed`.
