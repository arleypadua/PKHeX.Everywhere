using System.Text.RegularExpressions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Extensions;

public static class PokemonExtensions
{
    public static string NameDisplay(this Pokemon pokemon) => pokemon.Nickname == pokemon.Species.Name
        ? pokemon.Species.Name
        : $"{pokemon.Nickname} ({pokemon.Species.Name})";

    public static LegalityAnalysis Legality(this Pokemon pokemon) => new (pokemon.Pkm);

    public static string Showdown(this Pokemon pokemon) => ShowdownParsing.GetShowdownText(pokemon.Pkm);
    
    public static string Showdown(this IEnumerable<Pokemon> pokemonList) => string.Join("\n\n", pokemonList.Select(Showdown));
}

public static partial class SpeciesExtensions
{
    public static int Id(this Species species) => (int)species;
    public static string Name(this Species species) => PascalCaseRegex().Replace(species.ToString(), " $1");
    
    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex PascalCaseRegex();
}