using PKHeX.Core;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Abstractions;

/// <summary>
/// What a save can store for each kind of value, with a name for each. Its Save format provides it, and a save with no format gets <see cref="PKHeXGameData"/>.
/// </summary>
public interface IGameDataSource
{
    IReadOnlyList<Choice> Species { get; }
    IReadOnlyList<Choice> Items { get; }
    IReadOnlyList<Choice> HeldItems { get; }
    IReadOnlyList<Choice> Moves { get; }
    IReadOnlyList<Choice> Balls { get; }
    IReadOnlyList<Choice> Natures { get; }
    IReadOnlyList<Choice> Languages { get; }
    IReadOnlyList<Choice> OriginGames { get; }

    /// <summary>
    /// The locations a Pokémon from the origin game can be met at, or hatched at when <paramref name="egg"/> is set.
    /// </summary>
    IReadOnlyList<Choice> MetLocations(GameVersion origin, bool egg = false);

    /// <summary>
    /// Names an id the save stores that PKHeX can't name, or returns null when the save doesn't own the id.
    /// </summary>
    string? NameOf(GameDataKind kind, int id);

    /// <summary>
    /// The Pokémon fields the save can't change.
    /// </summary>
    IReadOnlySet<PokemonField> Locked { get; }

    /// <summary>
    /// Whether computed stats may differ from the game's, because they use base stats the game may change.
    /// </summary>
    bool StatsApproximate { get; }

    /// <summary>
    /// Whether the save stores which of the species' abilities is active instead of an ability id, so a Pokémon can only have one of its species' abilities.
    /// </summary>
    bool StoresAbilitySlot => false;
}

public enum GameDataKind
{
    Species,
    Item,
    HeldItem,
    Move,
    MetLocation,
    Ball,
    Nature,
    Language,
    OriginGame,
}
