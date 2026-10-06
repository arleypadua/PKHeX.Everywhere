---
"@pkhex-everywhere/engine": patch
---

Gen 3 and 4 saves no longer lock `nature`. `pokemon.update({ nature })` gives the Pokémon a new PID with that nature, keeping its gender, ability and shininess, and a Gen 3 Unown's letter.
