using System.Reflection;
using System.Runtime.CompilerServices;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Everywhere.RomHacks.Cfru.RadicalRed;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;
using PKHeX.Everywhere.RomHacks.Expansion.Imperium;
using Xunit.Sdk;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade.Tests.Base;

public static class GameFixture
{
    public static void SaveAndReload(this Game game, Action<Game> afterReload)
    {
        var format = game.Format is null ? SaveFormats.PKHeX : SaveFormats.Find(game.Format.Id);
        var reloadedGame = Game.LoadFrom(game.ToByteArray(), game.SaveFile.Metadata.FilePath, format);
        reloadedGame.Should().NotBeNull();

        afterReload(reloadedGame);
    }
}

public class SupportedSaveFilesAttribute : DataAttribute
{
    public string[] Except { get; set; } = [];

    public override IEnumerable<object[]> GetData(MethodInfo testMethod) => SaveFilePath.All
        .Except([SaveFilePath.Yellow, .. Except])
        .Select(p => new object[] { p });
}

public class GamesAttribute(params GameVersion[] versions) : DataAttribute
{
    public override IEnumerable<object[]> GetData(MethodInfo testMethod) => versions.Select(v => new object[]
    {
        Game.LoadFrom(SaveFilePath.PathFrom(v))
    });
}

public static class SaveFilePath
{
    public const string Yellow = "./data/save/savedata_1.sav"; // yellow
    public const string HgSs = "./data/save/savedata_4hgss.dsv"; // soul silver
    public const string LetsGoPikachu = "./data/save/savedata_7b.bin"; // let's go pikachu
    public const string LetsGoEevee = "./data/save/savedata_7b_lge.bin";
    public const string Emerald = "./data/save/emerald.sav"; // emerald
    public const string Crystal = "./data/save/crystal.sav"; // crystal
    public const string FireRed = "./data/save/firered.sav";
    public const string Unbound = "./data/save/unbound.sav"; // Unbound 2.0
    public const string RadicalRed = "./data/save/radicalred.sav";
    public const string Imperium = "./data/save/imperium.sav"; // Emerald Imperium 1.3
    public const string UnboundUnknownSpecies = "./data/save/unbound-unknown-species.sav"; // Shadow Warrior in box 23, slot 19

    public static IReadOnlyList<string> All { get; } =
        [Yellow, Crystal, Emerald, FireRed, HgSs, LetsGoPikachu, LetsGoEevee, Unbound, RadicalRed];

    // The fixture's slot starts at sector id 18. Rotated so id 0 comes first, PKHeX reads it as Emerald.
    public static byte[] ImperiumReadableAsEmerald()
    {
        var fixture = File.ReadAllBytes(Imperium);
        var data = fixture.ToArray();
        var first = Enumerable.Range(0, 28).First(sector => ReadUInt16LittleEndian(fixture.AsSpan((sector * 0x1000) + 0xFF4)) == 0);
        for (var sector = 0; sector < 28; sector++)
            fixture.AsSpan(((sector + first) % 28) * 0x1000, 0x1000).CopyTo(data.AsSpan(sector * 0x1000));

        return data;
    }

    public static Game Load(string path) => Game.LoadFrom(File.ReadAllBytes(path), path, FormatOf(path));

    // Radical Red only possibly matches its format, so loading it without a choice asks for one.
    public static ISaveFormat? FormatOf(string path) => path == RadicalRed ? new RadicalRedFormat() : null;

    public static string PathFrom(GameVersion version) => version switch
    {
        GameVersion.RBY => Yellow,
        GameVersion.HGSS or GameVersion.HG or GameVersion.SS => HgSs,
        GameVersion.GG or GameVersion.GP => LetsGoPikachu,
        GameVersion.GE => LetsGoEevee,
        GameVersion.E => Emerald,
        GameVersion.C => Crystal,
        GameVersion.FR => FireRed,
        _ => throw new InvalidOperationException($"{version} not yet supported on tests"),
    };
}

internal static class RomHackFormats
{
    [ModuleInitializer]
    internal static void Register()
    {
        SaveFormats.Register(new UnboundFormat());
        SaveFormats.Register(new RadicalRedFormat());
        SaveFormats.Register(new ImperiumFormat(), enabled: false);
    }
}
