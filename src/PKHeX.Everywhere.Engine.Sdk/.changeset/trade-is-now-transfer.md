---
"@pkhex-everywhere/engine": minor
"@pkhex-everywhere/react": minor
---

The `trade` namespace is now `transfer`. `trade.*` calls become `transfer.*`, `useTrade` becomes `useTransfer`, every `Trade*` type becomes `Transfer*`, `TradedPokemon` becomes `TransferredPokemon`, the `trade` topic becomes `transfer`, and the `trade-refused` and `no-trade` errors become `transfer-refused` and `no-transfer`. The `link` route and the `link` and `tradeEvolution` change reasons keep their names.
