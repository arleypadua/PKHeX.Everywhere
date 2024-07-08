using System.Collections.Immutable;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade;

public class PokemonBox
{
    public PokemonBox(Game game)
    {
        _game = game;
        
        PopulateFromSave();
    }
    
    private readonly Game _game;
    private IList<Pokemon> _pokemonList = default!;
    
    public IDictionary<Species, List<Pokemon>> BySpecies { get; private set; } = default!;
    public IList<Pokemon> All => _pokemonList;

    public void Commit()
    {
        _game.SaveFile.BoxData = _pokemonList
            .Select(p => p.Pkm)
            .ToList();
    }

    public bool AddOnEmptySlot(Pokemon pokemon)
    {
        var openSlot = _game.SaveFile.NextOpenBoxSlot();
        if (openSlot == -1) return false;
        
        _game.SaveFile.SetBoxSlotAtIndex(pokemon.Pkm, openSlot);
        PopulateFromSave();

        return true;
    }

    private void PopulateFromSave()
    {
        _pokemonList = _game.SaveFile.BoxData
            .Select(p => new Pokemon(p, _game))
            .ToImmutableList();

        BySpecies = _pokemonList
            .Where(p => p.Species != Species.None)
            .GroupBy(p => p.Species)
            .ToImmutableSortedDictionary(
                key => key.Key.Species, 
                value => value.OrderByDescending(p => p.Level).ToList());
    }
}