using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class TrainerTests
{
    [Theory]
    [SupportedSaveFiles]
    public void TrainerData_ShouldBeParsed(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        game.Trainer.Gender.Should().Be(Gender.Male);
        game.Trainer.Name.Should().NotBeNull();
        game.Trainer.Money.Amount.Should().BeGreaterThan(0);
    }

    [Theory]
    [SupportedSaveFiles]
    public void TrainerName_ShouldBeMutable(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        game.Trainer.Name = "NewName";

        game.SaveAndReload(reloaded =>
        {
            reloaded.Trainer.Name.Should().Be("NewName");
        });
    }

    [Theory]
    [SupportedSaveFiles]
    public void TrainerTID_ShouldBeMutable(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        game.Trainer.TID = 12345;

        game.SaveAndReload(reloaded =>
        {
            reloaded.Trainer.TID.Should().Be(12345);
        });
    }

    [Theory]
    [SupportedSaveFiles]
    public void TrainerSID_ShouldBeMutable(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        game.Trainer.SID = 54321;

        game.SaveAndReload(reloaded =>
        {
            reloaded.Trainer.SID.Should().Be(54321);
        });
    }
}
