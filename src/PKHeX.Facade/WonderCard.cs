using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade;

public enum WonderCardKind
{
    Pokemon,
    Item,
    Other,
}

public sealed class WonderCard
{
    internal WonderCard(DataMysteryGift gift) => Gift = gift;

    internal DataMysteryGift Gift { get; }

    public int Generation => Gift.Generation;

    public string Title => Gift.CardTitle.Replace('　', ' ').Trim();

    public int CardId => Gift.CardID;

    public WonderCardKind Kind => Gift switch
    {
        PCD { Gift.IsEntity: true } or PGF { IsEntity: true } => WonderCardKind.Pokemon,
        _ when ItemId is not null => WonderCardKind.Item,
        _ => WonderCardKind.Other,
    };

    public SpeciesDefinition? Species => Kind == WonderCardKind.Pokemon ? SpeciesRepository.Find(Gift.Species) : null;

    public ItemDefinition? Item => ItemId is { } id ? ItemRepository.GetItem(id) : null;

    // PKHeX's database knows a Generation 5 card's language but the card's bytes don't, so a card from a save or a file takes it from its database copy.
    public string? Language => OwnLanguage ?? MysteryGiftDatabase.LanguageOf(this);

    internal string? OwnLanguage => LanguageCodes.Of(Gift switch
    {
        PCD { Gift.IsEntity: true, Gift.IsManaphyEgg: false } pcd => pcd.Gift.PK.Language,
        PGF pgf => pgf.Language != 0 ? pgf.Language : pgf.RestrictLanguage,
        _ => 0,
    });

    public bool Known => MysteryGiftDatabase.Knows(this);

    public Pokemon? ToPokemon(Game game) => Kind == WonderCardKind.Pokemon ? new Pokemon(Gift.ConvertToPKM(game.SaveFile), game) : null;

    private const ushort OaksLetter = 452;
    private const ushort MemberCard = 454;
    private const ushort AzureFlute = 455;
    private const ushort SecretKey = 467;

    private ushort? ItemId => Gift switch
    {
        PCD pcd => pcd.GiftType switch
        {
            GiftType4.Item => (ushort)pcd.ItemID,
            GiftType4.OaksLetter => OaksLetter,
            GiftType4.MemberCard => MemberCard,
            GiftType4.AzureFlute => AzureFlute,
            GiftType4.SecretKey => SecretKey,
            _ => null,
        },
        PGF { IsItem: true } pgf => (ushort)pgf.ItemID,
        _ => null,
    };
}

public sealed class MysteryGiftEvent
{
    internal MysteryGiftEvent(string id, WonderCard card, bool received)
    {
        Id = id;
        Card = card;
        Received = received;
    }

    // Built from the card's bytes, so it stays the same across releases as long as PKHeX ships the same card.
    public string Id { get; }

    public WonderCard Card { get; }

    public bool Received { get; }
}

public enum WonderCardRefusal
{
    NotACard,
    WrongGeneration,
    NotForTheAlbum,
    DeliveryQueueFull,
}

public class WonderCardRefusedException(WonderCardRefusal reason, string message) : Exception(message)
{
    public WonderCardRefusal Reason { get; } = reason;
}
