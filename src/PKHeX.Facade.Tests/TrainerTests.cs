using FluentAssertions;
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
}
