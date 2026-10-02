using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class EncounterHandlerTests
{
    [Theory]
    [SupportedSaveFiles]
    public void VersionsReturnsTheVersionsTheBlazorEncounterSearchOffers(string saveFile)
    {
        var session = Loaded(saveFile);
        var game = session.Game!;

        var result = Value(Dispatch(session, "encounters.versions", "[]"))!;

        var expected = GameVersionRepository.Instance
            .GetAvailableFor(game.Generation, game.SaveVersion.Version)
            .Select(v => new { id = v.Id, name = v.Name });
        result["versions"]!.ToJsonString().Should().Be(JsonSerializer.Serialize(expected));
        result["default"]!.GetValue<int>().Should().Be(game.GameVersionApproximation.Id);
        result["versions"]!.AsArray().Select(v => v!["id"]!.GetValue<int>()).Should().Contain(game.GameVersionApproximation.Id);
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald, "Emerald")]
    [InlineData(SaveFilePath.HgSs, "SoulSilver")]
    public void VersionsDefaultsToTheSaveVersion(string saveFile, string version)
    {
        var result = Value(Dispatch(Loaded(saveFile), "encounters.versions", "[]"))!;

        var defaultId = result["default"]!.GetValue<int>();
        result["versions"]!.AsArray().Single(v => v!["id"]!.GetValue<int>() == defaultId)!["name"]!.GetValue<string>()
            .Should().Be(version);
    }

    [Fact]
    public void VersionsReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "encounters.versions", "[]")).Should().Be("no-save");

    [Theory]
    [SupportedSaveFiles]
    public void SearchReturnsTheEncountersOfTheSpecies(string saveFile)
    {
        var session = Loaded(saveFile);
        var game = session.Game!;
        var version = game.GameVersionApproximation;

        var rows = Value(Dispatch(session, "encounters.search", Args(version.Id, (int)Species.Abra)))!.AsArray();

        var expected = game.PokemonRepository.FindEncounter(version.Version, Species.Abra).ToList();
        rows.Should().NotBeEmpty().And.HaveCount(expected.Count);
        rows.Select(r => r!["index"]!.GetValue<int>()).Should().Equal(Enumerable.Range(0, expected.Count));
        rows.Select(r => r!["species"]!.GetValue<string>()).Should().AllBe("Abra");
        rows.Select(r => r!["speciesId"]!.GetValue<int>()).Should().AllBeEquivalentTo((int)Species.Abra);
        rows.Select(r => r!["name"]!.GetValue<string>()).Should().Equal(expected.Select(e => e.Data.LongName));
        rows.Select(r => r!["levelRange"]!.GetValue<string>()).Should().Equal(expected.Select(e => e.LevelRange.Format));
        rows.Select(r => r!["location"]!.GetValue<string>()).Should().Equal(expected.Select(e => e.Location.Name));
        rows.Select(r => r!["version"]!.GetValue<string>()).Should().Equal(expected.Select(e => e.Version.Name));
        rows.Select(r => r!["isEgg"]!.GetValue<bool>()).Should().Equal(expected.Select(e => e.Data.IsEgg));
    }

    [Theory]
    [InlineData(-1, (int)Species.Abra)]
    [InlineData((int)GameVersion.SW, (int)Species.Abra)]
    [InlineData((int)GameVersion.E, 0)]
    [InlineData((int)GameVersion.E, (int)Species.Turtwig)]
    public void SearchFailsWithBadArgumentsForAVersionOrSpeciesOutsideTheGame(int version, int species) =>
        Error(Dispatch(Loaded(SaveFilePath.Emerald), "encounters.search", Args(version, species))).Should().Be("bad-arguments");

    [Fact]
    public void SearchFailsWithNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "encounters.search", Args((int)GameVersion.E, (int)Species.Abra))).Should().Be("no-save");

    [Theory]
    [SupportedSaveFiles]
    public void AddEncounterAddsTheRowsSpeciesToTheFirstEmptyBoxSlot(string saveFile)
    {
        var session = Loaded(saveFile);
        var game = session.Game!;
        var rows = Search(session, Species.Abra);
        var row = rows[^1]!;
        var index = game.SaveFile.NextOpenBoxSlot();
        var slots = game.SaveFile.BoxSlotCount;
        var expected = PokemonHandle.InBox(index / slots, index % slots);
        var changes = new List<string[]>();
        var published = new List<IEngineEvent>();
        session.Changed += changes.Add;
        session.Published += published.Add;

        var added = Value(Dispatch(session, "box.addEncounter", Args(row["index"]!.GetValue<int>())))!;

        added["at"]!.ToJsonString().Should().Be(Args(expected)[1..^1]);
        var pokemon = Value(Dispatch(session, "pokemon.get", Args(expected)))!;
        pokemon["species"]!.GetValue<string>().Should().Be(row["species"]!.GetValue<string>());
        added["id"]!.GetValue<string>().Should().Be(pokemon["id"]!.GetValue<string>());
        changes.Should().ContainSingle().Which.Should().Equal(Topics.Box);
        published.Should().Equal(new PokemonAdded(expected, PokemonAddSource.Encounter, game.Trainer.PokemonBox.All[index].ToOverview()));
    }

    [Fact]
    public void AddEncounterUsesTheLatestSearch()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Search(session, Species.Abra);
        Search(session, Species.Zubat);

        var added = Value(Dispatch(session, "box.addEncounter", Args(0)))!;

        var pokemon = Value(Dispatch(session, "pokemon.get", $"[{added["at"]!.ToJsonString()}]"))!;
        pokemon["species"]!.GetValue<string>().Should().Be("Zubat");
    }

    [Fact]
    public void AddEncounterFailsWithNotFoundWithoutASearch() =>
        AddEncounterFailsWith(Loaded(SaveFilePath.Emerald), 0, "not-found");

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void AddEncounterFailsWithNotFoundForAnIndexOutsideTheResult(int index)
    {
        var session = Loaded(SaveFilePath.Emerald);
        var rows = Search(session, Species.Abra);

        AddEncounterFailsWith(session, index == int.MaxValue ? rows.Count : index, "not-found");
    }

    [Fact]
    public void AddEncounterFailsWithNotFoundAfterASaveLoad()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Search(session, Species.Abra);

        Value(Dispatch(session, "game.load", Args(Convert.ToBase64String(File.ReadAllBytes(SaveFilePath.HgSs)), "other.sav")));

        AddEncounterFailsWith(session, 0, "not-found");
    }

    [Fact]
    public void AddEncounterFailsWithBoxFullWhenNoBoxSlotIsEmpty()
    {
        var session = Loaded(SaveFilePath.Emerald);
        var save = session.Game!.SaveFile;
        var filler = session.Game.Trainer.Party.Pokemons[0].Pkm;
        for (var index = save.NextOpenBoxSlot(); index >= 0; index = save.NextOpenBoxSlot())
            save.SetBoxSlotAtIndex(filler.Clone(), index);
        Search(session, Species.Abra);

        AddEncounterFailsWith(session, 0, "box-full");
    }

    [Fact]
    public void AddEncounterFailsWithNoSaveWithoutALoadedSave() =>
        AddEncounterFailsWith(new Session(), 0, "no-save");

    private static JsonArray Search(Session session, Species species) =>
        Value(Dispatch(session, "encounters.search", Args(session.Game!.GameVersionApproximation.Id, (int)species)))!.AsArray();

    private static void AddEncounterFailsWith(Session session, int index, string code)
    {
        var published = new List<IEngineEvent>();
        session.Published += published.Add;

        Error(Dispatch(session, "box.addEncounter", Args(index))).Should().Be(code);

        published.Should().BeEmpty();
    }
}
