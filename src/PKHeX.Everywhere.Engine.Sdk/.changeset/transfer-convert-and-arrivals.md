---
'@pkhex-everywhere/engine': minor
'@pkhex-everywhere/react': minor
---

Add `transfer.convert`, `transfer.details` and `transfer.export`. A transfer offer takes `arrivals`: each places an offered Pokémon as given bytes plus a patch, instead of running the usual conversion. `EditablePokemon` gains `evolutionFamily` and `ExportedPokemon` gains `generation`. Remove kept copies: `KeptCopy`, `TransferOffer.keptCopies`, `OfferedPokemon.keepsCopy`, `TransferredPokemon.keptCopy`, `identityKey` and the `restored` change reason.
