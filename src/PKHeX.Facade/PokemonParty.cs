using PKHeX.Core;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade;

public class PokemonParty(Game game)
{
    private readonly IList<PKM> _partyData = game.SaveFile.PartyData;
    public IList<Pokemon> Pokemons => _partyData
        .Select(pkm => new Pokemon(pkm, game))
        .ToList();

    public void Commit()
    {
        game.SaveFile.PartyData = _partyData;
    }
}
