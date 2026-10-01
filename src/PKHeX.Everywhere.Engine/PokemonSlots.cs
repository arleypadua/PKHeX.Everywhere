using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using Pokemon = PKHeX.Facade.Pokemons.Pokemon;

namespace PKHeX.Everywhere.Engine;

internal sealed record PokemonSlot(Pokemon Pokemon, Action Save, string[] Topics)
{
    public void Commit(Session session)
    {
        Save();
        session.AlsoWrote(Topics);
    }
}

internal static class PokemonSlots
{
    public static PokemonSlot Find(this Game game, PokemonHandle at) => at.Source switch
    {
        SlotSource.Party => InParty(game, at.Slot) ?? throw NotFound(at),
        SlotSource.Box => InBox(game, at),
        _ => throw NotFound(at),
    };

    private static PokemonSlot? InParty(Game game, int slot)
    {
        var party = game.Trainer.Party;
        var pokemons = party.Pokemons;
        if (slot < 0 || slot >= pokemons.Count || pokemons[slot].Pkm.Species == 0) return null;

        var pokemon = pokemons[slot];
        string[] topics = party.BoxIndexOf(slot) is { } index
            ? [Topics.Party, BoxHandle(game.SaveFile, index).Topic()]
            : [Topics.Party];
        return new PokemonSlot(pokemon, () =>
        {
            pokemon.Pkm.ResetPartyStats();
            party.Commit();
        }, topics);
    }

    // The box commit skips Let's Go party members, so they are written through the party.
    private static PokemonSlot InBox(Game game, PokemonHandle at)
    {
        var save = game.SaveFile;
        if (at.Box is not { } box || box < 0 || box >= save.BoxCount || at.Slot < 0 || at.Slot >= save.BoxSlotCount) throw NotFound(at);

        var index = box * save.BoxSlotCount + at.Slot;
        if (game.Trainer.Party.SlotOf(index) is { } partySlot) return InParty(game, partySlot) ?? throw NotFound(at);

        var pokemons = game.Trainer.PokemonBox;
        if (index >= pokemons.All.Count || pokemons.All[index].Pkm.Species == 0) throw NotFound(at);

        return new PokemonSlot(pokemons.All[index], pokemons.Commit, [at.Topic()]);
    }

    private static PokemonHandle BoxHandle(SaveFile save, int index) =>
        PokemonHandle.InBox(index / save.BoxSlotCount, index % save.BoxSlotCount);

    private static EngineException NotFound(PokemonHandle at) =>
        new(ErrorCodes.NotFound, $"No Pokémon at {at.Topic()} slot {at.Slot}.");
}
