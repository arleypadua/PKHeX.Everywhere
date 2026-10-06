---
"@pkhex-everywhere/engine": minor
---

`TrainerCard.gender` is now `null` for Red, Green, Blue, Yellow, Gold, Silver and Pokémon Stadium, which have no trainer gender, and `trainer.setGender` fails with `not-in-game` for them. `game.loadBlank` no longer throws for Gold, Silver and Crystal.
