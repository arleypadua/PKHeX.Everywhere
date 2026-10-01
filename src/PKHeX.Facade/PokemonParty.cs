using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade;

public class PokemonParty(Game game) : IMutablePokemonCollection
{
    private readonly IList<PKM> _partyData = game.SaveFile.PartyData;
    public IList<Pokemon> Pokemons => _partyData
        .Select(pkm => new Pokemon(pkm, game))
        .ToList();

    public void Commit()
    {
        // SAV7b empty party slots point to a non-existent box slot, so the PartyData setter throws when blanking them
        if (game.SaveFile is SAV7b)
        {
            for (var i = 0; i < _partyData.Count; i++)
                game.SaveFile.SetPartySlotAtIndex(_partyData[i], i);
            game.Trainer.PokemonBox.RefreshPartyMembers();
            return;
        }

        game.SaveFile.PartyData = _partyData;
    }

    // Let's Go keeps party members in box storage, so each one also sits at a box index.
    public int? BoxIndexOf(int slot) => game.SaveFile is SAV7b save && slot >= 0 && slot < save.PartyCount
        ? (save.GetPartyOffset(slot) - save.GetBoxSlotOffset(0)) / save.SIZE_BOXSLOT
        : null;

    public int? SlotOf(int boxIndex) => Enumerable.Range(0, game.SaveFile.PartyCount)
        .Where(slot => BoxIndexOf(slot) == boxIndex)
        .Select(slot => (int?)slot)
        .FirstOrDefault();

    public void AddOrUpdate(UniqueId id, Pokemon pokemon)
    {
        var existing = _partyData.FirstOrDefault(p => UniqueId.From(p).Equals(id));

        if (existing is null)
            throw new InvalidOperationException("Adding pokemons to the party is not supported.");
        
        var index = _partyData.IndexOf(existing);
        _partyData[index] = pokemon.Pkm;
        
        Commit();
    }
}
