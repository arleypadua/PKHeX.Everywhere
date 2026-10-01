using System.Collections.Immutable;
using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade;

public class PokemonBox : IMutablePokemonCollection
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

    public bool AddOnEmptySlot(Pokemon pokemon) => AddOnEmptySlot(pokemon, out _);

    public bool AddOnEmptySlot(Pokemon pokemon, out int index)
    {
        index = _game.SaveFile.NextOpenBoxSlot();
        if (index == -1) return false;

        _game.SaveFile.SetBoxSlotAtIndex(pokemon.Pkm, index);
        PopulateFromSave();

        return true;
    }

    internal void RefreshPartyMembers()
    {
        if (_game.SaveFile is not SAV7b { Blocks.Storage: var storage } save) return;

        for (var i = 0; i < _pokemonList.Count; i++)
            if (storage.IsParty(i)) _pokemonList[i] = new Pokemon(save.GetBoxSlotAtIndex(i), _game);

        IndexBySpecies();
    }

    private void PopulateFromSave()
    {
        _pokemonList = _game.SaveFile.BoxData
            .Select(p => new Pokemon(p, _game))
            .ToList();

        IndexBySpecies();
    }

    private void IndexBySpecies()
    {
        BySpecies = _pokemonList
            .Where(p => p.Species != Species.None)
            .GroupBy(p => p.Species)
            .ToImmutableSortedDictionary(
                key => key.Key.Species,
                value => value.OrderByDescending(p => p.Level).ToList());
    }

    public void AddOrUpdate(UniqueId id, Pokemon pokemon)
    {
        var existing = _pokemonList.FirstOrDefault(p => p.UniqueId.Equals(id));

        if (existing is null) HandleAdd(pokemon);
        else HandleUpdate(existing, pokemon);
    }

    private void HandleAdd(Pokemon pokemon)
    {
        var added = AddOnEmptySlot(pokemon);
        if (!added) throw new InvalidOperationException("Pokemon box is full.");
    }

    private void HandleUpdate(Pokemon existing, Pokemon pokemon)
    {
        var index = _pokemonList.IndexOf(existing);
        _pokemonList[index] = pokemon;

        Commit();
        PopulateFromSave();
    }
}