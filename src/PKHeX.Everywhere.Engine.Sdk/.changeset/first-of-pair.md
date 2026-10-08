---
"@pkhex-everywhere/engine": patch
---

A save that can't tell paired games apart now names a game from its own pair. `game.version()` and the default in `encounters.versions()` return the first game of the pair. A Ruby or Sapphire save says Ruby, and a Diamond or Pearl save says Diamond. Before, they named the generation's default game, such as Emerald or SoulSilver.
