using PKHeX.Facade;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.CLI.Extensions;

public static class PokemonExtensions
{
    public static string GetPokemonDisplay(this Pokemon pokemon, bool includeLegalityFlag = false)
    {
        var gender = $"[{pokemon.Gender.Color()} bold]{pokemon.Gender.Symbol}[/]";
        var legalityFlag = includeLegalityFlag
            ? pokemon.LegalityFlag()
            : string.Empty;

        return $"{pokemon.NameDisplay()} {gender} Lv. [yellow]{pokemon.Level}[/]{pokemon.ShinyDisplay()} {legalityFlag}";
    }

    public static string ShinyDisplay(this Pokemon pokemon) => pokemon.IsShiny ? " ✨" : string.Empty;

    private static string Color(this Gender gender)
    {
        if (gender == Gender.Male) return "dodgerblue2";
        if (gender == Gender.Female) return "violet";
        if (gender == Gender.Genderless) return "yellow3";

        return string.Empty;
    }

    private static string LegalityFlag(this Pokemon pokemon) => pokemon.Legality().Valid
        ? string.Empty
        : " [red bold]-- Illegal --[/]";
}