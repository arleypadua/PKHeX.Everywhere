using System.Text.Json;
using PKHeX.Everywhere.Engine.Dtos;

namespace PKHeX.Everywhere.Engine;

/// <summary>
/// A domain change a command announces. Holds only primitives or Engine DTOs, so JS receives it as JSON with a camelCase <c>type</c>.
/// </summary>
public interface IEngineEvent;

/// <summary>
/// Writes an Engine event declared in another assembly as JSON. Pass that assembly's generated <c>HandlerRegistry.TryWriteEvent</c>.
/// </summary>
public delegate bool EventWriter(IEngineEvent engineEvent, Utf8JsonWriter writer);

/// <summary>
/// Fires when an item's count in a pouch changes: through <c>inventory.setItem</c>, or once per ticket that <c>events.giveTickets</c> adds.
/// </summary>
/// <param name="ItemId">The item's id in the save's own item list, the same id space as <c>OwnedItem.id</c>.</param>
/// <param name="Count">The item's new count. 0 means it was removed.</param>
public sealed record ItemChanged(int ItemId, int Count) : IEngineEvent;

/// <summary>
/// Where a Pokémon added to the box came from: an imported Pokémon file (<c>box.addFromFile</c>) or an encounter search result (<c>box.addEncounter</c>).
/// </summary>
public enum PokemonAddSource
{
    File,
    Encounter,
}

/// <summary>
/// Fires when <c>box.addFromFile</c> or <c>box.addEncounter</c> puts a Pokémon in the first empty box slot.
/// </summary>
/// <param name="At">The box slot the Pokémon went to.</param>
public sealed record PokemonAdded(PokemonHandle At, PokemonAddSource Source, PokemonOverview Pokemon) : IEngineEvent;

/// <summary>
/// Fires when <c>pokemon.setLevel</c> or <c>pokemon.update</c> changes a Pokémon.
/// </summary>
/// <param name="At">The Pokémon that changed.</param>
public sealed record PokemonChanged(PokemonHandle At) : IEngineEvent;

/// <summary>
/// Fires when an edited or cloned Pokémon is written to the save: by <c>pokemon.commit</c> back to its slot, or by <c>pokemon.addToBox</c> to the first empty box slot.
/// </summary>
/// <param name="At">The slot the Pokémon was saved to.</param>
/// <param name="Pokemon">The Pokémon as saved.</param>
public sealed record PokemonSaved(PokemonHandle At, PokemonOverview Pokemon) : IEngineEvent;

/// <summary>
/// Fires when <c>game.export</c> writes the save file for download.
/// </summary>
public sealed record GameExported(GameOverview Game) : IEngineEvent;

/// <summary>
/// Fires when a save is loaded, by <c>game.load</c> or <c>game.loadBlank</c>. Loading over an open save doesn't fire <c>gameClosed</c> first.
/// </summary>
public sealed record GameLoaded(GameOverview Game) : IEngineEvent;

/// <summary>
/// Fires when <c>game.close</c> closes the loaded save. Closing with no save loaded fires nothing.
/// </summary>
public sealed record GameClosed : IEngineEvent;
