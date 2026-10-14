using PKHeX.Core;

namespace PKHeX.Facade.Transfers;

// PKHeX's ConvertToPK7 rolls the IVs, PID, encryption constant and memory at random, and reads the origin game, language and region from global settings.
// They are redone here from the saves on both ends, seeded so the same Pokémon always arrives the same.
internal static class VirtualConsoleTransporter
{
    private const int BankMemory = 4;

    public static TransferRefusal? Rework(GBPKM from, SaveFile source, PK7 arrives, SaveFile destination)
    {
        if (from is PK2 { HeldItem: not 0 }) return TransferRefusal.HoldingItem;

        var language = Language(from, source);
        if (!SetRegion(arrives, destination, language)) return TransferRefusal.LanguageMismatch;

        arrives.Language = language;
        if (!arrives.IsNicknamed) arrives.Nickname = SpeciesName.GetSpeciesNameGeneration(arrives.Species, language, 7);
        if (from.OriginalTrainerTrash[0] == StringConverter1.TradeOTCode) arrives.OriginalTrainerName = StringConverter12Transporter.GetTradeNameGen1(language);
        // PKHeX gives a Pokémon caught in Crystal, which alone records where, Crystal as its origin.
        if (Origin(source) is { } origin && from is not PK2 { HasOriginalMetLocation: true }) arrives.Version = origin;

        var rng = new Xoroshiro128Plus(TransferConversion.Seed(from), (ulong)from.Species << 32 | from.EXP);
        arrives.EncryptionConstant = (uint)rng.Next();
        arrives.PID = (uint)rng.Next();
        // As PKHeX's SetTransferPID: a shiny becomes a square shiny against a secret ID of 0, and any other loses its shininess.
        if (from.IsShiny && !arrives.IsShiny)
        {
            var low = arrives.PID & 0xFFFF;
            arrives.PID = (low ^ arrives.TID16) << 16 | low;
        }
        else if (!from.IsShiny && arrives.IsShiny)
        {
            arrives.PID ^= 0x1000_0000;
        }

        SetIVs(arrives, from.Species is (ushort)Species.Mew or (ushort)Species.Celebi ? 5 : 3, ref rng);

        byte feeling;
        do feeling = (byte)rng.NextInt(10);
        while (!MemoryContext6.CanHaveFeeling6(BankMemory, feeling, 0));
        arrives.HandlingTrainerMemoryFeeling = feeling;

        arrives.RefreshChecksum();
        return null;
    }

    private static int Language(GBPKM pk, SaveFile source)
    {
        var hint = source is SAV1 or SAV2 && source.Language > 0 ? source.Language : (int)LanguageID.English;
        var language = pk.IsSpeciesNameMatch(hint) ? hint : pk.GuessedLanguage(hint);
        if ((language == (int)LanguageID.Japanese) != pk.Japanese) language = pk.Japanese ? (int)LanguageID.Japanese : (int)LanguageID.English;
        return language;
    }

    // A save that can't tell Red from Blue, or Gold from Silver, leaves PKHeX's default.
    private static GameVersion? Origin(SaveFile source) =>
        source is SAV1 or SAV2 && source.Version is GameVersion.RD or GameVersion.BU or GameVersion.GN or GameVersion.YW or GameVersion.GD or GameVersion.SI or GameVersion.C
            ? source.Version
            : null;

    // Transporter ran on the console the Pokémon arrives on, which could only run Virtual Console games of its region.
    private static bool SetRegion(PK7 pk, SaveFile destination, int language)
    {
        if (destination is not IRegionOriginReadOnly region || !Locale3DS.IsRegionLockedLanguageValidVC(region.ConsoleRegion, language)) return false;

        (pk.ConsoleRegion, pk.Country, pk.Region) = (region.ConsoleRegion, region.Country, region.Region);
        (pk.Geo1_Country, pk.Geo1_Region) = (pk.Country, pk.Region);
        return true;
    }

    private static void SetIVs(PK7 pk, int flawless, ref Xoroshiro128Plus rng)
    {
        Span<int> ivs = stackalloc int[6];
        for (var i = 0; i < ivs.Length; i++) ivs[i] = i < flawless ? 31 : (int)rng.NextInt(32);
        for (var i = ivs.Length - 1; i > 0; i--)
        {
            var j = (int)rng.NextInt((ulong)i + 1);
            (ivs[i], ivs[j]) = (ivs[j], ivs[i]);
        }
        pk.SetIVs(ivs);
    }
}
