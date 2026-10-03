using System.Collections.Frozen;

namespace PKHeX.Facade.Pokemons;

public record PokemonDetails(
    int Species,
    int Form,
    Gender Gender,
    int Nature,
    int Ability,
    int HeldItem,
    string? UnknownHeldItem,
    int Ball,
    int Friendship,
    int Language,
    bool IsShiny,
    bool? IsAlpha,
    bool IsEgg,
    string Nickname,
    int Level,
    uint Pid,
    IReadOnlyList<int> Types,
    bool IsInfected,
    bool IsCured,
    uint TrainerId,
    uint SecretId,
    string OriginalTrainerName,
    Gender OriginalTrainerGender,
    string HandlingTrainerName,
    Gender HandlingTrainerGender,
    Owner.Handler CurrentHandler,
    int Version,
    int MetLocation,
    int MetLevel,
    DateOnly? MetDate,
    bool FatefulEncounter,
    StatValues Ivs,
    StatValues Evs,
    StatValues? Avs,
    StatValues Stats,
    HiddenPowerDefinition? HiddenPower,
    int? CombatPower,
    int? CalculatedCombatPower,
    IReadOnlyList<MoveSlot> Moves,
    PokemonLegality? Legality);

public record StatValues(int Health, int Attack, int Defense, int SpecialAttack, int SpecialDefense, int Speed);

public record MoveSlot(int Id, string Name, int Pp, int MaxPp)
{
    public bool IsUnknown { get; init; }
}

public record PokemonLegality(bool Valid, IReadOnlyList<string> Messages);

/// <summary>
/// The fields to change on a Pokémon. A null field stays as it is.
/// </summary>
public record PokemonPatch(
    int? Species = null,
    int? Form = null,
    Gender? Gender = null,
    int? Nature = null,
    int? Ability = null,
    int? HeldItem = null,
    int? Ball = null,
    int? Friendship = null,
    int? Language = null,
    bool? IsShiny = null,
    bool? IsAlpha = null,
    bool? IsEgg = null,
    string? Nickname = null,
    int? Level = null,
    uint? TrainerId = null,
    uint? SecretId = null,
    string? OriginalTrainerName = null,
    Gender? OriginalTrainerGender = null,
    string? HandlingTrainerName = null,
    Gender? HandlingTrainerGender = null,
    Owner.Handler? CurrentHandler = null,
    int? Version = null,
    int? MetLocation = null,
    int? MetLevel = null,
    DateOnly? MetDate = null,
    bool? FatefulEncounter = null,
    StatPatch? Ivs = null,
    StatPatch? Evs = null,
    StatPatch? Avs = null,
    int? CombatPower = null,
    IReadOnlyList<int>? Moves = null);

public record StatPatch(
    int? Health = null,
    int? Attack = null,
    int? Defense = null,
    int? SpecialAttack = null,
    int? SpecialDefense = null,
    int? Speed = null);

/// <summary>
/// Thrown when a patch holds a value the save can't store. The Pokémon is left unchanged.
/// </summary>
public class InvalidPatchException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}

public record Choice(int Id, string Name);

/// <summary>
/// The choices that depend on the Pokémon itself: its evolution line, its species' abilities, its forms, the met locations of its origin game and the moves it can legally know.
/// </summary>
public record PokemonOptions(IReadOnlyList<Choice> Species, IReadOnlyList<Choice> Abilities, IReadOnlyList<Choice> Forms, IReadOnlyList<Choice> MetLocations, IReadOnlyList<Choice> Moves)
{
    public IReadOnlySet<PokemonField> Locked { get; init; } = FrozenSet<PokemonField>.Empty;
}

/// <summary>
/// A Pokémon field a save can lock, named as in <see cref="PokemonPatch"/>.
/// </summary>
public enum PokemonField
{
    Gender,
    Nature,
    Ability,
    MetLocation,
}
