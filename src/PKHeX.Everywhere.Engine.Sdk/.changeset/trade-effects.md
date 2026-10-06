---
"@pkhex-everywhere/engine": minor
---

Trades now follow the receiving game's rules. `trade.preview()` and `trade.commit()` apply trade evolutions, reset friendship to 70 on Generation 2 to 4 link trades, and revert Giratina, Shaymin and Rotom forms leaving Platinum. Platinum refuses with `bagFull` when its bag can't take back the Griseous Orb. New change reasons: `received`, `tradeEvolution`, `itemUsed` and `formReverted`. `TradeSaveChange` now has `save` and `label` instead of `species`, and new kinds `itemReturned` and `eventVar`.
