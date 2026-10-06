using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Repositories;
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
    [InlineData(GameVersion.RD, false)]
    [InlineData(GameVersion.YW, false)]
    [InlineData(GameVersion.GD, false)]
    [InlineData(GameVersion.SI, false)]
    [InlineData(GameVersion.C, true)]
    [InlineData(GameVersion.E, true)]
    public void HasGender_SaysWhetherTheGameHasATrainerGender(GameVersion version, bool expected) =>
        Game.EmptyOf(GameVersionRepository.Instance.Get(version)).Trainer.HasGender.Should().Be(expected);

    [Fact]
    public void HasGender_IsFalseForPokemonStadium()
    {
        new Game(new SAV1Stadium()).Trainer.HasGender.Should().BeFalse();
        new Game(new SAV2Stadium()).Trainer.HasGender.Should().BeFalse();
    }

    [Fact]
    public void HasGender_IsFalseForTheYellowFixture() =>
        SaveFilePath.Load(SaveFilePath.Yellow).Trainer.HasGender.Should().BeFalse();

    [Theory]
    [SupportedSaveFiles]
    public void HasGender_IsTrueForTheSupportedFixtures(string saveFile) =>
        SaveFilePath.Load(saveFile).Trainer.HasGender.Should().BeTrue();

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
