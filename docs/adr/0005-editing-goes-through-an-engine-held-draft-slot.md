# Editing goes through an Engine-held draft slot

The Pokémon editor changes a copy and writes it to the save only on Save. In Blazor the copy lived in the page. React pages hold no Facade objects, so the Engine holds it instead. Context: pkhex-web/issue-tracker#89.

## Decision

The Session holds at most one draft: a Pokémon and the slot it came from. JavaScript addresses it as a `PokemonHandle` with source `draft`, and its Topic is `draft`.

- `pokemon.edit(at)` opens a draft from a clone of the saved Pokémon and replaces any draft already open.
- Every Pokémon query, command, Entity hook and plug-in action takes the draft handle like any party or box slot. Calls that only make sense for a saved slot, such as `pokemon.edit`, reject it with `draft-not-allowed`.
- `pokemon.commit()` writes the draft to its slot, clears the draft and returns the Pokémon's new id.
- Loading or closing a save clears the draft. Leaving the page without saving leaves the draft in place; nothing reads it, and the next `edit` replaces it.
- Commands on the draft raise `PokemonChanged(draft)`, so plug-in hooks run on the unsaved Pokémon as they did in Blazor. `commit` raises `PokemonSaved` with the real slot.

## Why the session holds state

The editor sends one change at a time and reads back legality after each. Keeping the copy in the Engine means a change crosses the boundary as a small patch, and the rules that apply it stay in the Facade. One draft is enough: the editor and clone pages open one Pokémon at a time, and replacing on `edit` needs no cleanup when a page remounts or the player moves to another Pokémon.

## Rejected alternatives

- **The page holds the Pokémon as bytes and sends it with every call.** Each call would parse and serialize the Pokémon, and plug-in hooks couldn't change what the page shows.
- **Several drafts addressed by id.** Nothing needs more than one, and ids would need a lifecycle the pages have to manage.
