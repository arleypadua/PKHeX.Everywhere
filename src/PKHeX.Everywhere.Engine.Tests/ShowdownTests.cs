using AwesomeAssertions;
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
    [SupportedSaveFiles(Except = [SaveFilePath.Unbound, SaveFilePath.RadicalRed])] // the hacks don't support Showdown
    public void PartyShowdownMatchesTheFacade(string saveFile)
    {
        var session = Loaded(saveFile);

        Value(Dispatch(session, "party.showdown", "[]"))!.GetValue<string>()
            .Should().Be(session.Game!.Trainer.Party.Pokemons.Showdown());
    }

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Unbound, SaveFilePath.RadicalRed])] // the hacks don't support Showdown
    public void BoxShowdownCoversTheBoxedPokemon(string saveFile)
    {
        var session = Loaded(saveFile);

        Value(Dispatch(session, "box.showdown", "[]"))!.GetValue<string>()
            .Should().Be(BoxedPokemon(session.Game!).Select(p => p.Pokemon).Showdown());
    }

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Unbound, SaveFilePath.RadicalRed])] // the hacks don't support Showdown
    public void PokemonShowdownMatchesTheFacadeForAPartyPokemon(string saveFile)
    {
        var session = Loaded(saveFile);

        Value(Dispatch(session, "pokemon.showdown", Args(PokemonHandle.Party(0))))!.GetValue<string>()
            .Should().Be(session.Game!.Trainer.Party.Pokemons[0].Showdown());
    }

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Unbound, SaveFilePath.RadicalRed])] // the hacks don't support Showdown
    public void PokemonShowdownMatchesTheFacadeForABoxPokemon(string saveFile)
    {
        var session = Loaded(saveFile);
        var (at, index) = FirstBoxPokemon(session.Game!)!.Value;

        Value(Dispatch(session, "pokemon.showdown", Args(at)))!.GetValue<string>()
            .Should().Be(session.Game!.Trainer.PokemonBox.All[index].Showdown());
    }

    [Fact]
    public void PokemonShowdownReturnsNotFoundForAnEmptySlot() =>
        Error(Dispatch(Loaded(SaveFilePath.HgSs), "pokemon.showdown", Args(PokemonHandle.Party(6))))
            .Should().Be("not-found");

    [Fact]
    public void PartyShowdownReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "party.showdown", "[]")).Should().Be("no-save");
}
