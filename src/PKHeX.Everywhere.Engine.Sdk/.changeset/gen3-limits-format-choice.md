---
'@pkhex-everywhere/engine': patch
---

A Gen 3 save holding Pokémon or moves its game doesn't have now fails with `format-choice-required` and no `candidates`, instead of `invalid-save`. `EngineError.pkhexGame` names the game loading with `pkhex` reads the save as.
