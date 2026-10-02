---
'@pkhex-everywhere/engine': minor
---

`game.load` fails with the new `format-choice-required` error code when a save might be in a format PKHeX doesn't know, such as Pokémon Radical Red. The `EngineError` lists those formats in `candidates`. Pass one of their ids as `formatId` to load the save with it, or `pkhex` to load it with PKHeX's own detection.
