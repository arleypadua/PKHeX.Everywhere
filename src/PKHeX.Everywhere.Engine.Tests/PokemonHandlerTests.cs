using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class PokemonHandlerTests
{
    [Theory]
    [SupportedSaveFiles]
    public void GetReturnsThePokemonAtTheHandle(string saveFile)
    {
        var session = Loaded(saveFile);

        var pokemon = Value(Dispatcher.Dispatch(session, "pokemon.get", Args(PokemonHandle.Party(0))))!;

        var party = Value(Dispatcher.Dispatch(session, "party.get", "[]"))!.AsArray();
        pokemon.ToJsonString().Should().Be(party[0]!.ToJsonString());
    }

    [Theory]
    [SupportedSaveFiles]
    public void SetLevelChangesAPartyPokemonInTheSave(string saveFile)
    {
        var session = Loaded(saveFile);
        var level = OtherLevel(session.Game!.Trainer.Party.Pokemons[0].Level);

        Value(Dispatcher.Dispatch(session, "pokemon.setLevel", Args(PokemonHandle.Party(0), level))).Should().BeNull();

        Level(session, PokemonHandle.Party(0)).Should().Be(level);
        session.Game.SaveAndReload(reloaded => reloaded.Trainer.Party.Pokemons[0].Level.Should().Be(level));
    }

    [Theory]
    [SupportedSaveFiles(Except = [GameVersion.GE])]
    public void SetLevelChangesABoxPokemonInTheSave(string saveFile)
    {
        var session = Loaded(saveFile);
        var (at, index) = FirstBoxPokemon(session.Game!)!.Value;
        var level = OtherLevel(session.Game!.Trainer.PokemonBox.All[index].Level);

        Value(Dispatcher.Dispatch(session, "pokemon.setLevel", Args(at, level))).Should().BeNull();

        Level(session, at).Should().Be(level);
        session.Game.SaveAndReload(reloaded => reloaded.Trainer.PokemonBox.All[index].Level.Should().Be(level));
    }

    [Fact]
    public void SetLevelChangesTheTopicOfTheHandle()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var (at, _) = FirstBoxPokemon(session.Game!)!.Value;
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Dispatcher.Dispatch(session, "pokemon.setLevel", Args(PokemonHandle.Party(0), 42));
        Dispatcher.Dispatch(session, "pokemon.setLevel", Args(at, 42));

        changes.Should().BeEquivalentTo([new[] { Topics.Party }, new[] { $"box/{at.Box}" }], o => o.WithStrictOrdering());
    }

    [Fact]
    public void AFailedCommandChangesNothing()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Dispatcher.Dispatch(session, "pokemon.setLevel", Args(PokemonHandle.Party(0), 101));

        changes.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void SetLevelRejectsLevelsOutsideOneToHundred(int level) =>
        Error(Dispatcher.Dispatch(Loaded(SaveFilePath.HgSs), "pokemon.setLevel", Args(PokemonHandle.Party(0), level)))
            .Should().Be("out-of-range");

    public static TheoryData<PokemonHandle> MissingHandles() =>
    [
        PokemonHandle.Party(-1),
        PokemonHandle.Party(6),
        PokemonHandle.InBox(-1, 0),
        PokemonHandle.InBox(0, 1000),
        PokemonHandle.InBox(1000, 0),
        new(SlotSource.Box, 0),
    ];

    [Theory]
    [MemberData(nameof(MissingHandles))]
    public void ReturnsNotFoundForHandlesWithoutAPokemon(PokemonHandle at)
    {
        var session = Loaded(SaveFilePath.HgSs);

        Error(Dispatcher.Dispatch(session, "pokemon.get", Args(at))).Should().Be("not-found");
        Error(Dispatcher.Dispatch(session, "pokemon.setLevel", Args(at, 42))).Should().Be("not-found");
    }

    [Fact]
    public void SetLevelReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatcher.Dispatch(new Session(), "pokemon.setLevel", Args(PokemonHandle.Party(0), 42))).Should().Be("no-save");

    [Fact]
    public void SetLevelReturnsBadArgumentsForAMalformedHandle() =>
        Error(Dispatcher.Dispatch(Loaded(SaveFilePath.HgSs), "pokemon.setLevel", """[{"source":"daycare","slot":0}, 42]"""))
            .Should().Be("bad-arguments");

    private static int Level(Session session, PokemonHandle at) =>
        Value(Dispatcher.Dispatch(session, "pokemon.get", Args(at)))!["level"]!.GetValue<int>();

    private static int OtherLevel(int level) => level == 50 ? 51 : 50;
}
