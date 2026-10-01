using AwesomeAssertions;
using PKHeX.Facade;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;
using static PKHeX.Everywhere.Engine.Tests.PokemonHandlerTests;

namespace PKHeX.Everywhere.Engine.Tests;

public class GameHandlerTests
{
    [Fact]
    public void GetReturnsNullWithoutALoadedSave() =>
        Value(Dispatcher.Dispatch(new Session(), "game.get", "[]")).Should().BeNull();

    [Fact]
    public void GetSummarisesTheLoadedSave()
    {
        var session = new Session();
        session.Load(Game.LoadFrom(SaveFilePath.HgSs), "soulsilver.dsv");

        Value(Dispatcher.Dispatch(session, "game.get", "[]"))!.ToJsonString()
            .Should().Be($$"""{"fileName":"soulsilver.dsv","version":"{{session.Game!.GameVersionApproximation.Name}}","generation":4}""");
    }

    [Fact]
    public void LoadLoadsTheSaveAndChangesEverything()
    {
        var session = new Session();
        var changes = new List<string[]>();
        var gameChanges = 0;
        session.Changed += changes.Add;
        session.GameChanged += () => gameChanges++;

        Value(Dispatcher.Dispatch(session, "game.load", Args(Convert.ToBase64String(File.ReadAllBytes(SaveFilePath.Emerald)), "emerald.sav")))
            .Should().BeNull();

        Value(Dispatcher.Dispatch(session, "game.get", "[]"))!["generation"]!.GetValue<int>().Should().Be(3);
        session.FileName.Should().Be("emerald.sav");
        changes.Should().BeEquivalentTo([new[] { Topics.All }]);
        gameChanges.Should().Be(1);
    }

    [Fact]
    public void LoadReturnsInvalidSaveForBytesThatAreNotASave() =>
        Error(Dispatcher.Dispatch(new Session(), "game.load", Args(Convert.ToBase64String(new byte[1234]), "nope.sav")))
            .Should().Be("invalid-save");

    [Fact]
    public void CloseUnloadsTheSave()
    {
        var session = new Session();
        session.Load(Game.LoadFrom(SaveFilePath.HgSs), SaveFilePath.HgSs);
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Value(Dispatcher.Dispatch(session, "game.close", "[]")).Should().BeNull();

        Value(Dispatcher.Dispatch(session, "game.get", "[]")).Should().BeNull();
        Error(Dispatcher.Dispatch(session, "party.get", "[]")).Should().Be("no-save");
        changes.Should().BeEquivalentTo([new[] { Topics.All }]);
    }
}
