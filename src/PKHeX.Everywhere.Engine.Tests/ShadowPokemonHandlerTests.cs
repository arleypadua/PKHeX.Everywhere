using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class ShadowPokemonHandlerTests
{
    private static readonly PokemonHandle Slot = PokemonHandle.InBox(0, 0);

    [Theory]
    [InlineData(GameVersion.COLO)]
    [InlineData(GameVersion.XD)]
    public void DetailsReportAShadowPokemon(GameVersion version)
    {
        var session = Loaded(version);

        Shadow(Details(session, Slot)).Should().Be("""{"isShadow":true,"shadowId":10,"purification":3000}""");
    }

    [Theory]
    [InlineData(GameVersion.COLO)]
    [InlineData(GameVersion.XD)]
    public void APokemonPurifiedInTheDraftStaysPurifiedAfterExport(GameVersion version)
    {
        var session = Loaded(version);
        Value(Dispatch(session, "pokemon.edit", Args(Slot)));
        Value(Dispatch(session, "pokemon.update", Args(PokemonHandle.Draft(), new { isShadow = false })));
        Value(Dispatch(session, "pokemon.commit", "[]"));

        var exported = Value(Dispatch(session, "game.export", "[]"))!["bytes"]!.GetValue<string>();
        Value(Dispatch(session, "game.load", Args(exported, "shadow.gci", null!)));

        Shadow(Details(session, Slot)).Should().Be("""{"isShadow":false,"shadowId":10,"purification":0}""");
    }

    [Fact]
    public void UpdateRejectsPurifyingOutsideColosseumAndXD()
    {
        var session = new Session();
        session.Load(SaveFilePath.Load(SaveFilePath.Emerald), "emerald.sav");

        Error(Dispatch(session, "pokemon.update", Args(PokemonHandle.Party(0), new { isShadow = false }))).Should().Be("invalid-patch");
    }

    private static Session Loaded(GameVersion version)
    {
        var session = new Session();
        session.Load(GameFixture.WithShadowPokemon(version), "shadow.gci");
        return session;
    }

    private static JsonNode Details(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.details", Args(at)))!;

    private static string Shadow(JsonNode details) => new JsonObject
    {
        ["isShadow"] = details["isShadow"]!.DeepClone(),
        ["shadowId"] = details["shadowId"]!.DeepClone(),
        ["purification"] = details["purification"]!.DeepClone(),
    }.ToJsonString();
}
