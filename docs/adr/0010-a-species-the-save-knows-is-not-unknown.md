# A species the save knows is not Unknown

[ADR 0008](0008-rom-hacks-are-converted-at-the-save-boundary.md) made a Pokémon of a species with no PKHeX id read-only. Unbound defines species of its own, and we have the data for some of them: Shadow Warrior, Zygarde Cell and Zygarde Core. Their players want to edit them like any other Pokémon. Context: pkhex-web/issue-tracker#180.

This supersedes the rule in ADR 0008 that "an unknown species has no PKHeX id, and its Pokémon is read-only". The rest of 0008 stands.

## Decision

- A species is Unknown when neither PKHeX nor the Game data source knows it. A Pokémon of an Unknown species stays read-only.
- A hack species with data gets an id from the range `RomHacks` keeps for values PKHeX has no id for. The Game data source lists it, so it isn't Unknown. Players can edit and copy its Pokémon, and `Game.IsAwareOf` accepts it in a save of the same hack.
- The Facade only checks whether the Game data source lists the id. It never decodes the id.
- Where the Facade reads PKHeX's static per-species tables, it handles ids PKHeX doesn't know:
  - The species has no evolutions.
  - Its Pokémon is offered every move the Game data source lists.
  - Clearing its nickname keeps the nickname, as PKHeX has no default name for it.
  - Rerolling its PID keeps its gender, read from its own personal info.
- A hack species with only a name, egg slots and filler indexes stay Unknown.

## Why

The Game data source already decides which values a save can store. Letting it decide which species are known adds no new channel, and the next hack with species data only changes `RomHacks`.

## Rejected alternatives

- **Give hack species a PKHeX id.** That would need a change to our PKHeX fork, and PKHeX's tables would still have no data for them.
- **Keep hack species read-only.** Their players couldn't change a level or a move.
