using PKHeX.Core;

namespace PKHeX.Facade.Pokemons;

public class PokemonTypes(PKM pokemon)
{
    public int Type1 => ToLaterGamesId(pokemon.PersonalInfo.Type1);
    public int Type2 => ToLaterGamesId(pokemon.PersonalInfo.Type2);

    public bool HasSecondary => Type1 != Type2;

    public IReadOnlyList<int> Ids => HasSecondary ? [Type1, Type2] : [Type1];
    
    public (int Type1, int Type2) Tuple => (Type1, Type2);

    // PKHeX's Gen 1 and 2 personal tables store the games' own type ids.
    private int ToLaterGamesId(byte type) => (int)((MoveType)type).GetMoveTypeGeneration(pokemon.Format);
}
