---
"@pkhex-everywhere/engine": minor
---

New `pokemon.read(bytes, version)` reads a Gen 3 or Gen 4 Pokémon from encrypted party or box bytes without a loaded save, and returns the same fields as `pokemon.details()` with `legality` set to `null`. Bytes that aren't a Pokémon fail with the new `bad-checksum` code.
