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
