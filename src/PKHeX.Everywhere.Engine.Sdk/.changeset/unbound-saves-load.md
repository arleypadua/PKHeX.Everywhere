---
'@pkhex-everywhere/engine': patch
---

`game.load` opens Pokémon Unbound saves, with `format.id` set to `unbound`. Exporting one keeps its file size, including a flashcart's 16-byte RTC trailer.
