using System.Reflection;
using System.Runtime.CompilerServices;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Cfru.RadicalRed;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;
using Xunit.Sdk;

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

    public override IEnumerable<object[]> GetData(MethodInfo testMethod) => TestedFiles
        .Except(Except)
        .Select(p => new object[] { p });

    private static readonly string[] TestedFiles =
    [
        SaveFilePath.HgSs,
        SaveFilePath.LetsGoPikachu, SaveFilePath.LetsGoEevee,
        SaveFilePath.Emerald, SaveFilePath.Crystal, SaveFilePath.FireRed,
        SaveFilePath.Unbound, SaveFilePath.RadicalRed,
    ];
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

    public static IReadOnlyList<string> All { get; } =
        [Yellow, Crystal, Emerald, FireRed, HgSs, LetsGoPikachu, LetsGoEevee, Unbound, RadicalRed];

    // Radical Red only possibly matches its format, so loading it without a choice asks for one.
    public static Game Load(string path) =>
        Game.LoadFrom(File.ReadAllBytes(path), path, path == RadicalRed ? new RadicalRedFormat() : null);

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
    }
}
