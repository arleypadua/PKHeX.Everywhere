using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Pokemons;
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
    /// Shows what moving the offered Pokémon would change, and which ones can't move. Nothing is written.
    /// A handle without a Pokémon fails with <c>not-found</c>, and the draft with <c>draft-not-allowed</c>.
    /// </summary>
    [Query("transfer.preview", Topics.Transfer, Topics.Party, Topics.Box)]
    public static TransferPreview Preview(Session session, Game game, TransferOffer offer)
    {
        var transfer = TransferWith(session, game);
        var preview = transfer.Preview(ToFacade(transfer, offer));
        return new TransferPreview(
            preview.Offers.Select(offered => ToDto(transfer, offered)).ToArray(),
            preview.Refused.Select(refused => new RefusedPokemon(refused.Direction.ToDto(), HandleOf(transfer.From(refused.Direction), refused.From), refused.Reason.ToDto())).ToArray());
    }

    /// <summary>
    /// Moves the offered Pokémon: each leaves its slot and lands in the first empty box slots of the other save.
    /// Fails with <c>transfer-refused</c>, writing nothing, when <c>transfer.preview()</c> refuses any of them.
    /// </summary>
    [Command("transfer.commit", Topics.Transfer, Topics.Party, Topics.Box, Topics.Draft)]
    public static TransferResult Commit(Session session, Game game, TransferOffer offer)
    {
        var transfer = TransferWith(session, game);
        IReadOnlyList<Transfers.TransferArrival> arrived;
        try
        {
            arrived = transfer.Commit(ToFacade(transfer, offer));
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
        offer.Receive.Select(at => SlotOf(transfer.Partner, at)).ToList());

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
