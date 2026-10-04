using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class TrainerTests
{
    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Imperium])] // Imperium's money isn't read yet
    public void TrainerData_ShouldBeParsed(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        game.Trainer.Gender.Should().Be(Gender.Male);
        game.Trainer.Name.Should().NotBeNull();
        game.Trainer.Money.Amount.Should().BeGreaterThan(0);
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
