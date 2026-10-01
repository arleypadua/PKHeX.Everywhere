using System.Text.Json;
using AwesomeAssertions;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class SpeciesHandlerTests
{
    [Theory]
    [SupportedSaveFiles]
    public void ListReturnsTheSpeciesTheBlazorEncounterSearchOffers(string saveFile)
    {
        var session = Loaded(saveFile);

        var species = Value(Dispatch(session, "species.list", "[]"))!.AsArray();

        var expected = session.Game!.SpeciesRepository.AllGameSpecies
            .Where(SpeciesDefinition.IsSome)
            .Select(s => JsonSerializer.Serialize(new { id = s.Id, name = s.Name }));
        species.Select(s => s!.ToJsonString()).Should().BeEquivalentTo(expected);
        species.Select(s => s!["name"]!.GetValue<string>()).Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    [Theory]
    [InlineData(SaveFilePath.Crystal, 251)]
    [InlineData(SaveFilePath.Emerald, 386)]
    [InlineData(SaveFilePath.HgSs, 493)]
    public void ListLeavesOutNone(string saveFile, int count)
    {
        var species = Value(Dispatch(Loaded(saveFile), "species.list", "[]"))!.AsArray();

        species.Should().HaveCount(count);
        species.Select(s => s!["id"]!.GetValue<int>()).Should().NotContain(0);
        species.Select(s => s!["name"]!.GetValue<string>()).Should().Contain("Abra");
    }

    [Fact]
    public void ListReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "species.list", "[]")).Should().Be("no-save");
}
