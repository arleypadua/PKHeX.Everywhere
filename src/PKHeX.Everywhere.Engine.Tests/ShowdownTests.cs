using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class ShowdownTests
{
    [Theory]
    [SupportedSaveFiles]
    public void PartyShowdownMatchesTheFacade(string saveFile)
    {
        var session = Loaded(saveFile);

        Value(Dispatcher.Dispatch(session, "party.showdown", "[]"))!.GetValue<string>()
            .Should().Be(session.Game!.Trainer.Party.Pokemons.Showdown());
    }

    [Theory]
    [SupportedSaveFiles]
    public void PokemonShowdownMatchesTheFacadeForAPartyPokemon(string saveFile)
    {
        var session = Loaded(saveFile);

        Value(Dispatcher.Dispatch(session, "pokemon.showdown", Args(PokemonHandle.Party(0))))!.GetValue<string>()
            .Should().Be(session.Game!.Trainer.Party.Pokemons[0].Showdown());
    }

    [Theory]
    [SupportedSaveFiles(Except = [GameVersion.GE])]
    public void PokemonShowdownMatchesTheFacadeForABoxPokemon(string saveFile)
    {
        var session = Loaded(saveFile);
        var (at, index) = FirstBoxPokemon(session.Game!)!.Value;

        Value(Dispatcher.Dispatch(session, "pokemon.showdown", Args(at)))!.GetValue<string>()
            .Should().Be(session.Game!.Trainer.PokemonBox.All[index].Showdown());
    }

    [Fact]
    public void PokemonShowdownReturnsNotFoundForAnEmptySlot() =>
        Error(Dispatcher.Dispatch(Loaded(SaveFilePath.HgSs), "pokemon.showdown", Args(PokemonHandle.Party(6))))
            .Should().Be("not-found");

    [Fact]
    public void PartyShowdownReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatcher.Dispatch(new Session(), "party.showdown", "[]")).Should().Be("no-save");
}
