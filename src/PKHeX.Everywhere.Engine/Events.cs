using PKHeX.Everywhere.Engine.Dtos;

namespace PKHeX.Everywhere.Engine;

/// <summary>
/// A domain change a command announces. Holds only primitives or Engine DTOs, and stays in-process.
/// </summary>
public interface IEngineEvent;

public sealed record ItemChanged(int ItemId, int Count) : IEngineEvent;

public enum PokemonAddSource
{
    File,
}

public sealed record PokemonAdded(PokemonHandle At, PokemonAddSource Source) : IEngineEvent;

public sealed record PokemonChanged(PokemonHandle At) : IEngineEvent;

public sealed record PokemonSaved(PokemonHandle At) : IEngineEvent;
