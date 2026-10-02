using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade;

public class PokemonParty(Game game) : IMutablePokemonCollection
{
    private const int MaxPartySize = 6;
    private readonly IList<PKM> _partyData = game.SaveFile.PartyData;
    public IList<Pokemon> Pokemons => _partyData
        .Select(pkm => new Pokemon(pkm, game))
        .ToList();

    public string Showdown()
    {
        game.Require(Capability.Showdown);
        return Pokemons.Showdown();
    }

    public void Commit()
    {
        // SAV7b empty party slots point to a non-existent box slot, so they can't be blanked
        if (game.SaveFile is SAV7b)
        {
            for (var i = 0; i < _partyData.Count; i++)
                game.SaveFile.SetPartySlotAtIndex(_partyData[i], i, EntityImportSettings.None);
            return;
        }

        var members = _partyData.Where(pkm => !Pokemon.IsBlank(pkm)).ToList();
        for (var i = 0; i < MaxPartySize; i++)
            game.SaveFile.SetPartySlotAtIndex(i < members.Count ? members[i] : game.SaveFile.BlankPKM, i, EntityImportSettings.None);
    }

    // Let's Go keeps party members in box storage, so each one also sits at a box index.
    public int? BoxIndexOf(int slot) => game.SaveFile is SAV7b save && slot >= 0 && slot < save.PartyCount
        ? (save.GetPartyOffset(slot) - save.GetBoxSlotOffset(0)) / save.SIZE_BOXSLOT
        : null;

    public int? SlotOf(int boxIndex) => Enumerable.Range(0, game.SaveFile.PartyCount)
        .Where(slot => BoxIndexOf(slot) == boxIndex)
        .Select(slot => (int?)slot)
        .FirstOrDefault();

    internal IEnumerable<(int BoxIndex, PKM Pkm)> BoxedMembers()
    {
        for (var slot = 0; slot < _partyData.Count; slot++)
            if (BoxIndexOf(slot) is { } index) yield return (index, _partyData[slot]);
    }

    internal void Replace(int slot, PKM pkm) => _partyData[slot] = pkm;

    public void AddOrUpdate(UniqueId id, Pokemon pokemon)
    {
        var existing = _partyData.FirstOrDefault(p => UniqueId.From(p).Equals(id));

        if (existing is null)
            throw new InvalidOperationException("Adding pokemons to the party is not supported.");
        
        var index = _partyData.IndexOf(existing);
        pokemon.Pkm.ResetPartyStats();
        _partyData[index] = pokemon.Pkm;
        
        Commit();
        game.Trainer.PokemonBox.SharePartyMembers();
    }
}
