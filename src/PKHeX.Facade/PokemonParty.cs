namespace PKHeX.Facade;

public class PokemonParty
{
    private readonly Game _game;

    public PokemonParty(Game game)
    {
        _game = game;
    }

    public IEnumerable<Pokemon> Pokemons => _game.SaveFile.PartyData
        .Select(pkm => new Pokemon(pkm, _game));
}
