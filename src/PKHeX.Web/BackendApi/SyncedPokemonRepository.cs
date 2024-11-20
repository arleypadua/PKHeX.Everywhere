using System.Text.Json;
using Blazored.LocalStorage;
using PKHeX.Facade.Pokemons;
using PKHeX.Web.BackendApi.Representation;

namespace PKHeX.Web.BackendApi;

public class SyncedPokemonRepository(
    ISyncLocalStorageService localStorage)
{
    private const string Key = "synced-pokemon";
    
    public HashSet<SyncedPokemon> Get()
    {
        var json = localStorage.GetItemAsString(Key);
        if (string.IsNullOrEmpty(json))
            return new HashSet<SyncedPokemon>();
        
        return JsonSerializer.Deserialize<HashSet<SyncedPokemon>>(json)!;
    }
    
    public void Add(string localId, Guid remoteId)
    {
        var set = Get();
        set.Add(new SyncedPokemon(localId, remoteId));
        localStorage.SetItemAsString(Key, JsonSerializer.Serialize(set));
    }
    
    public void Remove(string localId)
    {
        var set = Get();
        set.RemoveWhere(x => x.LocalUniqueId == localId);
        localStorage.SetItemAsString(Key, JsonSerializer.Serialize(set));
    }
    
    public record SyncedPokemon(
        string LocalUniqueId,
        Guid RemoteId);
}

public static class SyncedPokemonRepositoryExtensions
{
    public static void Add(this SyncedPokemonRepository rep, 
        Pokemon pokemon,
        PokemonMetadataRepresentation metadata) =>
        rep.Add(pokemon.GetLocalSyncId(), metadata.Id);
    
    public static void Remove(this SyncedPokemonRepository rep, 
        Pokemon pokemon) =>
        rep.Remove(pokemon.GetLocalSyncId());
    
    public static void RemoveByRemoteId(this SyncedPokemonRepository rep, 
        Guid remoteId)
    {
        var set = rep.Get();
        var item = set.FirstOrDefault(x => x.RemoteId == remoteId);
        if (item is null)
            return;
        rep.Remove(item.LocalUniqueId);
    }

    public static bool IsSynced(this SyncedPokemonRepository rep,
        Pokemon pokemon)
    {
        var set = rep.Get();
        return set.Any(x => x.LocalUniqueId == pokemon.GetLocalSyncId());
    }

    /// <summary>
    /// This is the best effort trying to find a unique identifier for a Pokemon.
    /// Because save files don't really have a unique identifier, we have to make one up.
    ///
    /// Not using Pid as it is easily changeable.
    ///
    /// Uses: TID, SID, EncryptionConstant, OwnerName (depending on the handler), Version (MetConditions), Version (Pkm), Gen (Pkm)
    /// </summary>
    public static string GetLocalSyncId(this Pokemon pokemon) =>
        $"{pokemon.Id}" +
        $"--{pokemon.Pkm.EncryptionConstant}" +
        $"--{pokemon.Owner.GetName()}" +
        $"--{pokemon.MetConditions.Version.Version}" +
        $"--{pokemon.Pkm.Version}" +
        $"--{pokemon.Pkm.Context}";
}