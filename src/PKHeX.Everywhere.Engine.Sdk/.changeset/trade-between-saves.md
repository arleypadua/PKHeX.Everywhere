---
"@pkhex-everywhere/engine": patch
"@pkhex-everywhere/react": patch
---

New `trade` calls move Pokémon between the loaded save and a partner save, the way link trades, the Time Capsule, Pal Park and Poké Transfer do. `trade.open()` loads the partner and reports the routes and empty box slots, `trade.preview()` shows what each Pokémon arrives as and refuses what the games refuse, and `trade.commit()` moves them and returns both saves. New `useTrade()` hook and `trade` topic.
