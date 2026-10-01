using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class DispatcherTests
{
    [Theory]
    [SupportedSaveFiles]
    public void PartyGetReturnsTheLoadedParty(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        var session = new Session();
        session.Load(game, saveFile);

        var party = Value(Dispatcher.Dispatch(session, "party.get", "[]"))!.AsArray();

        var expected = game.Trainer.Party.Pokemons;
        party.Should().HaveCount(expected.Count);
        for (var slot = 0; slot < expected.Count; slot++)
        {
            var pokemon = party[slot]!;
            var form = expected[slot].Form.Form;
            pokemon["id"]!.GetValue<string>().Should().Be(expected[slot].UniqueId.Value);
            pokemon["at"]!.ToJsonString().Should().Be($$"""{"source":"party","slot":{{slot}},"box":null}""");
            pokemon["speciesId"]!.GetValue<int>().Should().Be(expected[slot].Species.Id);
            pokemon["species"]!.GetValue<string>().Should().Be(expected[slot].Species.Name);
            pokemon["form"]!.ToJsonString().Should().Be(JsonSerializer.Serialize(new { id = (int)form.Id, name = form.Name }));
            pokemon["nickname"]!.GetValue<string>().Should().Be(expected[slot].Nickname);
            pokemon["level"]!.GetValue<int>().Should().Be(expected[slot].Level);
            pokemon["isShiny"]!.GetValue<bool>().Should().Be(expected[slot].IsShiny);
        }
    }

    [Fact]
    public void ArgumentsCanBeOmitted()
    {
        var session = new Session();
        session.Load(Game.LoadFrom(SaveFilePath.HgSs), SaveFilePath.HgSs);

        Value(Dispatcher.Dispatch(session, "party.get", ""))!.AsArray().Should().NotBeEmpty();
    }

    [Fact]
    public void ReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatcher.Dispatch(new Session(), "party.get", "[]")).Should().Be("no-save");

    [Fact]
    public void ReturnsUnknownCallForUnregisteredNames() =>
        Error(Dispatcher.Dispatch(new Session(), "nope.get", "[]")).Should().Be("unknown-call");

    [Theory]
    [InlineData("{}")]
    [InlineData("not json")]
    [InlineData("[1]")]
    public void ReturnsBadArgumentsForMalformedArguments(string args)
    {
        var session = new Session();
        session.Load(Game.LoadFrom(SaveFilePath.HgSs), SaveFilePath.HgSs);

        Error(Dispatcher.Dispatch(session, "party.get", args)).Should().Be("bad-arguments");
    }

    [Fact]
    public void ReturnsUnexpectedWhenAHandlerThrows()
    {
        var result = Dispatcher.Dispatch(new Session(), "party.get", "[]",
            (_, _, _, _) => throw new InvalidOperationException("boom"));

        result.Should().Be("""{"ok":false,"error":{"code":"unexpected","message":"boom"}}""");
    }
}
