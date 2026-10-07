using System.Text.Json.Nodes;
using AwesomeAssertions;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
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

    [Fact]
    public void NamesAbilitiesAndNaturesWithoutASave()
    {
        var names = Names("""{ "speciesIds": [], "itemIds": [], "abilityIds": [22, 0], "natureIds": [3, 0] }""");

        names["abilities"]!.ToJsonString().Should().Be("""[{"id":22,"name":"Intimidate"},{"id":0,"name":"(None)"}]""");
        names["natures"]!.ToJsonString().Should().Be("""[{"id":3,"name":"Adamant"},{"id":0,"name":"Hardy"}]""");
    }

    [Fact]
    public void NamesGivesUnknownAbilitiesAndNaturesAPlaceholderInOrder()
    {
        var names = Names("""{ "speciesIds": [], "itemIds": [], "abilityIds": [65000, 22], "natureIds": [25, 3] }""");

        names["abilities"]!.ToJsonString().Should().Be("""[{"id":65000,"name":"Unknown Ability 65000"},{"id":22,"name":"Intimidate"}]""");
        names["natures"]!.ToJsonString().Should().Be("""[{"id":25,"name":"Unknown Nature 25"},{"id":3,"name":"Adamant"}]""");
    }

    [Fact]
    public void NamesNoAbilitiesOrNaturesWhenNoneAreRequested()
    {
        var names = Names("""{ "speciesIds": [], "itemIds": [] }""");

        names["abilities"]!.AsArray().Should().BeEmpty();
        names["natures"]!.AsArray().Should().BeEmpty();
    }

    [Fact]
    public void NamesAbilitiesAndNaturesTheSameForARomHack()
    {
        var names = Names("""{ "speciesIds": [], "itemIds": [], "abilityIds": [22], "natureIds": [3], "formatId": "unbound" }""");

        names["abilities"]!.ToJsonString().Should().Be("""[{"id":22,"name":"Intimidate"}]""");
        names["natures"]!.ToJsonString().Should().Be("""[{"id":3,"name":"Adamant"}]""");
    }

    private static JsonNode Names(string request) =>
        Value(Dispatch(new Session(), "catalog.names", $"[{request}]"))!;
}
