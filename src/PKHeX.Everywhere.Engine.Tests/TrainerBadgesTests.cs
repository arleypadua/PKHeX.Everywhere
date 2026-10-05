using System.Text.Json;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class TrainerBadgesTests
{
    [Theory]
    [InlineData(SaveFilePath.Yellow)]
    [InlineData(SaveFilePath.Crystal)]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.FireRed)]
    public void BadgesListsTheBadgesOfTheSave(string saveFile)
    {
        var session = Loaded(saveFile);

        Value(Dispatch(session, "trainer.badges", "[]"))!.ToJsonString().Should().Be(JsonSerializer.Serialize(
            session.Game!.Badges!.All.Select(b => new { name = b.Name, earned = b.Earned })));
    }

    [Theory]
    [InlineData(SaveFilePath.HgSs)]
    [InlineData(SaveFilePath.RadicalRed)]
    public void BadgesIsNullWhenTheSaveCannotWriteThem(string saveFile) =>
        Value(Dispatch(Loaded(saveFile), "trainer.badges", "[]")).Should().BeNull();

    [Theory]
    [InlineData(SaveFilePath.Yellow)]
    [InlineData(SaveFilePath.Crystal)]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.FireRed)]
    public void SetBadgesChangesTheEarnedBadges(string saveFile)
    {
        var session = Loaded(saveFile);
        var earned = session.Game!.Badges!.All.Select(b => b.Earned).ToArray();
        earned[0] = !earned[0];

        Value(Dispatch(session, "trainer.setBadges", Args(earned))).Should().BeNull();

        Value(Dispatch(session, "game.progress", "[]"))!["badges"]!["earned"]!.GetValue<int>().Should().Be(earned.Count(e => e));
        PKHeXBadges(session.Game.SaveFile).Should().Be(earned.Select((e, i) => e ? 1 << i : 0).Sum());
    }

    [Fact]
    public void SetBadgesReturnsOutOfRangeForTheWrongNumberOfBadges() =>
        Error(Dispatch(Loaded(SaveFilePath.Crystal), "trainer.setBadges", Args(new bool[8]))).Should().Be("out-of-range");

    [Theory]
    [InlineData(SaveFilePath.HgSs)]
    [InlineData(SaveFilePath.RadicalRed)]
    public void SetBadgesReturnsNotSupportedWhenTheSaveCannotWriteThem(string saveFile) =>
        Error(Dispatch(Loaded(saveFile), "trainer.setBadges", Args(new bool[8]))).Should().Be("not-supported");

    private static int PKHeXBadges(SaveFile save) => save switch
    {
        SAV1 gen1 => gen1.Badges,
        SAV2 gen2 => gen2.Badges,
        SAV3 gen3 => gen3.Badges,
        _ => throw new InvalidOperationException(),
    };
}
