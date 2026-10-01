using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;

namespace PKHeX.Everywhere.Engine.Tests;

public class GameHandlerTests
{
    [Fact]
    public void GetReturnsNullWithoutALoadedSave() =>
        Value(Dispatch(new Session(), "game.get", "[]")).Should().BeNull();

    [Fact]
    public void GetSummarisesTheLoadedSave()
    {
        var session = new Session();
        session.Load(Game.LoadFrom(SaveFilePath.HgSs), "soulsilver.dsv");

        Value(Dispatch(session, "game.get", "[]"))!.ToJsonString()
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

        Value(Dispatch(session, "game.load", Args(Convert.ToBase64String(File.ReadAllBytes(SaveFilePath.Emerald)), "emerald.sav")))
            .Should().BeNull();

        Value(Dispatch(session, "game.get", "[]"))!["generation"]!.GetValue<int>().Should().Be(3);
        session.FileName.Should().Be("emerald.sav");
        changes.Should().BeEquivalentTo([new[] { Topics.All }]);
        gameChanges.Should().Be(1);
    }

    [Fact]
    public void LoadReturnsInvalidSaveForBytesThatAreNotASave() =>
        Error(Dispatch(new Session(), "game.load", Args(Convert.ToBase64String(new byte[1234]), "nope.sav")))
            .Should().Be("invalid-save");

    [Fact]
    public void ExportedBytesLoadBackWithTheSameTrainer()
    {
        var session = EngineCalls.Loaded(SaveFilePath.HgSs);
        var published = new List<IEngineEvent>();
        session.Published += published.Add;

        var exported = Value(Dispatch(session, "game.export", "[]"))!;

        exported["fileName"]!.GetValue<string>().Should().Be(SaveFilePath.HgSs);
        published.Should().Equal(new GameExported());
        var reloaded = new Session();
        Value(Dispatch(reloaded, "game.load", Args(exported["bytes"]!.GetValue<string>(), "exported.dsv")));
        reloaded.Game!.Trainer.Name.Should().Be(session.Game!.Trainer.Name);
        reloaded.Game.Trainer.Id.Should().Be(session.Game.Trainer.Id);
    }

    [Fact]
    public void ExportReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "game.export", "[]")).Should().Be("no-save");

    [Fact]
    public void BlankVersionsHaveNoAggregatedVersion()
    {
        var versions = Value(Dispatch(new Session(), "game.blankVersions", "[]"))!.AsArray();

        var ids = versions.Select(v => v!["id"]!.GetValue<int>()).ToList();
        ids.Should().NotBeEmpty();
        ids.Should().NotContain(id => GameVersionRepository.Instance.Get(id).Aggregated);
        versions.Select(v => v!["name"]!.GetValue<string>()).Should().BeInAscendingOrder();
    }

    [Fact]
    public void LoadBlankLoadsTheVersionNamedAfterIt()
    {
        var session = new Session();
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Value(Dispatch(session, "game.loadBlank", Args((int)GameVersion.SW))).Should().BeNull();

        session.Game!.SaveVersion.Version.Should().Be(GameVersion.SW);
        session.FileName.Should().Be(GameVersionRepository.Instance.Get(GameVersion.SW).Name);
        changes.Should().BeEquivalentTo([new[] { Topics.All }]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData((int)GameVersion.HGSS)]
    public void LoadBlankReturnsNotFoundForAnUnknownVersion(int version) =>
        Error(Dispatch(new Session(), "game.loadBlank", Args(version))).Should().Be("not-found");

    [Fact]
    public void CloseUnloadsTheSave()
    {
        var session = new Session();
        session.Load(Game.LoadFrom(SaveFilePath.HgSs), SaveFilePath.HgSs);
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Value(Dispatch(session, "game.close", "[]")).Should().BeNull();

        Value(Dispatch(session, "game.get", "[]")).Should().BeNull();
        Error(Dispatch(session, "party.get", "[]")).Should().Be("no-save");
        changes.Should().BeEquivalentTo([new[] { Topics.All }]);
    }
}
