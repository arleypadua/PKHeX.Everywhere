using PKHeX.Core;

namespace PKHeX.Facade;

public class PokemonParty(Game game)
{
    private IList<PKM> _partyData = game.SaveFile.PartyData;
    public IEnumerable<Pokemon> Pokemons => _partyData
        .Select(pkm => new Pokemon(pkm, game));

    public void Commit()
    {
        game.SaveFile.PartyData = _partyData;
    }
}
