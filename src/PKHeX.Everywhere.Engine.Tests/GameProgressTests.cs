using System.Text.Json;
using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class GameProgressTests
{
    [Theory]
    [InlineData(SaveFilePath.Yellow)]
    [InlineData(SaveFilePath.Crystal)]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.FireRed)]
    [InlineData(SaveFilePath.RadicalRed)]
    public void ReportsThePlayTimeBadgesAndPokedexOfTheSave(string saveFile)
    {
        var session = Loaded(saveFile);
        var save = session.Game!.SaveFile;
        var badges = session.Game.Progress.Badges;

        Value(Dispatch(session, "game.progress", "[]"))!.ToJsonString().Should().Be(JsonSerializer.Serialize(new
        {
            playTime = new { hours = save.PlayedHours, minutes = save.PlayedMinutes, seconds = save.PlayedSeconds },
            badges = badges is null ? null : new { earned = badges.Earned, total = badges.Total },
            pokedex = save.HasPokeDex ? new { seen = save.SeenCount, caught = save.CaughtCount } : null,
        }));
    }

    [Fact]
    public void ReturnsNullForWhatTheSaveCannotReport() =>
        Value(Dispatch(Loaded(SaveFilePath.RadicalRed), "game.progress", "[]"))!.ToJsonString()
            .Should().Be("""{"playTime":{"hours":47,"minutes":30,"seconds":56},"badges":null,"pokedex":null}""");

    [Fact]
    public void ReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "game.progress", "[]")).Should().Be("no-save");
}
