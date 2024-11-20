using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PKHeX.Web.BackendApi.Representation;

namespace PKHeX.Web.BackendApi;

public class PublicPokemonRepository(
    IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("BackendApi.Anonymous");

    public async Task<PokemonPublicMetadataRepresentation> GetById(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<PokemonPublicMetadataRepresentation>($"/public/pokemon/{id}",
                   SerializerOptions)
               ?? throw new InvalidOperationException("Could not parse the response.");
    }

    public async Task<byte[]> GetBinaries(Guid id)
    {
        return await _httpClient.GetByteArrayAsync($"/public/pokemon/{id}/file");
    }

    private static readonly JsonSerializerOptions SerializerOptions;

    static PublicPokemonRepository()
    {
        SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
}