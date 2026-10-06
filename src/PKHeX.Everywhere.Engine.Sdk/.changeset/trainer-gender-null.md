---
"@pkhex-everywhere/engine": minor
---

`TrainerCard.gender` is now `null` for Red, Blue, Yellow, Gold and Silver, which have no trainer gender, and `trainer.setGender` fails with `not-in-game` for them. `game.loadBlank` no longer throws for Gold, Silver and Crystal.
