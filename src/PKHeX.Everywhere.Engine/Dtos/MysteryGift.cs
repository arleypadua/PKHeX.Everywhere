using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// What a Wonder Card gives.
/// </summary>
public enum GiftKind
{
    /// <summary>A Pokémon or an egg.</summary>
    Pokemon,

    /// <summary>An item, key items such as the Member Card included.</summary>
    Item,

    /// <summary>Something that is neither, such as a seal, an accessory, a Pokétch app or a Pokéwalker route. The title says what it is.</summary>
    Other,
}

/// <summary>
/// An item a Wonder Card gives.
/// </summary>
/// <param name="Id">PKHeX item id.</param>
public record GiftItem(int Id, string Name);

/// <summary>
/// A Wonder Card in the save's album.
/// </summary>
/// <param name="Title">The title the card shows in the game.</param>
/// <param name="Species">The species of the Pokémon the card gives, or null when it gives none.</param>
/// <param name="Item">The item the card gives, or null when it gives none.</param>
/// <param name="Language">The language code the card is for, such as <c>ENG</c>, <c>JPN</c> or <c>FRE</c>. Null when the card doesn't say, as Generation 4 item cards don't.</param>
/// <param name="CardId">The card's number, which the save's received marks go by.</param>
/// <param name="Known">The card matches an event in PKHeX's event database.</param>
public record WonderCardEntry(string Title, GiftKind Kind, SpeciesEntry? Species, GiftItem? Item, string? Language, int CardId, bool Known);

/// <summary>
/// A slot in the save's Wonder Card album.
/// </summary>
/// <param name="Index">Zero-based slot, as <c>mysteryGift.insertEvent</c> and <c>mysteryGift.remove</c> take it.</param>
/// <param name="Card">The card in the slot, or null when it's empty.</param>
public record AlbumSlot(int Index, WonderCardEntry? Card);

/// <summary>
/// The save's Wonder Card album, returned by <c>mysteryGift.status</c>.
/// </summary>
/// <param name="Unlocked">Whether the game shows Mystery Gift in its menu. Null in Generation 5, where the menu shows it once the player has the Pokédex.</param>
/// <param name="Slots">Every slot in order, empty ones included: 3 in Generation 4, 12 in Generation 5. The filled slots come first.</param>
public record GiftAlbum(bool? Unlocked, AlbumSlot[] Slots);

/// <summary>
/// An event from PKHeX's event database that the save's game can receive, as <c>mysteryGift.events</c> lists it.
/// </summary>
/// <param name="Id">The event's id, to pass to <c>mysteryGift.previewEvent</c> and <c>mysteryGift.insertEvent</c>. Made from the card's bytes, so it stays the same across releases.</param>
/// <param name="Title">The title the card shows in the game.</param>
/// <param name="Species">The species of the Pokémon the card gives, or null when it gives none.</param>
/// <param name="Item">The item the card gives, or null when it gives none.</param>
/// <param name="Language">The language code the card is for, such as <c>ENG</c>. Null when the card doesn't say, as Generation 4 item cards don't.</param>
/// <param name="CardId">The card's number. Distributions in several languages often share it.</param>
/// <param name="Received">The save is marked as having received a card with this number.</param>
public record GiftEvent(string Id, string Title, GiftKind Kind, SpeciesEntry? Species, GiftItem? Item, string? Language, int CardId, bool Received);

/// <summary>
/// A Wonder Card as the save would receive it, returned by <c>mysteryGift.previewEvent</c> and <c>mysteryGift.previewFile</c>.
/// </summary>
/// <param name="Title">The title the card shows in the game.</param>
/// <param name="Pokemon">The Pokémon as the delivery person would hand it to the save's trainer, or null when the card gives none. A random PID or IVs differ from what the game rolls.</param>
/// <param name="Item">The item the card gives, or null when it gives none.</param>
/// <param name="Language">The language code the card is for, such as <c>ENG</c>. Null when the card doesn't say.</param>
/// <param name="CardId">The card's number. A <c>.pgt</c> file has no card, so it reads 0.</param>
/// <param name="Known">The card matches an event in PKHeX's event database. Unknown cards still insert.</param>
public record WonderCardPreview(string Title, GiftKind Kind, PokemonPreview? Pokemon, GiftItem? Item, string? Language, int CardId, bool Known);

public static class MysteryGiftMapping
{
    public static GiftAlbum ToDto(this MysteryGiftAlbum album) => new(
        album.Unlocked,
        album.Slots.Select((card, index) => new AlbumSlot(index, card?.ToEntry())).ToArray());

    public static GiftEvent ToDto(this MysteryGiftEvent entry) => new(
        entry.Id,
        entry.Card.Title,
        entry.Card.Kind.ToDto(),
        entry.Card.Species?.ToEntry(),
        entry.Card.Item.ToDto(),
        entry.Card.Language,
        entry.Card.CardId,
        entry.Received);

    public static WonderCardPreview ToPreview(this WonderCard card, Game game) => new(
        card.Title,
        card.Kind.ToDto(),
        card.ToPokemon(game)?.ToPreview(),
        card.Item.ToDto(),
        card.Language,
        card.CardId,
        card.Known);

    private static WonderCardEntry ToEntry(this WonderCard card) => new(
        card.Title,
        card.Kind.ToDto(),
        card.Species?.ToEntry(),
        card.Item.ToDto(),
        card.Language,
        card.CardId,
        card.Known);

    private static GiftItem? ToDto(this Facade.Repositories.ItemDefinition? item) => item is null ? null : new GiftItem(item.Id, item.Name);

    private static GiftKind ToDto(this WonderCardKind kind) => kind switch
    {
        WonderCardKind.Pokemon => GiftKind.Pokemon,
        WonderCardKind.Item => GiftKind.Item,
        WonderCardKind.Other => GiftKind.Other,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };
}
