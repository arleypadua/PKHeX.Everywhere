using System.Runtime.CompilerServices;
using FluentAssertions;
using PKHeX.Core;

namespace PKHeX.Facade.Tests;

public static class GameFixture
{
    public static Game CreateTestGame()
    {
        var saveFileObject = FileUtil.GetSupportedFile("./data/savedata.bin");
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
