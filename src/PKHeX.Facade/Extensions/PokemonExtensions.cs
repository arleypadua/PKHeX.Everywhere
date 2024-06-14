using System.Text.RegularExpressions;
using PKHeX.Core;

namespace PKHeX.Facade.Extensions;

public static class PokemonExtensions
{
    public static string NameDisplay(this Pokemon pokemon) => pokemon.Nickname == pokemon.Species.Name()
        ? pokemon.Species.Name()
        : $"{pokemon.Nickname} ({pokemon.Species.Name()})";
}

public static partial class SpeciesExtensions
{
    public static string Name(this Species species) => PascalCaseRegex().Replace(species.ToString(), " $1");
    
    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex PascalCaseRegex();
}