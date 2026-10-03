---
'@pkhex-everywhere/engine': minor
'@pkhex-everywhere/react': minor
---

`pokemon.options()` returns `locked`, the fields the save can't change, typed as the new `PokemonField` union. Gen 3 and Gen 4 saves lock `nature`, since it comes from the PID. An update that changes a locked field fails with `invalid-patch`.
