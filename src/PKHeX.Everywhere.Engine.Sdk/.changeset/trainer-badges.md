---
"@pkhex-everywhere/engine": patch
"@pkhex-everywhere/react": patch
---

New `trainer.badges()` lists the gym badges by name with whether each is earned, and `trainer.setBadges(earned)` sets them. Both work on Generation 1, 2 and 3 saves, but not ROM hacks. `useTrainer()` gains `setBadges`.
