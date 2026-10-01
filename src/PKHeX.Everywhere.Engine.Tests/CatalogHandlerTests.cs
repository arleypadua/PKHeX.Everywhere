using System.Text.Json.Nodes;
using AwesomeAssertions;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class CatalogHandlerTests
{
    [Fact]
    public void NamesReturnsKnownSpeciesAndItemsWithoutASave()
    {
        var names = Names("""{ "speciesIds": [25, 150], "itemIds": [1, 2] }""");

        names["species"]!.ToJsonString().Should().Be("""[{"id":25,"name":"Pikachu"},{"id":150,"name":"Mewtwo"}]""");
        names["items"]!.ToJsonString().Should().Be("""[{"id":1,"name":"Master Ball"},{"id":2,"name":"Ultra Ball"}]""");
    }

    [Fact]
    public void NamesGivesAnUnknownItemAPlaceholderName()
    {
        var names = Names("""{ "speciesIds": [], "itemIds": [65000] }""");

        names["items"]!.ToJsonString().Should().Be("""[{"id":65000,"name":"Unknown Item 65000"}]""");
    }

    [Fact]
    public void NamesLeavesOutAnUnknownSpecies()
    {
        var names = Names("""{ "speciesIds": [25, 65000], "itemIds": [] }""");

        names["species"]!.AsArray().Select(s => s!["id"]!.GetValue<int>()).Should().Equal(25);
    }

    private static JsonNode Names(string request) =>
        Value(Dispatcher.Dispatch(new Session(), "catalog.names", $"[{request}]"))!;
}
