# Domain logic lives in the Facade

`PKHeX.Facade` is the domain model: objects for the Pokémon world on top of `PKHeX.Core`. It hides save-format quirks, so its callers never check which game a save comes from. The Engine stays thin. A handler resolves its Handles to Facade objects, calls the Facade, maps the result to DTOs, reports Topics, and turns failures into error codes. Nothing else.

When a handler needs a rule the Facade doesn't have, add it to the Facade with tests in `PKHeX.Facade.Tests`, then call it from the handler. A rule the CLI or a plug-in would also need belongs in the Facade. So does any check on the save type, such as `SAV7b`.

## Why

The Facade has more consumers than the Engine: the CLI and the plug-ins. A rule kept in the Engine is missing for all of them, and a bug the Engine works around stays a bug everywhere else. #440 is the example. `PokemonBox.Commit()` drops edits to Let's Go party members, and the Engine routed around it in `PokemonSlots` instead of fixing the box.

Engine tests check the contract with JavaScript: DTO shapes, Topics and error codes. Domain behaviour is tested once, in the Facade.

## Rejected alternatives

- **Logic in Engine handlers.** It would be faster, but the CLI and plug-ins would keep the old behaviour, and the Engine would turn into a second Facade.
