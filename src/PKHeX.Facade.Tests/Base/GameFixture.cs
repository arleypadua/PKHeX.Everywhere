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
    public override IEnumerable<object[]> GetData(MethodInfo testMethod) =>
    [
        // [SaveFilePath.Gen1], still need to fix tests 
        [SaveFilePath.Gen4],
        [SaveFilePath.Gen7],
    ];

    public static class SaveFilePath
    {
        public const string Gen1 = "./data/savedata_1.sav"; // yellow
        public const string Gen4 = "./data/savedata_4hgss.dsv"; // soul silver
        public const string Gen7 = "./data/savedata_7b.bin"; // let's go pikachu
    }
}