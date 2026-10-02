using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class BoxHandlerTests
{
    [Theory]
    [SupportedSaveFiles]
    public void GetReturnsEveryBoxedPokemonInBoxAndSlotOrder(string saveFile)
    {
        var session = Loaded(saveFile);
        var game = session.Game!;

        var box = Value(Dispatch(session, "box.get", "[]"))!.AsArray();

        var expected = BoxedPokemon(game).ToList();
        box.Select(p => p!["id"]!.GetValue<string>()).Should().Equal(expected.Select(p => p.Pokemon.UniqueId.Value));
        box.Select(p => p!["at"]!.ToJsonString()).Should().Equal(expected.Select(p => Args(p.At)[1..^1]));
        box.Select(p => p!["species"]!.GetValue<string>()).Should().Equal(expected.Select(p => p.Pokemon.Species.Name));
    }

    [Fact]
    public void GetFailsWithNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "box.get", "[]")).Should().Be("no-save");

    [Theory]
    [SupportedSaveFiles]
    public void AddFromFileAddsAPokemonExportedFromTheSaveToTheFirstEmptyBoxSlot(string saveFile)
    {
        var session = Loaded(saveFile);
        var exported = session.Game!.Trainer.Party.Pokemons[0];

        AddsToTheFirstEmptyBoxSlot(session, exported.ToFile().Bytes, exported.Species.Name);
    }

    [Fact]
    public void AddFromFileAddsAPokemonFileToTheFirstEmptyBoxSlot() =>
        AddsToTheFirstEmptyBoxSlot(Loaded(SaveFilePath.LetsGoPikachu), File.ReadAllBytes(PokemonFile.Venusaur), "Venusaur");

    [Fact]
    public void AddFromFileFailsWithUnparseableWhenTheBytesAreNotAPokemon() =>
        FailsWith(Loaded(SaveFilePath.Emerald), [1, 2, 3], "unparseable");

    [Fact]
    public void AddFromFileFailsWithConversionFailedWhenThePokemonCannotMoveToTheSaveFormat() =>
        FailsWith(Loaded(SaveFilePath.HgSs), File.ReadAllBytes(PokemonFile.Golduck), "conversion-failed");

    [Fact]
    public void AddFromFileFailsWithNotInGameWhenTheGameDoesNotHaveTheSpecies()
    {
        var chikorita = new PB7 { Species = (ushort)Species.Chikorita, Version = GameVersion.GP };
        chikorita.RefreshChecksum();
        var bytes = new byte[chikorita.SIZE_PARTY];
        chikorita.WriteDecryptedDataParty(bytes);

        FailsWith(Loaded(SaveFilePath.LetsGoPikachu), bytes, "not-in-game");
    }

    [Theory]
    [SupportedSaveFiles]
    public void AddFromFileFailsWithBoxFullWhenNoBoxSlotIsEmpty(string saveFile)
    {
        var session = Loaded(saveFile);
        var save = session.Game!.SaveFile;
        var exported = session.Game.Trainer.Party.Pokemons[0];
        for (var index = save.NextOpenBoxSlot(); index >= 0; index = save.NextOpenBoxSlot())
            save.SetBoxSlotAtIndex(exported.Pkm.Clone(), index);

        FailsWith(session, exported.ToFile().Bytes, "box-full");
    }

    [Fact]
    public void AddFromFileFailsWithNoSaveWithoutALoadedSave() =>
        FailsWith(new Session(), File.ReadAllBytes(PokemonFile.Golduck), "no-save");

    private static void AddsToTheFirstEmptyBoxSlot(Session session, byte[] bytes, string species)
    {
        var game = session.Game!;
        var index = game.SaveFile.NextOpenBoxSlot();
        var slots = game.SaveFile.BoxSlotCount;
        var expected = PokemonHandle.InBox(index / slots, index % slots);
        var changes = new List<string[]>();
        var published = new List<IEngineEvent>();
        session.Changed += changes.Add;
        session.Published += published.Add;

        var added = Value(Dispatch(session, "box.addFromFile", Args(Convert.ToBase64String(bytes))))!;

        added["at"]!.ToJsonString().Should().Be(Args(expected)[1..^1]);
        var pokemon = Value(Dispatch(session, "pokemon.get", Args(expected)))!;
        pokemon["species"]!.GetValue<string>().Should().Be(species);
        added["id"]!.GetValue<string>().Should().Be(pokemon["id"]!.GetValue<string>());
        changes.Should().ContainSingle().Which.Should().Equal(Topics.Box);
        published.Should().Equal(new PokemonAdded(expected, PokemonAddSource.File, game.Trainer.PokemonBox.All[index].ToOverview()));
    }

    private static void FailsWith(Session session, byte[] bytes, string code)
    {
        var published = new List<IEngineEvent>();
        session.Published += published.Add;

        Error(Dispatch(session, "box.addFromFile", Args(Convert.ToBase64String(bytes)))).Should().Be(code);

        published.Should().BeEmpty();
    }
}
