---
"@pkhex-everywhere/engine": patch
---

New `game.progress()` returns the save's play time, gym badges and Pokédex seen and caught counts. Each is null when the engine can't read it for the game. ROM hack saves give only play time.
