using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class CloneHandlerTests
{
    private static readonly PokemonHandle Draft = PokemonHandle.Draft();

    [Fact]
    public void CloneOpensACopyOfTheSavedPokemon()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var saved = session.Game!.Trainer.Party.Pokemons[0];

        Clone(session, PokemonHandle.Party(0));

        Get(session, Draft)["speciesId"]!.GetValue<int>().Should().Be(saved.Species.Id);
        Get(session, Draft)["id"]!.GetValue<string>().Should().NotBe(Get(session, PokemonHandle.Party(0))["id"]!.GetValue<string>());
    }

    [Fact]
    public void CloneReplacesTheOpenDraft()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var (inBox, index) = FirstBoxPokemon(session.Game!)!.Value;
        Edit(session, PokemonHandle.Party(0));

        Clone(session, inBox);

        session.Draft!.Pokemon.Species.Should().Be(session.Game!.Trainer.PokemonBox.All[index].Species);
        Error(Dispatch(session, "pokemon.commit", "[]")).Should().Be("no-slot");
    }

    [Fact]
    public void CloneRejectsTheDraftHandle()
    {
        var session = Loaded(SaveFilePath.HgSs);
        Edit(session, PokemonHandle.Party(0));

        Error(Dispatch(session, "pokemon.clone", Args(Draft))).Should().Be("draft-not-allowed");
    }

    [Theory]
    [SupportedSaveFiles]
    public void AddToBoxWritesTheCloneToTheFirstEmptyBoxSlot(string saveFile)
    {
        var session = Loaded(saveFile);
        var game = session.Game!;
        var index = game.SaveFile.NextOpenBoxSlot();
        var expected = PokemonHandle.InBox(index / game.SaveFile.BoxSlotCount, index % game.SaveFile.BoxSlotCount);
        Clone(session, PokemonHandle.Party(0));
        Update(session, Draft, new { nickname = "Twin" });
        var published = new List<IEngineEvent>();
        session.Published += published.Add;

        var added = Value(Dispatch(session, "pokemon.addToBox", "[]"))!;

        added["at"]!.ToJsonString().Should().Be(JsonNode.Parse(Args(expected))![0]!.ToJsonString());
        added["id"]!.GetValue<string>().Should().Be(Get(session, expected)["id"]!.GetValue<string>());
        Details(session, expected)["nickname"]!.GetValue<string>().Should().Be("Twin");
        published.Should().Equal(new PokemonSaved(expected, game.Trainer.PokemonBox.All[index].ToOverview()));
        Error(Dispatch(session, "pokemon.details", Args(Draft))).Should().Be(ErrorCodes.NoDraft);
        game.SaveAndReload(reloaded => reloaded.Trainer.PokemonBox.All[index].Nickname.Should().Be("Twin"));
    }

    [Theory]
    [SupportedSaveFiles]
    public void AddToBoxFailsWithBoxFullWhenNoBoxSlotIsEmpty(string saveFile)
    {
        var session = Loaded(saveFile);
        var save = session.Game!.SaveFile;
        var filler = session.Game.Trainer.Party.Pokemons[0];
        for (var index = save.NextOpenBoxSlot(); index >= 0; index = save.NextOpenBoxSlot())
            save.SetBoxSlotAtIndex(filler.Pkm.Clone(), index);
        Clone(session, PokemonHandle.Party(0));

        Error(Dispatch(session, "pokemon.addToBox", "[]")).Should().Be("box-full");
        Details(session, Draft).Should().NotBeNull();
    }

    [Fact]
    public void AddToBoxReturnsNoDraftWithoutADraft()
    {
        var session = Loaded(SaveFilePath.HgSs);

        Error(Dispatch(session, "pokemon.addToBox", "[]")).Should().Be(ErrorCodes.NoDraft);
    }

    private static void Clone(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.clone", Args(at)));

    private static void Edit(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.edit", Args(at)));

    private static void Update(Session session, PokemonHandle at, object patch) => Value(Dispatch(session, "pokemon.update", Args(at, patch)));

    private static JsonNode Details(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.details", Args(at)))!;

    private static JsonNode Get(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.get", Args(at)))!;
}
