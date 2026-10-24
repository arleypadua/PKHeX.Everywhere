using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class MalformedSaveTests
{
    [Fact]
    public void LoadFrom_ShouldReportGameNotLoaded_WhenBytesMatchASaveSizeButAreNotASave()
    {
        var notASave = new byte[File.ReadAllBytes(SaveFilePath.HgSs).Length];
        Array.Fill(notASave, (byte)0xFF);

        var loading = () => Game.LoadFrom(notASave, "not-a-save.dsv");

        loading.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void LoadFrom_ShouldReportGameNotLoaded_WhenBytesAreNotACredibleSaveAtAll()
    {
        var loading = () => Game.LoadFrom([1, 2, 3, 4], "not-a-save.txt");

        loading.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void LoadFrom_ShouldStillLoadAValidSave()
    {
        var game = Game.LoadFrom(File.ReadAllBytes(SaveFilePath.HgSs), "savedata_4hgss.dsv");

        game.Trainer.Should().NotBeNull();
    }
}
