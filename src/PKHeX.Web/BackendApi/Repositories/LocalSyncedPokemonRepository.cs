using System.Text.Json;
using Blazored.LocalStorage;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;
using PKHeX.Web.BackendApi.Representation;

namespace PKHeX.Web.BackendApi.Repositories;

public class LocalSyncedPokemonRepository(
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
    public static void Add(this LocalSyncedPokemonRepository rep,
        Pokemon pokemon,
        PokemonMetadataRepresentation metadata) =>
        rep.Add(pokemon.GetLocalSyncId(), metadata.Id);

    public static void Remove(this LocalSyncedPokemonRepository rep,
        Pokemon pokemon) =>
        rep.Remove(pokemon.GetLocalSyncId());

    public static void RemoveByRemoteId(this LocalSyncedPokemonRepository rep,
        Guid remoteId)
    {
        var set = rep.Get();
        var item = set.FirstOrDefault(x => x.RemoteId == remoteId);
        if (item is null)
            return;
        rep.Remove(item.LocalUniqueId);
    }

    public static LocalSyncedPokemonRepository.SyncedPokemon? GetByRemoteId(this LocalSyncedPokemonRepository rep,
        Guid remoteId)
    {
        var set = rep.Get();
        return set.SingleOrDefault(x => x.RemoteId == remoteId);
    }

    public static bool IsSynced(this LocalSyncedPokemonRepository rep,
        Pokemon pokemon)
    {
        var set = rep.Get();
        return set.Any(x => x.LocalUniqueId == pokemon.GetLocalSyncId());
    }

    public static bool IsSynced(this LocalSyncedPokemonRepository rep,
        Pokemon pokemon,
        out LocalSyncedPokemonRepository.SyncedPokemon synced)
    {
        var set = rep.Get();
        var byLocal = set.FirstOrDefault(x => x.LocalUniqueId == pokemon.GetLocalSyncId());

        if (byLocal is null)
        {
            synced = null!;
            return false;
        }

        synced = byLocal;
        return true;
    }

    public static bool IsSynced(this LocalSyncedPokemonRepository rep,
        Guid remoteId)
    {
        var set = rep.Get();
        return set.Any(x => x.RemoteId == remoteId);
    }

    public static int Count(this LocalSyncedPokemonRepository rep) =>
        rep.Get().Count;

    public static bool Any(this LocalSyncedPokemonRepository rep) =>
        rep.Count() > 0;

    /// <summary>
    /// This is the best effort trying to find a unique identifier for a Pokemon.
    /// Because save files don't really have a unique identifier, we have to make one up.
    ///
    /// Not using Pid as it is easily changeable.
    ///
    /// Uses: TID, SID, EncryptionConstant, OwnerName (depending on the handler), Version (MetConditions), Version (Pkm), Gen (Pkm)
    /// </summary>
    public static string GetLocalSyncId(this Pokemon pokemon) =>
        string.Join("--", 
            pokemon.Id, 
            pokemon.PidDisplay(), 
            pokemon.Owner.GetName(),
            pokemon.MetConditions.Location.Name, 
            pokemon.MetConditions.Level, 
            pokemon.MetConditions.Version.Version,
            pokemon.Pkm.Version, 
            pokemon.Pkm.Context,
            pokemon.IVs.Attack,
            pokemon.IVs.Defense,
            pokemon.IVs.Health,
            pokemon.IVs.Speed,
            pokemon.IVs.SpecialAttack,
            pokemon.IVs.SpecialDefense);
}