using System.Text.Json;
using AwesomeAssertions;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class EncounterHandlerTests
{
    [Theory]
    [SupportedSaveFiles]
    public void VersionsReturnsTheVersionsTheBlazorEncounterSearchOffers(string saveFile)
    {
        var session = Loaded(saveFile);
        var game = session.Game!;

        var result = Value(Dispatcher.Dispatch(session, "encounters.versions", "[]"))!;

        var expected = GameVersionRepository.Instance
            .GetAvailableFor(game.Generation, game.SaveVersion.Version)
            .Select(v => new { id = v.Id, name = v.Name });
        result["versions"]!.ToJsonString().Should().Be(JsonSerializer.Serialize(expected));
        result["default"]!.GetValue<int>().Should().Be(game.GameVersionApproximation.Id);
        result["versions"]!.AsArray().Select(v => v!["id"]!.GetValue<int>()).Should().Contain(game.GameVersionApproximation.Id);
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald, "Emerald")]
    [InlineData(SaveFilePath.HgSs, "SoulSilver")]
    public void VersionsDefaultsToTheSaveVersion(string saveFile, string version)
    {
        var result = Value(Dispatcher.Dispatch(Loaded(saveFile), "encounters.versions", "[]"))!;

        var defaultId = result["default"]!.GetValue<int>();
        result["versions"]!.AsArray().Single(v => v!["id"]!.GetValue<int>() == defaultId)!["name"]!.GetValue<string>()
            .Should().Be(version);
    }

    [Fact]
    public void VersionsReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatcher.Dispatch(new Session(), "encounters.versions", "[]")).Should().Be("no-save");
}
