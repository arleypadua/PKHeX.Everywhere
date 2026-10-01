using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using Pokemon = PKHeX.Facade.Pokemons.Pokemon;

namespace PKHeX.Everywhere.Engine;

internal sealed record PokemonSlot(Pokemon Pokemon, Action Commit);

internal static class PokemonSlots
{
    public static PokemonSlot Find(this Game game, PokemonHandle at) => at.Source switch
    {
        SlotSource.Party => InParty(game, at),
        SlotSource.Box => InBox(game, at),
        _ => throw NotFound(at),
    };

    private static PokemonSlot InParty(Game game, PokemonHandle at)
    {
        var party = game.Trainer.Party;
        var pokemons = party.Pokemons;
        if (at.Slot < 0 || at.Slot >= pokemons.Count || pokemons[at.Slot].Pkm.Species == 0) throw NotFound(at);

        var pokemon = pokemons[at.Slot];
        return new PokemonSlot(pokemon, () =>
        {
            pokemon.Pkm.ResetPartyStats();
            party.Commit();
        });
    }

    private static PokemonSlot InBox(Game game, PokemonHandle at)
    {
        var save = game.SaveFile;
        if (at.Box is not { } box || box < 0 || box >= save.BoxCount || at.Slot < 0 || at.Slot >= save.BoxSlotCount) throw NotFound(at);

        var pokemons = game.Trainer.PokemonBox;
        var index = box * save.BoxSlotCount + at.Slot;
        if (index >= pokemons.All.Count || pokemons.All[index].Pkm.Species == 0) throw NotFound(at);

        return new PokemonSlot(pokemons.All[index], pokemons.Commit);
    }

    private static EngineException NotFound(PokemonHandle at) =>
        new(ErrorCodes.NotFound, $"No Pokémon at {at.Topic()} slot {at.Slot}.");
}
