using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class MysteryGiftHandlers
{
    /// <summary>
    /// The save's Wonder Card album and whether Mystery Gift is unlocked.
    /// Only Generation 4 and 5 saves have an album, which <c>game.get</c> reports as <c>hasMysteryGift</c>. The <c>mysteryGift</c> calls fail with <c>not-supported</c> on any other save.
    /// </summary>
    [Query("mysteryGift.status", Topics.MysteryGift)]
    public static GiftAlbum Status(Game game) => AlbumOf(game).ToDto();

    /// <summary>
    /// Lists the events from PKHeX's event database that the save's game can receive, in every language.
    /// </summary>
    [Query("mysteryGift.events", Topics.MysteryGift)]
    public static GiftEvent[] Events(Game game) => AlbumOf(game).Events.Select(entry => entry.ToDto()).ToArray();

    /// <summary>
    /// Shows an event's card as the save would receive it. Nothing is written.
    /// Fails with <c>not-found</c> for an id <c>mysteryGift.events</c> doesn't list.
    /// </summary>
    /// <param name="id">The event's id, from <c>mysteryGift.events</c>.</param>
    [Query("mysteryGift.previewEvent", Topics.Trainer)]
    public static WonderCardPreview PreviewEvent(Game game, string id) => FindEvent(AlbumOf(game), id).ToPreview(game);

    /// <summary>
    /// Shows a Wonder Card file as the save would receive it. Nothing is written.
    /// Takes <c>.pgt</c>, <c>.pcd</c> and <c>.wc4</c> files for Generation 4 saves and <c>.pgf</c> files for Generation 5 saves.
    /// Fails with <c>unparseable</c> when the bytes aren't a card, and <c>wrong-generation</c> for a card of another generation.
    /// </summary>
    [Query("mysteryGift.previewFile", Topics.Trainer)]
    public static WonderCardPreview PreviewFile(Game game, byte[] bytes) => Refusing(() => AlbumOf(game).ReadFile(bytes)).ToPreview(game);

    /// <summary>
    /// Puts an event's card in the album as the game does when it receives one, and marks its card number received.
    /// The player collects the gift from the delivery person in the game: in Generation 4 it waits in the delivery queue, and in Generation 5 the card isn't collected yet.
    /// A filled slot is replaced. A slot past the filled ones puts the card in the first empty slot, so the album never has a gap.
    /// Mystery Gift stays as locked or unlocked as it was; call <c>setUnlocked</c> to show it.
    /// Fails with <c>not-found</c> for an id <c>mysteryGift.events</c> doesn't list, <c>out-of-range</c> for a slot outside the album,
    /// and <c>no-slot</c> when eight Generation 4 gifts already wait for the delivery person.
    /// </summary>
    /// <param name="id">The event's id, from <c>mysteryGift.events</c>.</param>
    /// <param name="slot">Zero-based album slot.</param>
    /// <returns>The slot the card landed in.</returns>
    [Command("mysteryGift.insertEvent", Topics.MysteryGift)]
    public static int InsertEvent(Game game, string id, int slot)
    {
        var album = AlbumOf(game);
        return Insert(album, FindEvent(album, id), slot);
    }

    /// <summary>
    /// Puts a Wonder Card file in the album, as <c>insertEvent</c> does. A card PKHeX doesn't know still goes in.
    /// Fails with <c>unparseable</c> and <c>wrong-generation</c> as <c>previewFile</c> does, <c>not-supported</c> for the Lock Capsule card, which doesn't go in the album,
    /// and <c>out-of-range</c> and <c>no-slot</c> as <c>insertEvent</c> does.
    /// </summary>
    /// <param name="slot">Zero-based album slot.</param>
    /// <returns>The slot the card landed in.</returns>
    [Command("mysteryGift.insertFile", Topics.MysteryGift)]
    public static int InsertFile(Game game, byte[] bytes, int slot)
    {
        var album = AlbumOf(game);
        return Insert(album, Refusing(() => album.ReadFile(bytes)), slot);
    }

    /// <summary>
    /// Empties an album slot. The cards after it move up, so the album never has a gap. In Generation 4, a gift still waiting for the delivery person goes with its card.
    /// Removing from an empty slot does nothing. Fails with <c>out-of-range</c> for a slot outside the album.
    /// </summary>
    /// <param name="slot">Zero-based album slot.</param>
    [Command("mysteryGift.remove", Topics.MysteryGift)]
    public static void Remove(Game game, int slot) => InRange(() => AlbumOf(game).Remove(slot));

    /// <summary>
    /// Shows or hides Mystery Gift in a Generation 4 game's menu. Fails with <c>not-supported</c> in Generation 5, which has no unlock of its own.
    /// </summary>
    [Command("mysteryGift.setUnlocked", Topics.MysteryGift)]
    public static void SetUnlocked(Game game, bool value)
    {
        try
        {
            AlbumOf(game).SetUnlocked(value);
        }
        catch (NotSupportedException e)
        {
            throw new EngineException(ErrorCodes.NotSupported, e.Message, e);
        }
    }

    /// <summary>
    /// Sets or clears the save's mark that it received a card number. Fails with <c>out-of-range</c> for a number the save has no mark for.
    /// </summary>
    /// <param name="cardId">The card's number, as <c>cardId</c> reports it.</param>
    [Command("mysteryGift.setReceived", Topics.MysteryGift)]
    public static void SetReceived(Game game, int cardId, bool value) => InRange(() => AlbumOf(game).SetReceived(cardId, value));

    private static MysteryGiftAlbum AlbumOf(Game game) =>
        game.MysteryGifts ?? throw new EngineException(ErrorCodes.NotSupported, "Only Generation 4 and 5 saves have a Wonder Card album.");

    private static WonderCard FindEvent(MysteryGiftAlbum album, string id) =>
        album.FindEvent(id) ?? throw new EngineException(ErrorCodes.NotFound, $"There is no event {id} for this game.");

    private static int Insert(MysteryGiftAlbum album, WonderCard card, int slot) => Refusing(() => InRange(() => album.Insert(card, slot)));

    private static T Refusing<T>(Func<T> read)
    {
        try
        {
            return read();
        }
        catch (WonderCardRefusedException e)
        {
            var code = e.Reason switch
            {
                WonderCardRefusal.NotACard => ErrorCodes.Unparseable,
                WonderCardRefusal.WrongGeneration => ErrorCodes.WrongGeneration,
                WonderCardRefusal.NotForTheAlbum => ErrorCodes.NotSupported,
                WonderCardRefusal.DeliveryQueueFull => ErrorCodes.NoSlot,
                _ => ErrorCodes.Unexpected,
            };
            throw new EngineException(code, e.Message, e);
        }
    }

    private static void InRange(Action action) => InRange(() =>
    {
        action();
        return true;
    });

    private static T InRange<T>(Func<T> func)
    {
        try
        {
            return func();
        }
        catch (ArgumentOutOfRangeException e)
        {
            throw new EngineException(ErrorCodes.OutOfRange, e.Message, e);
        }
    }
}
