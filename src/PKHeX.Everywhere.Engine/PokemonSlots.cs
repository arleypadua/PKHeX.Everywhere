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
    public static PokemonSlot Find(this Session session, PokemonHandle at)
    {
        var game = session.RequireGame();
        if (at.Source != SlotSource.Draft) return game.FindSaved(at);

        return new PokemonSlot(session.RequireDraft().Pokemon, () => { }, [Topics.Draft]);
    }

    public static PokemonSlot FindEditable(this Session session, PokemonHandle at) => RequireEditable(session.Find(at));

    public static PokemonSlot FindEditableSaved(this Game game, PokemonHandle at) => RequireEditable(game.FindSaved(at));

    private static PokemonSlot RequireEditable(PokemonSlot slot) => slot.Pokemon.IsEditable
        ? slot
        : throw new EngineException(ErrorCodes.UnknownSpecies, $"{slot.Pokemon.Species.Name} can't be edited, as its species is unknown.");

    public static Draft RequireDraft(this Session session) =>
        session.Draft ?? throw new EngineException(ErrorCodes.NoDraft, "No Pokémon is open for editing.");

    public static PokemonSlot FindSaved(this Game game, PokemonHandle at) => at.Source switch
    {
        SlotSource.Party => InParty(game, at.Slot) ?? throw NotFound(at),
        SlotSource.Box => InBox(game, at),
        SlotSource.Draft => throw new EngineException(ErrorCodes.DraftNotAllowed, "This only works on a Pokémon in the party or the box."),
        _ => throw NotFound(at),
    };

    private static PokemonSlot? InParty(Game game, int slot)
    {
        var party = game.Trainer.Party;
        var pokemons = party.Pokemons;
        if (slot < 0 || slot >= pokemons.Count || pokemons[slot].IsEmpty) return null;

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

    private static PokemonSlot InBox(Game game, PokemonHandle at)
    {
        var save = game.SaveFile;
        if (at.Box is not { } box || box < 0 || box >= save.BoxCount || at.Slot < 0 || at.Slot >= save.BoxSlotCount) throw NotFound(at);

        var index = box * save.BoxSlotCount + at.Slot;
        var pokemons = game.Trainer.PokemonBox;
        if (index >= pokemons.All.Count || pokemons.All[index].IsEmpty) throw NotFound(at);

        var pokemon = pokemons.All[index];
        if (game.Trainer.Party.SlotOf(index) is null) return new PokemonSlot(pokemon, pokemons.Commit, [at.Topic()]);

        return new PokemonSlot(pokemon, () =>
        {
            pokemon.Pkm.ResetPartyStats();
            pokemons.Commit();
        }, [at.Topic(), Topics.Party]);
    }

    public static PokemonHandle BoxHandle(SaveFile save, int index) =>
        PokemonHandle.InBox(index / save.BoxSlotCount, index % save.BoxSlotCount);

    private static EngineException NotFound(PokemonHandle at) =>
        new(ErrorCodes.NotFound, $"No Pokémon at {at.Topic()} slot {at.Slot}.");
}
