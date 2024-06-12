using PKHeX.Facade;

namespace PKHeX.CLI.Extensions;

public static class PokemonExtensions
{
    public static string GetPokemonDisplay(this Pokemon pokemon)
    {
        var gender = pokemon.Gender == Gender.Male
            ? $"[dodgerblue2 bold]{pokemon.Gender.Symbol}[/]"
            : $"[violet bold]{pokemon.Gender.Symbol}[/]";

        var shiny = pokemon.IsShiny ? " ✨" : string.Empty;
        
        return $"{pokemon.Species} {gender} Lv. [yellow]{pokemon.Level}[/]{shiny}";
    }
}
