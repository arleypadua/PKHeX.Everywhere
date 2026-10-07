---
"@pkhex-everywhere/engine": minor
"@pkhex-everywhere/react": minor
---

A transfer to an older game keeps a copy of the Pokémon, so a later transfer forward restores what the older game dropped. Each offer has `keepsCopy`, and `transfer.commit` returns a `keptCopy` for each Pokémon that moved to an older game. Pass stored copies in the offer's new `keptCopies`: a copy with the Pokémon's `identityKey` puts back its ball, met data, origin game, ribbons and ability, with the new reason `restored`. `PokemonSummary` and `PokemonPreview` gain `identityKey`, built from the PID, trainer ID and secret ID, and null in Gen 1 and 2.
