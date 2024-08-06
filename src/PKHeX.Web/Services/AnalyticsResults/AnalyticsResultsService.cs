using System.Net.Http.Json;
using System.Text.Json;

namespace PKHeX.Web.Services.AnalyticsResults;

public class AnalyticsResultsService(HttpClient httpClient)
{
    public async Task<Dictionary<string, List<GameLoadedByCountry>>> GetGameLoadsByCountry()
    {
        var result =
            await httpClient.GetFromJsonAsync<List<GameLoadedByCountry>>(
                $"{BaseResultUrl}/game_loaded_by_country.json",
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

public class GameLoadedByCountry
{
    public required string EventName { get; init; }
    public required string VersionName { get; init; }
    public required string Country { get; init; }
    public required string Count { get; init; }
    
    public int CountNumber => int.Parse(Count);
}

public class ItemChangedByCountry
{
    public required string EventName { get; init; }
    public required string Country { get; init; }
    public required string ItemId { get; set; }
    public required string Count { get; init; }
    public required double QuantityAverage { get; set; }
    public required string QuantityMax { get; set; }
    public required string QuantityMin { get; set; }
}

public class PokemonSavedByCountry
{
    public required string EventName { get; init; }
    public required string Country { get; init; }
    public required string Count { get; init; }
    public required string SpeciesId { get; set; }
    public required string SpeciesName { get; set; }
}