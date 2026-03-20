using System.Text.RegularExpressions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Extensions;

public static class PokemonExtensions
{
    public static string NameDisplay(this Pokemon pokemon) => pokemon.Nickname == pokemon.Species.Name
        ? pokemon.Species.Name
        : $"{pokemon.Nickname} ({pokemon.Species.Name})";
    
    /// <summary>
    /// Returns the PID of the Pokémon in hexadecimal format.
    /// </summary>
    public static string PidDisplay(this Pokemon pokemon) => pokemon.PID.ToString("x8");
    
    public static string Showdown(this Pokemon pokemon) => ShowdownParsing.GetShowdownText(pokemon.Pkm);
    
    public static string Showdown(this IEnumerable<Pokemon> pokemonList) => string.Join("\n\n", pokemonList.Select(Showdown));

    public static LegalityAnalysis Legality(this Pokemon pokemon) => new (pokemon.Pkm);
    
    public static async Task<Pokemon> ToLegalAsync(this Pokemon pokemon)
    {
        if (pokemon.Legality().Valid) return pokemon;
        
        var template = pokemon.Pkm.Clone();
        var result = await pokemon.Game.SaveFile.LegalizeAsync(template);
        
        return new(result, pokemon.Game);
    }

    public static async Task ApplyLegalAsync(this Pokemon pokemon)
    {
        var result = await pokemon.ToLegalAsync();
        pokemon.ApplyChangesFrom(result, keepPid: true);
        if (!pokemon.Legality().Valid)
        {
            ApplyDoableCorrections(result, pokemon);

            if (pokemon.Legality().Valid)
            {
                return;
            }

            pokemon.ApplyChangesFrom(result, keepPid: false);
        }
    }

    private static void ApplyDoableCorrections(Pokemon before, Pokemon after)
    {
        var legality = after.Legality();
        if (legality.Valid) return;

        var localization = LegalityLocalizationContext.Create(legality);

        var hasOriginalTrainerTrashBytesIssue = false;
        foreach (var result in legality.Results)
        {
            if (result is not { Judgement: Severity.Invalid, Identifier: CheckIdentifier.TrashBytes })
                continue;

            if (!localization.Humanize(result).Contains("OT", StringComparison.Ordinal))
                continue;

            hasOriginalTrainerTrashBytesIssue = true;
            break;
        }

        if (hasOriginalTrainerTrashBytesIssue)
        {
            before.Game.SaveFile.ApplyTo(after.Pkm);
        }
    }
}

public static partial class SpeciesExtensions
{
    public static int Id(this Species species) => (int)species;
    public static string Name(this Species species) => PascalCaseRegex().Replace(species.ToString(), " $1");
    
    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex PascalCaseRegex();
}