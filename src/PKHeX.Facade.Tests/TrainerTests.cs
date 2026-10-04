using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class TrainerTests
{
    [Theory]
    [SupportedSaveFiles]
    public void TrainerData_ShouldBeParsed(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        game.Trainer.Gender.Should().Be(Gender.Male);
        game.Trainer.Name.Should().NotBeNull();
        game.Trainer.Money.Amount.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(SaveFilePath.Unbound, 48855u, 16608u)]
    [InlineData(SaveFilePath.RadicalRed, 10334u, 56690u)]
    [InlineData(SaveFilePath.Imperium, 27859u, 47663u)]
    public void Id_OfAGen3RomHack_ShowsTheSixteenBitIds(string saveFile, uint tid, uint sid)
    {
        var game = SaveFilePath.Load(saveFile);

        game.Trainer.Id.Should().Be(new EntityId(tid, sid));
    }

    [Theory]
    [SupportedSaveFiles]
    public void Name_ShouldPersistAfterReload(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);

        game.Trainer.Name = "Ash";

        game.SaveAndReload(reloaded => reloaded.Trainer.Name.Should().Be("Ash"));
    }

    [Theory]
    [SupportedSaveFiles]
    public void Name_LongerThanMax_ShouldBeTruncated(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var max = game.SaveFile.MaxStringLengthTrainer;

        game.Trainer.Name = new string('A', max + 5);

        game.SaveAndReload(reloaded => reloaded.Trainer.Name.Should().Be(new string('A', max)));
    }
}
