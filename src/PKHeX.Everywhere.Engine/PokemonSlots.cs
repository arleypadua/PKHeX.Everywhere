using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using Pokemon = PKHeX.Facade.Pokemons.Pokemon;

namespace PKHeX.Everywhere.Engine;

internal sealed record PokemonSlot(Pokemon Pokemon, Action Commit, string[] Topics);

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
        string[] topics = BoxIndexOfPartyMember(game.SaveFile, slot) is { } index
            ? [Topics.Party, BoxHandle(game.SaveFile, index).Topic()]
            : [Topics.Party];
        return new PokemonSlot(pokemon, () =>
        {
            pokemon.Pkm.ResetPartyStats();
            party.Commit();
        }, topics);
    }

    private static PokemonSlot InBox(Game game, PokemonHandle at)
    {
        var save = game.SaveFile;
        if (at.Box is not { } box || box < 0 || box >= save.BoxCount || at.Slot < 0 || at.Slot >= save.BoxSlotCount) throw NotFound(at);

        var index = box * save.BoxSlotCount + at.Slot;
        if (PartySlotOf(save, index) is { } partySlot) return InParty(game, partySlot) ?? throw NotFound(at);

        var pokemons = game.Trainer.PokemonBox;
        if (index >= pokemons.All.Count || pokemons.All[index].Pkm.Species == 0) throw NotFound(at);

        return new PokemonSlot(pokemons.All[index], pokemons.Commit, [at.Topic()]);
    }

    // Let's Go keeps party members in box storage, and the box commit skips their slots, so they are written as party members.
    private static int? PartySlotOf(SaveFile save, int boxIndex) => save is SAV7b
        ? Enumerable.Range(0, save.PartyCount).Cast<int?>().FirstOrDefault(slot => BoxIndexOfPartyMember(save, slot!.Value) == boxIndex)
        : null;

    private static int? BoxIndexOfPartyMember(SaveFile save, int slot) => save is SAV7b
        ? (save.GetPartyOffset(slot) - save.GetBoxSlotOffset(0)) / save.SIZE_BOXSLOT
        : null;

    private static PokemonHandle BoxHandle(SaveFile save, int index) =>
        PokemonHandle.InBox(index / save.BoxSlotCount, index % save.BoxSlotCount);

    private static EngineException NotFound(PokemonHandle at) =>
        new(ErrorCodes.NotFound, $"No Pokémon at {at.Topic()} slot {at.Slot}.");
}
