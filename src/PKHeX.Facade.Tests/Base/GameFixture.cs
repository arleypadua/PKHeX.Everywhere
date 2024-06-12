using System.Reflection;
using FluentAssertions;
using PKHeX.Core;
using Xunit.Sdk;

namespace PKHeX.Facade.Tests.Base;

public static class GameFixture
{
    public static Game LoadTestGame(string saveFilePath)
    {
        var saveFileObject = FileUtil.GetSupportedFile(saveFilePath);
        saveFileObject.Should().NotBeNull();
        var saveFile = saveFileObject.Should().BeAssignableTo<SaveFile>().Subject!;

        return new Game(saveFile);
    }

    public static void SaveAndReload(this Game game, Action<Game> afterReload)
    {
        var byteArray = game.ToByteArray();
        var reloadedSave = SaveUtil.GetVariantSAV(byteArray);
        reloadedSave.Should().NotBeNull();

        var reloadedGame = new Game(reloadedSave!);
        afterReload(reloadedGame);
    }
}

public class SupportedSaveFilesAttribute : DataAttribute
{
    public override IEnumerable<object[]> GetData(MethodInfo testMethod) =>
    [
        ["./data/savedata_4hggs.dsv"],
        ["./data/savedata_7b.bin"],
    ];
}