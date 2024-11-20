using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PKHeX.Facade.Pokemons;
using PKHeX.Web.BackendApi.Representation;

namespace PKHeX.Web.BackendApi;

public class MyPokemonRepository(
    SyncedPokemonRepository syncedRepository,
    IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("BackendApi");
    
    public async Task<List<PokemonMetadataRepresentation>> GetAll()
    {
        var synced = await _httpClient.GetFromJsonAsync<List<PokemonMetadataRepresentation>>("/pokemon", SerializerOptions);
        return synced ?? [];
    }

    public async Task<PokemonMetadataRepresentation> GetByLocalId(string localId)
    {
        var synced = syncedRepository.Get().FirstOrDefault(s => s.LocalUniqueId == localId)
                     ?? throw new InvalidOperationException($"{localId} is not stored in the local synced");

        return await GetById(synced.RemoteId);
    }

    public async Task<PokemonMetadataRepresentation> GetById(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<PokemonMetadataRepresentation>($"/pokemon/{id}", SerializerOptions)
               ?? throw new InvalidOperationException("Could not parse the response.");
    }

    public async Task<PokemonMetadataRepresentation> Upload(Pokemon pokemon)
    {
        var alreadySyncedPokemonSet = syncedRepository.Get();
        var alreadySynced = alreadySyncedPokemonSet.FirstOrDefault(s => s.LocalUniqueId == pokemon.GetLocalSyncId());

        return alreadySynced is not null
            ? await Replace(alreadySynced.RemoteId, pokemon)
            : await UploadNew(pokemon);
    }

    public Task Share(Guid id, bool isShared)
    {
        return _httpClient.PostAsJsonAsync($"/pokemon/share/{id}", new { IsShared = isShared });
    }

    public Task AllowDownload(Guid id, bool isAllowed)
    {
        return _httpClient.PostAsJsonAsync($"/pokemon/allow-download/{id}", new { IsAllowed = isAllowed });
    }
    
    public Task RemoveByLocalId(string localId)
    {
        return RemoveById(syncedRepository.Get().First(s => s.LocalUniqueId == localId).RemoteId);
    }
    
    public async Task RemoveById(Guid id)
    {
        await _httpClient.DeleteAsync($"/pokemon/{id}");
        syncedRepository.RemoveByRemoteId(id);
    }

    private async Task<PokemonMetadataRepresentation> UploadNew(Pokemon pokemon)
    {
        var file = pokemon.ToFile();
        
        var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent(file.Bytes), "file", file.Name);
        
        var response = await _httpClient.PostAsync("/pokemon/upload", multipart);

        response.EnsureSuccessStatusCode();

        var metadata = await response.Content.ReadFromJsonAsync<PokemonMetadataRepresentation>(SerializerOptions)
                       ?? throw new InvalidOperationException("Could not parse the response.");

        syncedRepository.Add(pokemon, metadata);

        return metadata;
    }

    private async Task<PokemonMetadataRepresentation> Replace(Guid id, Pokemon pokemon)
    {
        var file = pokemon.ToFile();

        var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent(file.Bytes), "file", file.Name);
        
        var response = await _httpClient.PutAsync($"/pokemon/upload/{id}", multipart);

        response.EnsureSuccessStatusCode();

        var metadata = await response.Content.ReadFromJsonAsync<PokemonMetadataRepresentation>(SerializerOptions)
                       ?? throw new InvalidOperationException("Could not parse the response.");

        return metadata;
    }

    private static readonly JsonSerializerOptions SerializerOptions;

    static MyPokemonRepository()
    {
        SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
}