---
"@pkhex-everywhere/engine": patch
---

`game.export()` now writes back only the Pokémon that changed. Exporting a Yellow, FireRed or Let's Go save with no edits used to rewrite stored stats and checksums of Pokémon nobody touched.
