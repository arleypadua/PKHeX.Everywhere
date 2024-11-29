using PKHeX.Core;

namespace PKHeX.Facade.Pokemons;

public class PokemonTypes(PKM pokemon)
{
    public int Type1 => pokemon.PersonalInfo.Type1;
    public int Type2 => pokemon.PersonalInfo.Type2;

    public bool HasSecondary => Type1 != Type2;
    
    public (int Type1, int Type2) Tuple => (Type1, Type2);
}