using System.Reflection;
using FluentAssertions;
using PKHeX.Core;
using Xunit.Sdk;

namespace PKHeX.Facade.Tests.Base;

public static class GameFixture
{
    public static void SaveAndReload(this Game game, Action<Game> afterReload)
    {
        var byteArray = game.ToByteArray();
        var reloadedSave = SaveUtil.GetVariantSAV(byteArray, game.SaveFile.Metadata.FilePath);
        reloadedSave.Should().NotBeNull();

        var reloadedGame = new Game(reloadedSave!);
        afterReload(reloadedGame);
    }
}

public class SupportedSaveFilesAttribute : DataAttribute
{
    public GameVersion[] Except { get; set; } = [];
    
    public override IEnumerable<object[]> GetData(MethodInfo testMethod) => TestedVersions
        .Except(Except)
        .Select(SaveFilePath.PathFrom)
        .Distinct()
        .Select(p => new object[] { p });

    private static readonly GameVersion[] TestedVersions =
    [
        GameVersion.HG, GameVersion.SS, GameVersion.HGSS,
        GameVersion.GP, GameVersion.GG, GameVersion.GE,
        GameVersion.E, GameVersion.C,
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

    public static string PathFrom(GameVersion version) => version switch
    {
        GameVersion.RBY => Yellow,
        GameVersion.HGSS or GameVersion.HG or GameVersion.SS => HgSs,
        GameVersion.GG or GameVersion.GP => LetsGoPikachu,
        GameVersion.GE => LetsGoEevee,
        GameVersion.E => Emerald,
        GameVersion.C => Crystal,
        _ => throw new InvalidOperationException($"{version} not yet supported on tests"),
    };
}