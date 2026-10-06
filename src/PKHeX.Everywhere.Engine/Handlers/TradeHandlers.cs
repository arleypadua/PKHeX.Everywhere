using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Pokemons;
using Trades = PKHeX.Facade.Trades;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class TradeHandlers
{
    private const string DefaultFileName = "partner.sav";

    /// <summary>
    /// Opens a partner save next to the loaded one, to move Pokémon between them. It replaces the partner opened before,
    /// and stays open until <c>trade.close()</c>, even when another save is loaded.
    /// </summary>
    /// <param name="data">The partner save file's bytes.</param>
    /// <param name="fileName">The file name to export the partner with. Defaults to the name of a <c>File</c>, or <c>partner.sav</c>.</param>
    /// <param name="formatId">The id of a save format from <c>game.formats()</c> to load the partner with, as in <c>game.load()</c>.</param>
    [Command("trade.open", Topics.Trade)]
    public static TradeSummary Open(Session session, Game game, byte[] data, string? fileName = null, string? formatId = null)
    {
        fileName ??= DefaultFileName;
        var partner = new Session.OpenTrade(GameHandlers.Read(data, fileName, formatId), fileName);
        session.Trade = partner;
        return Summary(game, partner);
    }

    /// <summary>
    /// Reads the open trade's partner, routes and room, or null when no trade is open or no save is loaded.
    /// </summary>
    [Query("trade.get", Topics.Trade, Topics.Box)]
    public static TradeSummary? Get(Session session) =>
        session is { Game: { } game, Trade: { } partner } ? Summary(game, partner) : null;

    /// <summary>
    /// Lists the partner's party, then its boxed Pokémon in box and slot order. Handles point into the partner save.
    /// </summary>
    [Query("trade.partnerBoxes", Topics.Trade)]
    public static PokemonSummary[] PartnerBoxes(Session session)
    {
        var partner = RequirePartner(session).Partner;
        return partner.Trainer.Party.Pokemons
            .Select((pokemon, slot) => pokemon.ToSummary(PokemonHandle.Party(slot)))
            .Concat(partner.Trainer.PokemonBox.Boxed().Select(boxed => boxed.Pokemon.ToSummary(PokemonSlots.BoxHandle(partner.SaveFile, boxed.Index))))
            .ToArray();
    }

    /// <summary>
    /// Shows what moving the offered Pokémon would change, and which ones can't move. Nothing is written.
    /// A handle without a Pokémon fails with <c>not-found</c>, and the draft with <c>draft-not-allowed</c>.
    /// </summary>
    [Query("trade.preview", Topics.Trade, Topics.Party, Topics.Box)]
    public static TradePreview Preview(Session session, Game game, TradeOffer offer)
    {
        var trade = Start(session, game);
        var preview = trade.Preview(ToFacade(trade, offer));
        return new TradePreview(
            preview.Offers.Select(offered => ToDto(trade, offered)).ToArray(),
            preview.Refused.Select(refused => new RefusedPokemon(refused.Direction.ToDto(), HandleOf(From(trade, refused.Direction), refused.From), refused.Reason.ToDto())).ToArray());
    }

    /// <summary>
    /// Moves the offered Pokémon: each leaves its slot and lands in the first empty box slots of the other save.
    /// Fails with <c>trade-refused</c>, writing nothing, when <c>trade.preview()</c> refuses any of them.
    /// </summary>
    [Command("trade.commit", Topics.Trade, Topics.Party, Topics.Box, Topics.Draft)]
    public static TradeResult Commit(Session session, Game game, TradeOffer offer)
    {
        var trade = Start(session, game);
        IReadOnlyList<Trades.TradeArrival> arrived;
        try
        {
            arrived = trade.Commit(ToFacade(trade, offer));
        }
        catch (Trades.TradeRefusedException e)
        {
            throw new EngineException(ErrorCodes.TradeRefused, e.Message, e);
        }

        if (offer.Send.Length > 0 && session.Draft?.From is not null) session.Draft = null;

        var partner = RequirePartner(session);
        return new TradeResult(
            new ExportedSave(game.ToByteArray(), session.FileName ?? string.Empty),
            new ExportedSave(partner.Partner.ToByteArray(), partner.FileName),
            arrived.Select(a => new TradedPokemon(
                a.Direction.ToDto(),
                new PokemonId(a.Pokemon.UniqueId.Value),
                PokemonSlots.BoxHandle(To(trade, a.Direction).SaveFile, a.BoxIndex))).ToArray());
    }

    /// <summary>
    /// Closes the open trade, dropping the partner save. Closing with no trade open does nothing.
    /// </summary>
    [Command("trade.close", Topics.Trade)]
    public static void Close(Session session) => session.Trade = null;

    private static Session.OpenTrade RequirePartner(Session session) =>
        session.Trade ?? throw new EngineException(ErrorCodes.NoTrade, "No trade is open. Open one with trade.open().");

    private static Trades.Trade Start(Session session, Game game) => new(game, RequirePartner(session).Partner);

    private static TradeSummary Summary(Game game, Session.OpenTrade partner)
    {
        var trade = new Trades.Trade(game, partner.Partner);
        return new TradeSummary(
            partner.Partner.ToSummary(partner.FileName),
            new TradeRoutes(trade.SendRoute.ToDto(), trade.ReceiveRoute.ToDto()),
            trade.Room.ToDto());
    }

    private static Game From(Trades.Trade trade, Trades.TradeDirection direction) =>
        direction == Trades.TradeDirection.Send ? trade.Mine : trade.Partner;

    private static Game To(Trades.Trade trade, Trades.TradeDirection direction) =>
        direction == Trades.TradeDirection.Send ? trade.Partner : trade.Mine;

    private static Trades.TradeOffer ToFacade(Trades.Trade trade, TradeOffer offer) => new(
        offer.Send.Select(at => SlotOf(trade.Mine, at)).ToList(),
        offer.Receive.Select(at => SlotOf(trade.Partner, at)).ToList());

    private static Trades.TradeSlot SlotOf(Game game, PokemonHandle at)
    {
        game.FindSaved(at);
        return at.Source == SlotSource.Party
            ? new Trades.TradeSlot(PokemonSource.Party, at.Slot)
            : new Trades.TradeSlot(PokemonSource.Box, at.Box!.Value * game.SaveFile.BoxSlotCount + at.Slot);
    }

    private static PokemonHandle HandleOf(Game game, Trades.TradeSlot slot) => slot.Source == PokemonSource.Party
        ? PokemonHandle.Party(slot.Index)
        : PokemonSlots.BoxHandle(game.SaveFile, slot.Index);

    private static OfferedPokemon ToDto(Trades.Trade trade, Trades.TradedPokemon offered) => new(
        offered.Direction.ToDto(),
        offered.Pokemon.ToSummary(HandleOf(From(trade, offered.Direction), offered.From)),
        offered.Arrives.ToSummary(PokemonSlots.BoxHandle(To(trade, offered.Direction).SaveFile, offered.ArrivesAt)),
        offered.Changes.Select(change => change.ToDto()).ToArray(),
        offered.SaveChanges.Select(change => change.ToDto()).ToArray(),
        new Legality(offered.Legality.Valid, offered.Legality.Messages.ToArray()));
}
