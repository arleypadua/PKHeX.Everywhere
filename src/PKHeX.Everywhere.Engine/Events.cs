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

public sealed record ItemChanged(int ItemId, int Count) : IEngineEvent;

public enum PokemonAddSource
{
    File,
    Encounter,
}

public sealed record PokemonAdded(PokemonHandle At, PokemonAddSource Source, PokemonOverview Pokemon) : IEngineEvent;

public sealed record PokemonChanged(PokemonHandle At) : IEngineEvent;

public sealed record PokemonSaved(PokemonHandle At, PokemonOverview Pokemon) : IEngineEvent;

public sealed record GameExported(GameOverview Game) : IEngineEvent;

public sealed record GameLoaded(GameOverview Game) : IEngineEvent;

public sealed record GameClosed : IEngineEvent;
