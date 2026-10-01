namespace PKHeX.Facade.Pokemons;

public record PokemonDetails(string Nickname, int Level, PokemonLegality Legality);

public record PokemonLegality(bool Valid, IReadOnlyList<string> Messages);

/// <summary>
/// The fields to change on a Pokémon. A null field stays as it is.
/// </summary>
public record PokemonPatch(string? Nickname = null, int? Level = null);

/// <summary>
/// Thrown when a patch holds a value the save can't store. The Pokémon is left unchanged.
/// </summary>
public class InvalidPatchException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
