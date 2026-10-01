namespace PKHeX.Facade.Pokemons;

public record PokemonDetails(
    int Species,
    int Form,
    Gender Gender,
    int Nature,
    int Ability,
    int HeldItem,
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
    PokemonLegality Legality);

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
    int? Level = null);

/// <summary>
/// Thrown when a patch holds a value the save can't store. The Pokémon is left unchanged.
/// </summary>
public class InvalidPatchException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}

public record Choice(int Id, string Name);

/// <summary>
/// The choices that depend on the Pokémon itself: its evolution line, its species' abilities and its forms.
/// </summary>
public record PokemonOptions(IReadOnlyList<Choice> Species, IReadOnlyList<Choice> Abilities, IReadOnlyList<Choice> Forms);
