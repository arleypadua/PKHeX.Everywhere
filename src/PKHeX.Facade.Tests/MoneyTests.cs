using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class MoneyTests
{
    [Fact]
    public void BlankLegendsZA_MoneyIsNotSupported_AndSetDoesNotThrow()
    {
        var game = Game.EmptyOf(GameVersionRepository.Instance.Get(GameVersion.ZA));

        game.Trainer.Money.IsSupported.Should().BeFalse();
        game.Invoking(g => g.Trainer.Money.Set(1234)).Should().NotThrow();
        game.Invoking(g => g.Trainer.Money.SetMax()).Should().NotThrow();
    }

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Imperium])] // Imperium's money isn't read yet
    public void Set_ShouldPersistAfterReload(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        game.Trainer.Money.IsSupported.Should().BeTrue();

        game.Trainer.Money.Set(1234);

        game.SaveAndReload(reloaded => reloaded.Trainer.Money.Amount.Should().Be(1234));
    }
}
