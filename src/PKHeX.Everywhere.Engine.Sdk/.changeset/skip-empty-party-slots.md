---
"@pkhex-everywhere/engine": patch
---

`party.get`, `transfer.partnerBoxes`, `party.showdown` and the `party` of `GameOverview` leave out empty party slots. Each Pokémon keeps its real slot in its handle.
