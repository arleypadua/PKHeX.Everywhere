using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade;

public class PokemonBox
{
    public PokemonBox(Game game)
    {
        _game = game;
        _pokemonList = game.SaveFile.BoxData
            .Select(p => new Pokemon(p, game))
            .ToImmutableList();

        BySpecies = _pokemonList
            .Where(p => p.Species != Species.None)
            .GroupBy(p => p.Species)
            .ToImmutableSortedDictionary(
                key => key.Key, 
                value => value.OrderByDescending(p => p.Level).ToList());
    }
    
    private readonly Game _game;
    private readonly IList<Pokemon> _pokemonList;
    
    public IDictionary<Species, List<Pokemon>> BySpecies { get; }

    public void Commit()
    {
        _game.SaveFile.BoxData = _pokemonList
            .Select(p => p.Pkm)
            .ToList();
    }
}