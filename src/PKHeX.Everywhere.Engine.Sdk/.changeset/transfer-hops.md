---
'@pkhex-everywhere/engine': minor
'@pkhex-everywhere/react': minor
---

Transfers now chain the games' official hops and reach Generation 6 and 7. The new `transporter` route takes a Generation 5 Pokémon into Pokémon Bank, and `bank` takes it into a Generation 6 or 7 game, or takes a Generation 6 Pokémon into a Generation 7 game. Breaking: `TransferRoutes.send` and `receive` and `OfferedPokemon.route` are now `TransferRoute[]`, the hops in order. A one-hop route is a one-item list, and an unofficial route is `['unofficial']`. Emerald to Sun is `['palPark', 'pokeTransfer', 'transporter', 'bank']`. `TransferChangeReason` adds `transporter` and `bank`, and `TransferRefusal` adds `fused` for a fused Kyurem, which Pokémon Bank refuses.
