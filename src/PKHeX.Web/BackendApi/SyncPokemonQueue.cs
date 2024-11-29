using PKHeX.Facade.Pokemons;
using PKHeX.Web.BackendApi.Repositories;

namespace PKHeX.Web.BackendApi;

public class SyncPokemonQueue
{
    private readonly PriorityQueue<Pokemon, int> _queue = new();
    
    public int Size => _queue.Count;
    
    public void Enqueue(Pokemon pokemon, int priority = 0)
    {
        _queue.Enqueue(pokemon, priority);
    }

    public (Pokemon? Pokemon, int? Priority) Dequeue()
    {
        if (_queue.TryDequeue(out var pokemon, out var priority))
        {
            return (pokemon, priority);
        }

        return (null, null);
    }
    
    public bool IsEnqueued(Pokemon pokemon) =>
        _queue.UnorderedItems.Any(p => p.Element.GetLocalSyncId() == pokemon.GetLocalSyncId());
}