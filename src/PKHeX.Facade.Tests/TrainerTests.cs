using FluentAssertions;

namespace PKHeX.Facade.Tests;

public class TrainerTests
{
    [Fact]
    public void TrainerData_ShouldBeParsed()
    {
        var game = GameFixture.CreateTestGame();
        game.Trainer.Gender.Should().Be(Gender.Male);
        game.Trainer.Name.Should().Be("arley");
        game.Trainer.Money.Amount.Should().BeGreaterThan(0);
    }
}
