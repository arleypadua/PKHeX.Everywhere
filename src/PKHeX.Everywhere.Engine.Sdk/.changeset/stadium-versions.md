---
"@pkhex-everywhere/engine": minor
---

Stadium, Pocket Monsters Stadium and Stadium 2 saves are no longer reported as Red or Crystal. `game.get` and `game.version` name them as those games, with ids 89, 88 and 90, and `game.blankVersions` names them "Stadium", "Pocket Monsters Stadium" and "Stadium 2" instead of `Stadium`, `StadiumJ` and `Stadium2`. `encounters.versions` still defaults to Red for a Stadium save and Crystal for a Stadium 2 save.
