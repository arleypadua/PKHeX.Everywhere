using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;
using PokemonPatch = PKHeX.Everywhere.Engine.Dtos.PokemonPatch;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class PokemonHandlers
{
    [Requires(Requirement.Save)]
    [Query("pokemon.get", Topics.Party, Topics.Box, Topics.Draft)]
    public static PokemonSummary Get(Session session, PokemonHandle at) => session.Find(at).Pokemon.ToSummary(at);

    [Requires(Requirement.Save)]
    [Query("pokemon.showdown", Topics.Party, Topics.Box, Topics.Draft)]
    public static string Showdown(Session session, PokemonHandle at) => session.Find(at).Pokemon.Showdown();

    [Requires(Requirement.Save)]
    [Query("pokemon.details", Topics.Party, Topics.Box, Topics.Draft)]
    public static EditablePokemon Details(Session session, PokemonHandle at) => session.Find(at).Pokemon.Details().ToEditable();

    /// <summary>
    /// Reads one Gen 3, 4 or 5 Pokémon from bytes taken from a running game, without a loaded save. Nothing is written, and <c>legality</c> is null.
    /// Bytes that aren't a Pokémon, such as all zeros, an empty species or a Gen 3 bad egg, fail with <c>bad-checksum</c>.
    /// From party bytes, <c>level</c> is the level the game stores and shows. Box bytes have no level, so theirs comes from the Pokémon's EXP.
    /// </summary>
    /// <param name="bytes">The Pokémon as the game keeps it: encrypted and shuffled, in its party (100-byte PK3, 236-byte PK4, 220-byte PK5) or box (80-byte PK3, 136-byte PK4 or PK5) layout. Another length fails with <c>bad-arguments</c>.</param>
    /// <param name="version">The PKHeX game version id the bytes come from. A combined version, such as FireRed/LeafGreen, fails with <c>bad-arguments</c>, and a game outside Gen 3 to 5 with <c>not-supported</c>.</param>
    /// <param name="formatId">
    /// The id of a ROM hack's format, from <c>game.formats()</c> or <c>game.version()</c>, such as <c>unbound</c>. The bytes are then read in the hack's party layout, as <c>pokemon.details</c> reads the Pokémon in the hack's save.
    /// Only party bytes are accepted. <c>version</c> must be the format's base game, such as FireRed for Unbound, or the read fails with <c>bad-arguments</c>.
    /// An unknown or disabled id fails with <c>not-found</c>, and <c>pkhex</c> with <c>not-supported</c>.
    /// </param>
    [Query("pokemon.read")]
    public static EditablePokemon Read(byte[] bytes, int version, string? formatId = null)
    {
        try
        {
            var details = formatId is null ? Pokemon.Read(bytes, version) : Pokemon.Read(bytes, version, GameHandlers.FindFormat(formatId));
            return details.ToEditable();
        }
        catch (UnreadablePokemonException e)
        {
            throw new EngineException(e.Reason switch
            {
                UnreadableReason.BadChecksum => ErrorCodes.BadChecksum,
                UnreadableReason.UnsupportedVersion or UnreadableReason.UnsupportedFormat => ErrorCodes.NotSupported,
                _ => ErrorCodes.BadArguments,
            }, e.Message, e);
        }
    }

    [Requires(Requirement.Save)]
    [Query("pokemon.options", Topics.Party, Topics.Box, Topics.Draft)]
    public static Dtos.PokemonOptions Options(Session session, PokemonHandle at) => session.Find(at).Pokemon.Options().ToDto();

    [Requires(Requirement.Save)]
    [Query("pokemon.export", Topics.Party, Topics.Box, Topics.Draft)]
    public static ExportedPokemon Export(Session session, PokemonHandle at)
    {
        var file = session.Find(at).Pokemon.ToFile();
        return new ExportedPokemon(file.Bytes, file.Name);
    }

    [Requires(Requirement.Save)]
    [Command("pokemon.setLevel")]
    public static void SetLevel(Session session, PokemonHandle at, int level)
    {
        if (level is < 1 or > 100)
            throw new EngineException(ErrorCodes.OutOfRange, $"Level must be between 1 and 100, got {level}.");

        var slot = session.FindEditable(at);
        slot.Pokemon.ChangeLevel(level);
        slot.Commit(session);
        session.Raise(new PokemonChanged(at));
    }

    [Requires(Requirement.Save)]
    [Command("pokemon.update")]
    public static void Update(Session session, PokemonHandle at, PokemonPatch patch)
    {
        var slot = session.FindEditable(at);
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
        session.Draft = new Draft(game.FindEditableSaved(at).Pokemon.Clone(), at);

    [Command("pokemon.clone", Topics.Draft)]
    public static void Clone(Session session, Game game, PokemonHandle at) =>
        session.Draft = new Draft(game.FindEditableSaved(at).Pokemon.MakeCopy(), null);

    [Requires(Requirement.Draft)]
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
        session.Raise(new PokemonSaved(from, saved.Pokemon.ToOverview()));
        return new PokemonId(saved.Pokemon.UniqueId.Value);
    }

    [Requires(Requirement.Draft)]
    [Command("pokemon.addToBox", Topics.Draft, Topics.Box)]
    public static AddedPokemon AddToBox(Session session, Game game)
    {
        var draft = session.RequireDraft();
        var box = game.Trainer.PokemonBox;
        if (!box.AddOnEmptySlot(draft.Pokemon, out var index))
            throw new EngineException(ErrorCodes.BoxFull, "Your box is full.");

        var at = PokemonSlots.BoxHandle(game.SaveFile, index);
        var added = box.All[index];
        session.Draft = null;
        session.Raise(new PokemonSaved(at, added.ToOverview()));
        return new AddedPokemon(new PokemonId(added.UniqueId.Value), at);
    }
}
