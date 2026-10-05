using AwesomeAssertions;
using PKHeX.Core;

namespace PKHeX.Facade.Tests;

public class ProgressTests
{
    [Theory]
    [InlineData(SaveFilePath.Yellow, 8, 8)]
    [InlineData(SaveFilePath.Crystal, 7, 16)]
    [InlineData(SaveFilePath.Emerald, 0, 8)]
    [InlineData(SaveFilePath.FireRed, 8, 8)]
    [InlineData(SaveFilePath.HgSs, 2, 16)]
    public void Badges_CountTheBadgesEarnedOutOfTheGames(string saveFile, int earned, int total) =>
        SaveFilePath.Load(saveFile).Progress.Badges.Should().Be(new BadgeCount(earned, total));

    [Theory]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    [InlineData(SaveFilePath.Imperium)]
    public void Badges_AreNullWhenTheyCannotBeRead(string saveFile) =>
        SaveFilePath.Load(saveFile).Progress.Badges.Should().BeNull();

    [Theory]
    [InlineData(SaveFilePath.Yellow)]
    [InlineData(SaveFilePath.Crystal)]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.FireRed)]
    [InlineData(SaveFilePath.HgSs)]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    public void PlayTimeAndPokedex_MatchPKHeX(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var save = game.SaveFile;

        game.Progress.PlayTime.Should().Be(new PlayTime(save.PlayedHours, save.PlayedMinutes, save.PlayedSeconds));
        game.Progress.Pokedex.Should().Be(new PokedexCount(save.SeenCount, save.CaughtCount));
    }

    [Theory]
    [InlineData(SaveFilePath.RadicalRed, 47, 30, 56)]
    [InlineData(SaveFilePath.Unbound, 999, 59, 59)]
    [InlineData(SaveFilePath.Imperium, 189, 57, 39)]
    public void PlayTime_OfARomHack_IsRead(string saveFile, int hours, int minutes, int seconds) =>
        SaveFilePath.Load(saveFile).Progress.PlayTime.Should().Be(new PlayTime(hours, minutes, seconds));

    [Theory]
    [InlineData(SaveFilePath.RadicalRed)]
    [InlineData(SaveFilePath.Unbound)]
    public void PlayTime_OfACfruSave_MatchesPKHeXReadingItAsFireRed(string saveFile)
    {
        var fireRed = SaveUtil.GetSaveFile(File.ReadAllBytes(saveFile))!;

        SaveFilePath.Load(saveFile).Progress.PlayTime
            .Should().Be(new PlayTime(fireRed.PlayedHours, fireRed.PlayedMinutes, fireRed.PlayedSeconds));
    }

    [Fact]
    public void PlayTime_OfImperium_MatchesPKHeXReadingItAsEmerald()
    {
        var emerald = SaveUtil.GetSaveFile(SaveFilePath.ImperiumReadableAsEmerald())!;

        SaveFilePath.Load(SaveFilePath.Imperium).Progress.PlayTime
            .Should().Be(new PlayTime(emerald.PlayedHours, emerald.PlayedMinutes, emerald.PlayedSeconds));
    }

    [Theory]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    [InlineData(SaveFilePath.Imperium)]
    public void Pokedex_OfARomHack_IsNull(string saveFile) =>
        SaveFilePath.Load(saveFile).Progress.Pokedex.Should().BeNull();
}
