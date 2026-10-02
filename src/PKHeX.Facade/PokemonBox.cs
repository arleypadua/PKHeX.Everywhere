using System.Collections.Immutable;
using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade;

public class PokemonBox : IMutablePokemonCollection
{
    public PokemonBox(Game game, PokemonParty party)
    {
        _game = game;
        _party = party;

        PopulateFromSave();
    }

    private readonly Game _game;
    private readonly PokemonParty _party;
    private IList<Pokemon> _pokemonList = default!;

    public IDictionary<Species, List<Pokemon>> BySpecies { get; private set; } = default!;
    public IList<Pokemon> All => _pokemonList;

    public IEnumerable<(int Index, Pokemon Pokemon)> Boxed() => _pokemonList
        .Select((pokemon, index) => (index, pokemon))
        .Where(p => !p.pokemon.IsEmpty && !IsPartyMember(p.index));

    public string Showdown()
    {
        _game.Require(Capability.Showdown);
        return Boxed().Select(boxed => boxed.Pokemon).Showdown();
    }

    private bool IsPartyMember(int index) => _game.SaveFile is SAV7b { Blocks.Storage: var storage } && storage.IsParty(index);

    public void Commit()
    {
        for (var index = 0; index < _pokemonList.Count; index++)
            if (!_game.SaveFile.IsBoxSlotOverwriteProtected(index))
                _game.SaveFile.SetBoxSlotAtIndex(_pokemonList[index].Pkm, index, EntityImportSettings.None);

        foreach (var (index, pkm) in _party.BoxedMembers())
            _game.SaveFile.SetBoxSlotAtIndex(pkm, index, EntityImportSettings.None);
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

    internal void SharePartyMembers()
    {
        foreach (var (index, pkm) in _party.BoxedMembers())
            _pokemonList[index] = new Pokemon(pkm, _game);

        IndexBySpecies();
    }

    private void PopulateFromSave()
    {
        _pokemonList = _game.SaveFile.BoxData
            .Select(p => new Pokemon(p, _game))
            .ToList();

        SharePartyMembers();
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
        if (_party.SlotOf(index) is { } slot)
        {
            pokemon.Pkm.ResetPartyStats();
            _party.Replace(slot, pokemon.Pkm);
        }

        Commit();
        PopulateFromSave();
    }
}