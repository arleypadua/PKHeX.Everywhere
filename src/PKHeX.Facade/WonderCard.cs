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

    /// <summary>The language code the card is for, such as <c>ENG</c>, or null when the card doesn't say.</summary>
    public string? Language => LanguageCode(Gift switch
    {
        PCD { Gift.IsEntity: true, Gift.IsManaphyEgg: false } pcd => pcd.Gift.PK.Language,
        PGF pgf => pgf.Language != 0 ? pgf.Language : pgf.RestrictLanguage,
        _ => 0,
    });

    public bool Known => MysteryGiftDatabase.Knows(this);

    /// <returns>The Pokémon as the delivery person would hand it to the save's trainer, or null when the card holds no Pokémon.</returns>
    public Pokemon? ToPokemon(Game game) => Kind == WonderCardKind.Pokemon ? new Pokemon(Gift.ConvertToPKM(game.SaveFile), game) : null;

    private ushort? ItemId => Gift switch
    {
        PCD pcd => pcd.GiftType switch
        {
            GiftType4.Item => (ushort)pcd.ItemID,
            GiftType4.OaksLetter => 452,
            GiftType4.MemberCard => 454,
            GiftType4.AzureFlute => 455,
            GiftType4.SecretKey => 467,
            _ => null,
        },
        PGF { IsItem: true } pgf => (ushort)pgf.ItemID,
        _ => null,
    };

    private static string? LanguageCode(int language) => (LanguageID)language switch
    {
        LanguageID.Japanese => "JPN",
        LanguageID.English => "ENG",
        LanguageID.French => "FRE",
        LanguageID.Italian => "ITA",
        LanguageID.German => "GER",
        LanguageID.Spanish => "SPA",
        LanguageID.Korean => "KOR",
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

    /// <summary>Built from the card's bytes, so it stays the same across releases as long as PKHeX ships the same card.</summary>
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
