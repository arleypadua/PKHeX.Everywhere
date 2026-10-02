using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;
using PokemonPatch = PKHeX.Everywhere.Engine.Dtos.PokemonPatch;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class PokemonHandlers
{
    [Query("pokemon.get", Topics.Party, Topics.Box, Topics.Draft)]
    public static PokemonSummary Get(Session session, PokemonHandle at) => session.Find(at).Pokemon.ToSummary(at);

    [Query("pokemon.showdown", Topics.Party, Topics.Box, Topics.Draft)]
    public static string Showdown(Session session, PokemonHandle at) => session.Find(at).Pokemon.Showdown();

    [Query("pokemon.details", Topics.Party, Topics.Box, Topics.Draft)]
    public static EditablePokemon Details(Session session, PokemonHandle at) => session.Find(at).Pokemon.Details().ToEditable();

    [Query("pokemon.options", Topics.Party, Topics.Box, Topics.Draft)]
    public static Dtos.PokemonOptions Options(Session session, PokemonHandle at) => session.Find(at).Pokemon.Options().ToDto();

    [Query("pokemon.export", Topics.Party, Topics.Box, Topics.Draft)]
    public static ExportedPokemon Export(Session session, PokemonHandle at)
    {
        var file = session.Find(at).Pokemon.ToFile();
        return new ExportedPokemon(file.Bytes, file.Name);
    }

    [Command("pokemon.setLevel")]
    public static void SetLevel(Session session, PokemonHandle at, int level)
    {
        if (level is < 1 or > 100)
            throw new EngineException(ErrorCodes.OutOfRange, $"Level must be between 1 and 100, got {level}.");

        var slot = session.Find(at);
        slot.Pokemon.ChangeLevel(level);
        slot.Commit(session);
        session.Raise(new PokemonChanged(at));
    }

    [Command("pokemon.update")]
    public static void Update(Session session, PokemonHandle at, PokemonPatch patch)
    {
        var slot = session.Find(at);
        try
        {
            slot.Pokemon.Update(patch.ToFacade());
        }
        catch (InvalidPatchException e)
        {
            throw new EngineException(ErrorCodes.InvalidPatch, e.Message, e);
        }

        slot.Commit(session);
        session.Raise(new PokemonChanged(at));
    }

    [Command("pokemon.edit", Topics.Draft)]
    public static void Edit(Session session, Game game, PokemonHandle at) =>
        session.Draft = new Draft(game.FindSaved(at).Pokemon.Clone(), at);

    [Command("pokemon.clone", Topics.Draft)]
    public static void Clone(Session session, Game game, PokemonHandle at) =>
        session.Draft = new Draft(game.FindSaved(at).Pokemon.MakeCopy(), null);

    [Command("pokemon.commit", Topics.Draft)]
    public static PokemonId Commit(Session session, Game game)
    {
        var draft = session.RequireDraft();
        var from = draft.From ?? throw new EngineException(ErrorCodes.NoSlot, "This Pokémon is a clone. Add it to the box instead.");
        var replaced = game.FindSaved(from).Pokemon;
        var source = from.Source == SlotSource.Party ? PokemonSource.Party : PokemonSource.Box;
        game.Trainer.AddOrUpdate(replaced.UniqueId, draft.Pokemon, source);

        var saved = game.FindSaved(from);
        session.AlsoWrote(saved.Topics);
        session.Draft = null;
        session.Raise(new PokemonSaved(from));
        return new PokemonId(saved.Pokemon.UniqueId.Value);
    }

    [Command("pokemon.addToBox", Topics.Draft, Topics.Box)]
    public static AddedPokemon AddToBox(Session session, Game game)
    {
        var draft = session.RequireDraft();
        var box = game.Trainer.PokemonBox;
        if (!box.AddOnEmptySlot(draft.Pokemon, out var index))
            throw new EngineException(ErrorCodes.BoxFull, "Your box is full.");

        var at = PokemonSlots.BoxHandle(game.SaveFile, index);
        session.Draft = null;
        session.Raise(new PokemonSaved(at));
        return new AddedPokemon(new PokemonId(box.All[index].UniqueId.Value), at);
    }
}
