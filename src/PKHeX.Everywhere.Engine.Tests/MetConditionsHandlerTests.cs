using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class MetConditionsHandlerTests
{
    private static readonly PokemonHandle Draft = PokemonHandle.Draft();

    [Theory]
    [SupportedSaveFiles]
    public void OriginGamesReturnWhatTheSaveAllows(string saveFile)
    {
        var session = Loaded(saveFile);

        Value(Dispatch(session, "game.originGames", "[]"))!.ToJsonString()
            .Should().Be(Serialized(session.Game!.Options.OriginGames));
    }

    [Fact]
    public void OriginGamesReturnNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "game.originGames", "[]")).Should().Be("no-save");

    [Theory]
    [SupportedSaveFiles]
    public void DetailsReturnTheMetConditions(string saveFile)
    {
        var session = Loaded(saveFile);
        var pokemon = session.Game!.Trainer.Party.Pokemons[0].Pkm;

        var details = Details(session, PokemonHandle.Party(0));

        details["version"]!.GetValue<int>().Should().Be((int)pokemon.Version);
        details["metLocation"]!.GetValue<int>().Should().Be(pokemon.MetLocation);
        details["metLevel"]!.GetValue<int>().Should().Be(pokemon.MetLevel);
        details["metDate"]?.GetValue<string>().Should().Be(pokemon.MetDate?.ToString("yyyy-MM-dd"));
        details["fatefulEncounter"]!.GetValue<bool>().Should().Be(pokemon.FatefulEncounter);
    }

    [Fact]
    public void UpdateAppliesTheMetConditionsToTheDraft()
    {
        var session = Loaded(SaveFilePath.HgSs);
        Edit(session, PokemonHandle.Party(0));

        Update(session, Draft, new { version = (int)GameVersion.Pt, metLocation = 20, metLevel = 12, metDate = "2021-03-04", fatefulEncounter = true });

        var details = Details(session, Draft);
        details["version"]!.GetValue<int>().Should().Be((int)GameVersion.Pt);
        details["metLocation"]!.GetValue<int>().Should().Be(20);
        details["metLevel"]!.GetValue<int>().Should().Be(12);
        details["metDate"]!.GetValue<string>().Should().Be("2021-03-04");
        details["fatefulEncounter"]!.GetValue<bool>().Should().BeTrue();
        Details(session, PokemonHandle.Party(0))["metDate"]!.GetValue<string>().Should().NotBe("2021-03-04");
    }

    [Theory]
    [InlineData(SaveFilePath.HgSs, "version", 50, "Origin game")]
    [InlineData(SaveFilePath.HgSs, "metLocation", 5000, "Met location")]
    [InlineData(SaveFilePath.HgSs, "metLevel", 101, "Met level")]
    [InlineData(SaveFilePath.HgSs, "metDate", "04/03/2021", "Met date")]
    [InlineData(SaveFilePath.HgSs, "metDate", "1999-01-01", "Met date")]
    [InlineData(SaveFilePath.Emerald, "metDate", "2021-03-04", "Met date")]
    public void UpdateRejectsAValueTheSaveCantHoldNamingTheField(string saveFile, string field, object value, string named)
    {
        var session = Loaded(saveFile);
        Edit(session, PokemonHandle.Party(0));
        var before = Details(session, Draft);

        var result = JsonNode.Parse(Dispatch(session, "pokemon.update", Args(Draft, new Dictionary<string, object> { [field] = value, ["nickname"] = "Sparky" })))!;

        result["error"]!["code"]!.GetValue<string>().Should().Be("invalid-patch");
        result["error"]!["message"]!.GetValue<string>().Should().StartWith(named);
        Details(session, Draft).ToJsonString().Should().Be(before.ToJsonString());
    }

    [Theory]
    [SupportedSaveFiles]
    public void OptionsReturnTheMetLocationsOfTheOriginGame(string saveFile)
    {
        var session = Loaded(saveFile);
        var expected = session.Game!.Trainer.Party.Pokemons[0].Options();

        Options(session, PokemonHandle.Party(0))["metLocations"]!.ToJsonString().Should().Be(Serialized(expected.MetLocations));
    }

    [Fact]
    public void OptionsFollowTheOriginGameOfTheDraft()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var at = Enumerable.Range(0, session.Game!.Trainer.Party.Pokemons.Count)
            .Select(PokemonHandle.Party)
            .First(at => session.Game.Trainer.Party.Pokemons[at.Slot].Pkm.Version is GameVersion.HG or GameVersion.SS);
        Edit(session, at);
        var before = Options(session, Draft)["metLocations"]!.ToJsonString();
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Update(session, Draft, new { version = (int)GameVersion.D });

        changes.Should().ContainSingle().Which.Should().Contain(Topics.Draft);
        Options(session, Draft)["metLocations"]!.ToJsonString().Should().NotBe(before);
        Options(session, Draft)["metLocations"]!.ToJsonString().Should().Be(JsonSerializer.Serialize(
            GameInfo.GetLocationList(GameVersion.D, EntityContext.Gen4).DistinctBy(l => l.Value).Select(l => new { id = l.Value, name = l.Text })));
    }

    private static string Serialized(IEnumerable<Facade.Pokemons.Choice> choices) =>
        JsonSerializer.Serialize(choices.Select(c => new { id = c.Id, name = c.Name }));

    private static void Edit(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.edit", Args(at)));

    private static void Update(Session session, PokemonHandle at, object patch) => Value(Dispatch(session, "pokemon.update", Args(at, patch)));

    private static JsonNode Details(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.details", Args(at)))!;

    private static JsonNode Options(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.options", Args(at)))!;
}
