using System.Collections.Frozen;
using System.Security.Cryptography;
using PKHeX.Core;

namespace PKHeX.Facade;

internal static class MysteryGiftDatabase
{
    internal sealed record Entry(string Id, DataMysteryGift Gift);

    private static readonly Lazy<IReadOnlyList<Entry>> Gen4 = new(() => Index("pcd", EncounterEvent.MGDB_G4));
    private static readonly Lazy<IReadOnlyList<Entry>> Gen5 = new(() => Index("pgf", EncounterEvent.MGDB_G5));
    private static readonly Lazy<FrozenSet<string>> KnownCards = new(() =>
        Gen4.Value.Concat(Gen5.Value).Select(entry => KeyOf(new WonderCard(entry.Gift))).ToFrozenSet());

    internal static readonly GameVersion[] Gen4Versions = [GameVersion.D, GameVersion.P, GameVersion.Pt, GameVersion.HG, GameVersion.SS];
    private static readonly GameVersion[] Gen5Versions = [GameVersion.B, GameVersion.W, GameVersion.B2, GameVersion.W2];

    public static IEnumerable<Entry> ReceivableBy(SaveFile save)
    {
        var versions = (save.Generation == 4 ? Gen4Versions : Gen5Versions)
            .Where(version => save.Version == version || save.Version.Contains(version))
            .ToArray();

        var compatible = Gen4Compatibility(versions);
        return save.Generation switch
        {
            4 => Gen4.Value.Where(entry => entry.Gift is PCD { CanConvertToPGT: true } pcd && (pcd.CardCompatibility & compatible) != 0),
            5 => Gen5.Value.Where(entry => versions.Any(((PGF)entry.Gift).CanBeReceivedByVersion)),
            _ => [],
        };
    }

    // A card lists the games that can receive it as bits by GameVersion.
    internal static ushort Gen4Compatibility(IEnumerable<GameVersion> versions) =>
        (ushort)versions.Aggregate(0, (bits, version) => bits | (1 << (int)version));

    public static bool Knows(WonderCard card) => KnownCards.Value.Contains(KeyOf(card));

    // Saved and shared copies of a card differ from PKHeX's in the encryption of the gift and in the dates, so a card is known by what it shows.
    private static string KeyOf(WonderCard card) =>
        $"{card.Generation}|{card.CardId}|{card.Title}|{card.Kind}|{card.Species?.Id}|{card.Item?.Id}";

    private static IReadOnlyList<Entry> Index(string prefix, IEnumerable<DataMysteryGift> gifts) => gifts
        .Select(gift => new Entry(IdOf(prefix, gift), gift))
        .DistinctBy(entry => entry.Id)
        .ToList();

    private static string IdOf(string prefix, DataMysteryGift gift)
    {
        var hash = Convert.ToHexStringLower(SHA256.HashData(gift.Data)[..4]);
        return new WonderCard(gift).Language is { } language
            ? $"{prefix}-{gift.CardID}-{language}-{hash}"
            : $"{prefix}-{gift.CardID}-{hash}";
    }
}
