# PKHeX.Everywhere

Save editing for Pokémon games, in the browser and on the command line. The web UI is a React app, while all save editing stays in C# and runs in .NET WebAssembly.

## Language

**Facade**:
The domain model over PKHeX.Core: games, trainers, Pokémon, boxes and items, with save-format quirks hidden. It holds the editing rules.
_Avoid_: wrapper, adapter

**Engine**:
The UI-free .NET layer that holds the loaded save and answers queries and commands from JavaScript by calling the Facade. It has no editing rules of its own.
_Avoid_: backend, interop

**SDK**:
The TypeScript packages that call the Engine: a client package and its React bindings, partly generated from the Engine.
_Avoid_: API client

**Save format**:
A way to read saves PKHeX doesn't know, such as a ROM hack's. The host registers save formats at startup, and loading asks them before PKHeX's own detection. A save loaded through one carries the format's id, name and base game.
A format detects a save as a certain or a possible match. A possible match fails with `format-choice-required` until the caller names a format, or `pkhex` for PKHeX's own detection.
A format can be registered off, for a hack we haven't validated. It isn't listed or chosen, and a save it matches for certain fails to load instead of falling through to PKHeX. Its possible matches aren't offered. The host turns it on with `game.enableFormat`.
_Avoid_: save type, ROM hack format

**Capability**:
A feature that needs PKHeX to know the save's game: legality, AutoLegality, encounters, Showdown, events or plug-ins. PKHeX saves have all of them, and a save format turns off the ones it can't support. Calls behind a capability that's off fail with `not-supported`, and the UI hides them.
_Avoid_: feature flag, support level

**Game data source**:
What a save can store for each kind of value, with names: species, items, held items, moves, met locations, balls, natures, languages and origin games. It also names the save's Unknown ids, declares its Locked fields and says whether stats are approximate. The Save format provides it next to the Capabilities, and official saves get a default built from PKHeX's data. See [ADR 0008](docs/adr/0008-rom-hacks-are-converted-at-the-save-boundary.md).
_Avoid_: game data, option source

**Unknown**:
An item or move the save stores that PKHeX has no id or name for, such as an Unbound-only item. A species is unknown when neither PKHeX nor the Game data source knows it, such as an egg slot. An unknown item or move can be changed or cleared where it is. A Pokémon of an unknown species is read-only. See [ADR 0010](docs/adr/0010-a-species-the-save-knows-is-not-unknown.md).
_Avoid_: unmapped, invalid

**Hack species**:
A species a Save format defines that PKHeX has no id for, such as Unbound's Shadow Warrior. It has a name, and it has data (types, base stats, abilities, gender ratio, growth rate and catch rate) when we have a source for it. A hack species with data isn't Unknown, and the editor can change its Pokémon.
_Avoid_: custom species, fakemon

**Locked field**:
A Pokémon field the save can't change, such as nature on a Gen 3 save. The Game data source declares them, and the editor shows them disabled.
_Avoid_: read-only field, frozen field

**Handle**:
Where something sits in the save, such as a Pokémon's party or box slot, or a pouch item as `{ pouch, itemId }`. A Pokémon crosses to JavaScript as `{ id, at }`, with its opaque id and its Handle.
_Avoid_: pointer, reference

**Draft**:
The one unsaved Pokémon the Session holds while the editor changes it. It is addressed as a Handle with source `draft` and reaches the save only on commit. See [ADR 0005](docs/adr/0005-editing-goes-through-an-engine-held-draft-slot.md).
_Avoid_: working copy, scratch Pokémon

**Topic**:
A hierarchical path, such as `party`, `box/3` or `inventory`, naming a part of the save that queries read and commands write. A Topic covers every path beneath it.
_Avoid_: cache key, channel

**Engine event**:
A typed record a command raises through the Session, published after the command succeeds. `PlugInRan` is the exception: it is published as soon as the hook runs. JS receives events next to Topics.
_Avoid_: notification, message

**Entity hook**:
A generated React hook, such as `useParty()` or `usePokemon(at)`, that returns an entity's data together with its commands.
_Avoid_: query hook, data hook

**Plug-in host**:
The Engine-side registry and runtime for plug-ins built against the plug-in SDK it supports. It loads a plug-in from its assembly bytes and runs its hooks, and it publishes `PlugInRan` after each run.
_Avoid_: plug-in runtime, plug-in manager

**Page module**:
A JS module embedded in a plug-in assembly that renders one of the plug-in's pages through `mount(element, ctx)`.
_Avoid_: plug-in component, plug-in page component
