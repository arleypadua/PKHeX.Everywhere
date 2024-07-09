using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Abstractions;

public interface IMutablePokemonCollection
{
    void AddOrUpdate(UniqueId id, Pokemon pokemon);
}