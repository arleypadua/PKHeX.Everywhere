using System.Text.Json;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Tests.Base;
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

    private static Session Loaded(string saveFile)
    {
        var session = new Session();
        session.Load(Game.LoadFrom(saveFile), saveFile);
        return session;
    }

    private static int Level(Session session, PokemonHandle at) =>
        Value(Dispatcher.Dispatch(session, "pokemon.get", Args(at)))!["level"]!.GetValue<int>();

    private static int OtherLevel(int level) => level == 50 ? 51 : 50;

    // Let's Go keeps party members in box storage, so their box slots alias party slots. The Eevee save has no other box Pokémon.
    internal static (PokemonHandle At, int Index)? FirstBoxPokemon(Game game)
    {
        var all = game.Trainer.PokemonBox.All;
        var index = Enumerable.Range(0, all.Count).FirstOrDefault(i => all[i].Pkm.Species != 0 && !InParty(game, i), -1);
        var slots = game.SaveFile.BoxSlotCount;
        return index < 0 ? null : (PokemonHandle.InBox(index / slots, index % slots), index);
    }

    private static bool InParty(Game game, int boxIndex) =>
        game.SaveFile is SAV7b { Blocks.Storage: var storage } && storage.IsParty(boxIndex);

    internal static string Args(params object[] args) => JsonSerializer.Serialize(args.Select(Arg));

    private static object Arg(object arg) => arg is PokemonHandle at
        ? new { source = at.Source == SlotSource.Party ? "party" : "box", slot = at.Slot, box = at.Box }
        : arg;
}
