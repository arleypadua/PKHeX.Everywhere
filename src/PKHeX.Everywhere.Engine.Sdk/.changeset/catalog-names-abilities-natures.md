---
"@pkhex-everywhere/engine": minor
---

`catalog.names` now names abilities and natures, without a loaded save. Pass `abilityIds` and `natureIds`, the ids `pokemon.read` returns, and read `abilities` and `natures` back. Unknown ids get a placeholder such as `Unknown Ability 400`.
