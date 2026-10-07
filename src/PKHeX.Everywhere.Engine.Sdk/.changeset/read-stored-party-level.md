---
"@pkhex-everywhere/engine": patch
---

`pokemon.read` returns the level stored in party bytes instead of computing it from EXP, so it matches what the game shows when a randomizer changes growth rates. Box bytes still get the EXP-derived level.
