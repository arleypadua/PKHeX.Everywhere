using System.Net.Http.Json;
using System.Text.Json;
using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Web.Services.AnalyticsResults;

public class AnalyticsResultsService(HttpClient httpClient)
{
    public async Task<Dictionary<string, List<GameLoadedByCountryRepresentation>>> GetGameLoadsByCountry()
    {
        var result =
            await httpClient.GetFromJsonAsync<List<GameLoadedByCountryRepresentation>>(
                $"{BaseResultUrl}/game_loaded_by_country.json",
                SerializerOptions);

        if (result is null) return [];
        
        return result.GroupBy(g => g.Country)
            .ToDictionary(k => k.Key, v => v.ToList());
    }
    
    public async Task<Dictionary<string, List<PokemonSavedByCountryRepresentation>>> GetPokemonSavedByCountry()
    {
        var result =
            await httpClient.GetFromJsonAsync<List<PokemonSavedByCountryRepresentation>>(
                $"{BaseResultUrl}/pokemon_saved_by_country.json",
                SerializerOptions);

        if (result is null) return [];
        
        return result.GroupBy(g => g.Country)
            .ToDictionary(k => k.Key, v => v.ToList());
    }
    
    public async Task<Dictionary<string, List<ItemChangedByCountryRepresentation>>> GetItemChangedByCountry()
    {
        var result =
            await httpClient.GetFromJsonAsync<List<ItemChangedByCountryRepresentation>>(
                $"{BaseResultUrl}/items_changed_by_country.json",
                SerializerOptions);

        if (result is null) return [];
        
        return result.GroupBy(g => g.Country)
            .ToDictionary(k => k.Key, v => v.ToList());
    }
    
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private const string BaseResultUrl =
        "https://raw.githubusercontent.com/pkhex-web/analytics/main/data";
}

public static class AnalyticsResultsServiceExtensions
{
    public static async Task<IEnumerable<(SpeciesDefinition Species, int Count)>> GetSpeciesSavedCount(
        this AnalyticsResultsService service)
    {
        var pokemonEditedByCountry = await service.GetPokemonSavedByCountry();
        return pokemonEditedByCountry
            .SelectMany(g => g.Value)
            .GroupBy(g => g.SpeciesDefinition)
            .Select(g => (g.Key, g.Sum(c => c.CountNumber)));
    }
    
    public static async Task<IEnumerable<(ItemDefinition Definition, int Count)>> GetItemChangedCount(
        this AnalyticsResultsService service)
    {
        var itemChangedByCountry = await service.GetItemChangedByCountry();
        return itemChangedByCountry
            .SelectMany(g => g.Value)
            .GroupBy(g => g.ItemDefinition)
            .Select(g => (g.Key, g.Sum(c => c.CountNumber)));
    }
}

public class GameLoadedByCountryRepresentation
{
    public required string EventName { get; init; }
    public required string VersionName { get; init; }
    public required string Country { get; init; }
    public required string Count { get; init; }
    
    public int CountNumber => int.Parse(Count);
}

public class ItemChangedByCountryRepresentation
{
    public required string EventName { get; init; }
    public required string Country { get; init; }
    public required string ItemId { get; set; }
    public required string Count { get; init; }
    public required string QuantityAverage { get; set; }
    public required string QuantityMax { get; set; }
    public required string QuantityMin { get; set; }

    public ItemDefinition ItemDefinition => ItemRepository.GetItem(ushort.Parse(ItemId));
    public int CountNumber => int.Parse(Count);
}

public class PokemonSavedByCountryRepresentation
{
    public required string EventName { get; init; }
    public required string Country { get; init; }
    public required string Count { get; init; }
    public required string SpeciesId { get; set; }
    public required string SpeciesName { get; set; }

    public SpeciesDefinition SpeciesDefinition => SpeciesRepository.All[(Species)ushort.Parse(SpeciesId)];
    public int CountNumber => int.Parse(Count);
}