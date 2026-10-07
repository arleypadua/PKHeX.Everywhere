---
"@pkhex-everywhere/engine": minor
---

`pokemon.read(bytes, version, formatId?)` and `catalog.names({ speciesIds, itemIds, formatId? })` take an optional ROM hack format id, such as `unbound`. With it, `pokemon.read` reads the hack's party bytes as `pokemon.details()` shows them in the hack's save, and `catalog.names` names the hack's own species and items.
