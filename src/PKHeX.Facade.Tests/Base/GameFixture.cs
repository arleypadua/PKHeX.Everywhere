﻿using System.Reflection;
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
    public override IEnumerable<object[]> GetData(MethodInfo testMethod) =>
    [
        // [SaveFilePath.Gen1], still need to fix tests 
        [SaveFilePath.HgSs],
        [SaveFilePath.LetsGoPikachu],
        [SaveFilePath.Emerald],
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
    public const string Emerald = "./data/save/emerald.sav"; // emerald

    public static string PathFrom(GameVersion version) => version switch
    {
        GameVersion.RBY => Yellow,
        GameVersion.HGSS or GameVersion.HG or GameVersion.SS => HgSs,
        GameVersion.GG or GameVersion.GP or GameVersion.GE => LetsGoPikachu,
        GameVersion.E => Emerald,
        _ => throw new InvalidOperationException($"{version} not yet supported on tests"),
    };
}