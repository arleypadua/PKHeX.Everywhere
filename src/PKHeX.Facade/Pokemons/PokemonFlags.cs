using PKHeX.Core;

namespace PKHeX.Facade.Pokemons;

public class PokemonFlags(PKM pokemon)
{
    public bool IsInfected
    {
        get => pokemon.IsPokerusInfected;
        set => pokemon.IsPokerusInfected = value;
    }

    public bool IsCured
    {
        get => pokemon.IsPokerusCured;
        set => pokemon.IsPokerusCured = value;
    }
}