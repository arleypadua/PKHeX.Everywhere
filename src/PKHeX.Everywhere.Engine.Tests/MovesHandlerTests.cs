using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class MovesHandlerTests
{
    private static readonly PokemonHandle Draft = PokemonHandle.Draft();

    [Theory]
    [SupportedSaveFiles]
    public void DetailsReturnTheMoveSlots(string saveFile)
    {
        var session = Loaded(saveFile);
        var pokemon = session.Game!.Trainer.Party.Pokemons[0];

        var moves = Details(session, PokemonHandle.Party(0))["moves"]!.AsArray();

        moves.Select(m => m!["id"]!.GetValue<int>()).Should().Equal(pokemon.Moves.Values.Select(m => (int)m.Move.Id));
        moves.Select(m => m!["name"]!.GetValue<string>()).Should().Equal(pokemon.Moves.Values.Select(m => m.Move.Name));
        moves.Select(m => m!["maxPp"]!.GetValue<int>()).Should().Equal(pokemon.Moves.Values.Select(m => m.PP.Max));
    }

    [Fact]
    public void UpdateAppliesTheMovesOnTheDraft()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var move = session.Game!.Options.Moves.First(m => m.Id == (int)Move.Thunderbolt).Id;
        Edit(session, PokemonHandle.Party(0));
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Update(session, Draft, new { moves = new[] { 0, move, 0, 0 } });

        changes.Should().ContainSingle().Which.Should().Contain(Topics.Draft);
        Details(session, Draft)["moves"]!.AsArray().Select(m => m!["id"]!.GetValue<int>()).Should().Equal(move, 0, 0, 0);
        Details(session, PokemonHandle.Party(0))["moves"]!.AsArray().Select(m => m!["id"]!.GetValue<int>()).Should().NotEqual([move, 0, 0, 0]);
    }

    [Fact]
    public void MoveOptionsFollowTheDraftAfterAnUpdate()
    {
        var session = Loaded(SaveFilePath.HgSs);
        Edit(session, PokemonHandle.Party(0));
        Update(session, Draft, new { level = 1 });
        var before = MoveOptions(session);

        Update(session, Draft, new { level = 100 });

        MoveOptions(session).Should().Contain(id => !before.Contains(id));
    }

    [Fact]
    public void UpdateIgnoresAnAllEmptyMoveset()
    {
        var session = Loaded(SaveFilePath.HgSs);
        Edit(session, PokemonHandle.Party(0));
        var before = Details(session, Draft)["moves"]!.ToJsonString();

        Update(session, Draft, new { moves = new[] { 0, 0, 0, 0 } });

        Details(session, Draft)["moves"]!.ToJsonString().Should().Be(before);
    }

    [Fact]
    public void UpdateRejectsAMoveTheGameDoesntHave()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Edit(session, PokemonHandle.Party(0));
        var before = Details(session, Draft);

        var result = JsonNode.Parse(Dispatch(session, "pokemon.update", Args(Draft, new { moves = new[] { 5000, 0, 0, 0 } })))!;

        result["error"]!["code"]!.GetValue<string>().Should().Be("invalid-patch");
        Details(session, Draft).ToJsonString().Should().Be(before.ToJsonString());
    }

    private static void Edit(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.edit", Args(at)));

    private static void Update(Session session, PokemonHandle at, object patch) => Value(Dispatch(session, "pokemon.update", Args(at, patch)));

    private static JsonNode Details(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.details", Args(at)))!;

    private static List<int> MoveOptions(Session session) =>
        Value(Dispatch(session, "pokemon.options", Args(Draft)))!["moves"]!.AsArray().Select(m => m!["id"]!.GetValue<int>()).ToList();
}
