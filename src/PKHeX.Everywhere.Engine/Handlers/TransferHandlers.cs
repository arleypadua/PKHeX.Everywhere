using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Pokemons;
using Pokemon = PKHeX.Facade.Pokemons.Pokemon;
using Transfers = PKHeX.Facade.Transfers;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class TransferHandlers
{
    private const string DefaultFileName = "partner.sav";

    /// <summary>
    /// Opens a partner save next to the loaded one, to move Pokémon between them. It replaces the partner opened before,
    /// and stays open until <c>transfer.close()</c>, even when another save is loaded.
    /// </summary>
    /// <param name="data">The partner save file's bytes.</param>
    /// <param name="fileName">The file name to export the partner with. Defaults to the name of a <c>File</c>, or <c>partner.sav</c>.</param>
    /// <param name="formatId">The id of a save format from <c>game.formats()</c> to load the partner with, as in <c>game.load()</c>.</param>
    [Command("transfer.open", Topics.Transfer)]
    public static TransferSummary Open(Session session, Game game, byte[] data, string? fileName = null, string? formatId = null)
    {
        fileName ??= DefaultFileName;
        var partner = new Session.TransferPartner(GameHandlers.LoadGame(data, fileName, formatId), fileName);
        session.Partner = partner;
        return Summary(game, partner);
    }

    /// <summary>
    /// Reads the open transfer's partner, routes and room, or null when no transfer is open or no save is loaded.
    /// </summary>
    [Query("transfer.get", Topics.Transfer, Topics.Box)]
    public static TransferSummary? Get(Session session) =>
        session is { Game: { } game, Partner: { } partner } ? Summary(game, partner) : null;

    /// <summary>
    /// Lists the partner's party, then its boxed Pokémon in box and slot order. Handles point into the partner save.
    /// </summary>
    [Query("transfer.partnerBoxes", Topics.Transfer)]
    public static PokemonSummary[] PartnerBoxes(Session session)
    {
        var partner = RequirePartner(session).Save;
        return partner.Trainer.Party.Pokemons
            .Select((pokemon, slot) => pokemon.ToSummary(PokemonHandle.Party(slot)))
            .Concat(partner.Trainer.PokemonBox.Boxed().Select(boxed => boxed.Pokemon.ToSummary(PokemonSlots.BoxHandle(partner.SaveFile, boxed.Index))))
            .ToArray();
    }

    /// <summary>
    /// Converts a Pokémon file to <c>to</c>'s format the way a transfer would: by the official route when there is one, unofficially otherwise.
    /// The same file always gives the same bytes. Nothing is written.
    /// Fails with <c>unparseable</c>, <c>not-in-game</c> and <c>conversion-failed</c> as <c>box.previewFile</c> does, and with <c>no-transfer</c> when no transfer is open.
    /// </summary>
    /// <param name="to">The save to convert the Pokémon to.</param>
    [Query("transfer.convert", Topics.Transfer, Topics.Trainer)]
    public static ConvertedPokemon Convert(Session session, Game game, PokemonFile file, TransferSave to)
    {
        var transfer = TransferWith(session, game);
        Pokemon read;
        try
        {
            read = Pokemon.ReadFile(file.Bytes, file.Generation);
        }
        catch (UnreadablePokemonException e)
        {
            throw new EngineException(ErrorCodes.Unparseable, e.Message, e);
        }

        var converted = BoxHandlers.Converting(read, transfer.SaveOf(to.ToFacade()), () => transfer.Convert(read, to.ToFacade()));
        return new ConvertedPokemon(converted.Arrives.ToFile().Bytes, converted.Details().ToEditable());
    }

    /// <summary>
    /// <c>pokemon.export</c> for a slot in either save of the open transfer.
    /// </summary>
    [Query("transfer.export", Topics.Transfer, Topics.Party, Topics.Box)]
    public static ExportedPokemon Export(Session session, Game game, PokemonHandle at, TransferSave save) =>
        TransferWith(session, game).SaveOf(save.ToFacade()).FindSaved(at).Pokemon.ToExported();

    /// <summary>
    /// <c>pokemon.details</c> for a slot in either save of the open transfer.
    /// </summary>
    [Query("transfer.details", Topics.Transfer, Topics.Party, Topics.Box)]
    public static EditablePokemon Details(Session session, Game game, PokemonHandle at, TransferSave save) =>
        TransferWith(session, game).SaveOf(save.ToFacade()).FindSaved(at).Pokemon.Details().ToEditable();

    /// <summary>
    /// Shows what moving the offered Pokémon would change, and which ones can't move. Nothing is written.
    /// A handle without a Pokémon fails with <c>not-found</c>, and the draft with <c>draft-not-allowed</c>.
    /// An arrival for a Pokémon the offer doesn't have fails with <c>not-found</c>, bytes that aren't a Pokémon of the destination's format with <c>unparseable</c>,
    /// and a patch the way <c>pokemon.update</c> fails.
    /// </summary>
    [Query("transfer.preview", Topics.Transfer, Topics.Party, Topics.Box)]
    public static TransferPreview Preview(Session session, Game game, TransferOffer offer)
    {
        var transfer = TransferWith(session, game);
        var preview = Arriving(() => transfer.Preview(ToFacade(transfer, offer)));
        return new TransferPreview(
            preview.Offers.Select(offered => ToDto(transfer, offered)).ToArray(),
            preview.Refused.Select(refused => new RefusedPokemon(refused.Direction.ToDto(), HandleOf(transfer.From(refused.Direction), refused.From), refused.Reason.ToDto())).ToArray());
    }

    /// <summary>
    /// Moves the offered Pokémon: each leaves its slot and lands in the first empty box slots of the other save.
    /// Fails with <c>transfer-refused</c>, writing nothing, when <c>transfer.preview()</c> refuses any of them, and as <c>transfer.preview()</c> does on an arrival.
    /// </summary>
    [Command("transfer.commit", Topics.Transfer, Topics.Party, Topics.Box, Topics.Draft)]
    public static TransferResult Commit(Session session, Game game, TransferOffer offer)
    {
        var transfer = TransferWith(session, game);
        IReadOnlyList<Transfers.TransferArrival> arrived;
        try
        {
            arrived = Arriving(() => transfer.Commit(ToFacade(transfer, offer)));
        }
        catch (Transfers.TransferRefusedException e)
        {
            throw new EngineException(ErrorCodes.TransferRefused, e.Message, e);
        }

        if (session.Draft?.From is { } from && LeavesOrShifts(from, offer.Send)) session.Draft = null;

        var partner = RequirePartner(session);
        return new TransferResult(
            new ExportedSave(game.ToByteArray(), session.FileName ?? string.Empty),
            new ExportedSave(partner.Save.ToByteArray(), partner.FileName),
            arrived.Select(a => new TransferredPokemon(
                a.Direction.ToDto(),
                new PokemonId(a.Pokemon.UniqueId.Value),
                PokemonSlots.BoxHandle(transfer.To(a.Direction).SaveFile, a.BoxIndex))).ToArray());
    }

    /// <summary>
    /// Closes the open transfer, dropping the partner save. Closing with no transfer open does nothing.
    /// </summary>
    [Command("transfer.close", Topics.Transfer)]
    public static void Close(Session session) => session.Partner = null;

    private static Session.TransferPartner RequirePartner(Session session) =>
        session.Partner ?? throw new EngineException(ErrorCodes.NoTransfer, "No transfer is open. Open one with transfer.open().");

    private static Transfers.Transfer TransferWith(Session session, Game game) => new(game, RequirePartner(session).Save);

    // Sending a party member moves the ones after it up a slot, so a draft of any of them would commit over another Pokémon.
    private static bool LeavesOrShifts(PokemonHandle draft, PokemonHandle[] sent) =>
        sent.Contains(draft) || (draft.Source == SlotSource.Party && sent.Any(at => at.Source == SlotSource.Party));

    private static TransferSummary Summary(Game game, Session.TransferPartner partner)
    {
        var transfer = new Transfers.Transfer(game, partner.Save);
        return new TransferSummary(
            partner.Save.ToSummary(partner.FileName),
            new TransferRoutes(transfer.SendRoute.ToDto(), transfer.ReceiveRoute.ToDto()),
            transfer.Room.ToDto());
    }

    private static Transfers.TransferOffer ToFacade(Transfers.Transfer transfer, TransferOffer offer) => new(
        offer.Send.Select(at => SlotOf(transfer.Mine, at)).ToList(),
        offer.Receive.Select(at => SlotOf(transfer.Partner, at)).ToList(),
        offer.Arrivals?.Select(arrival => ToFacade(transfer, offer, arrival)).ToList());

    private static Transfers.ArrivalOverride ToFacade(Transfers.Transfer transfer, TransferOffer offer, TransferArrival arrival)
    {
        var direction = offer.Send.Contains(arrival.At) ? Transfers.TransferDirection.Send
            : offer.Receive.Contains(arrival.At) ? Transfers.TransferDirection.Receive
            : throw new EngineException(ErrorCodes.NotFound, $"The offer has no Pokémon at {arrival.At.Topic()} slot {arrival.At.Slot} to arrive as given.");
        return new Transfers.ArrivalOverride(direction, SlotOf(transfer.From(direction), arrival.At), arrival.Bytes, arrival.Patch.ToFacade());
    }

    private static T Arriving<T>(Func<T> transfer)
    {
        try
        {
            return transfer();
        }
        catch (UnreadablePokemonException e)
        {
            throw new EngineException(ErrorCodes.Unparseable, e.Message, e);
        }
        catch (InvalidPatchException e)
        {
            throw new EngineException(ErrorCodes.InvalidPatch, e.Message, e);
        }
        catch (UnknownSpeciesException e)
        {
            throw new EngineException(ErrorCodes.UnknownSpecies, e.Message, e);
        }
    }

    private static Transfers.TransferSlot SlotOf(Game game, PokemonHandle at)
    {
        game.FindSaved(at);
        return at.Source == SlotSource.Party
            ? new Transfers.TransferSlot(PokemonSource.Party, at.Slot)
            : new Transfers.TransferSlot(PokemonSource.Box, at.Box!.Value * game.SaveFile.BoxSlotCount + at.Slot);
    }

    private static PokemonHandle HandleOf(Game game, Transfers.TransferSlot slot) => slot.Source == PokemonSource.Party
        ? PokemonHandle.Party(slot.Index)
        : PokemonSlots.BoxHandle(game.SaveFile, slot.Index);

    private static OfferedPokemon ToDto(Transfers.Transfer transfer, Transfers.TransferredPokemon offered) => new(
        offered.Direction.ToDto(),
        offered.Route.ToDto(),
        offered.Pokemon.ToSummary(HandleOf(transfer.From(offered.Direction), offered.From)),
        offered.Arrives.ToSummary(PokemonSlots.BoxHandle(transfer.To(offered.Direction).SaveFile, offered.ArrivesAt)),
        offered.Changes.Select(change => change.ToDto()).ToArray(),
        offered.SaveChanges.Select(change => change.ToDto()).ToArray(),
        new Legality(offered.Legality.Valid, offered.Legality.Messages.ToArray()));
}
