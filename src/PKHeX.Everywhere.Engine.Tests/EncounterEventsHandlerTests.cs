using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

// Events are listed under the save's ParseSettings, which are global.
[Collection(nameof(SaveFormats))]
public class EncounterEventsHandlerTests
{
    [Fact]
    public void FireRedListsTheWishmakerJirachiFromRuby()
    {
        var rows = Events(Loaded(SaveFilePath.FireRed));

        rows.Should().Contain(row =>
            row!["speciesId"]!.GetValue<int>() == (int)Species.Jirachi
            && row["trainerName"]!.GetValue<string>() == "WISHMKR"
            && row["trainerId"]!.GetValue<int>() == 20043
            && row["language"]!.GetValue<string>() == "ENG"
            && row["version"]!.GetValue<string>() == "Ruby");
    }

    [Fact]
    public void YellowListsEventMew() =>
        Events(Loaded(SaveFilePath.Yellow), Species.Mew).Should().NotBeEmpty()
            .And.OnlyContain(row => row!["species"]!.GetValue<string>() == "Mew");

    [Fact]
    public void ARecipientsEventHasNoTrainer() =>
        Events(Loaded(SaveFilePath.Emerald)).Should().Contain(row =>
            row!["trainerName"] == null && row["trainerId"] == null && row["language"] == null);

    [Theory]
    [InlineData(SaveFilePath.FireRed)]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.Crystal)]
    [InlineData(SaveFilePath.Yellow)]
    public void AddEncounterPutsALegalEventPokemonInTheBox(string saveFile)
    {
        var session = Loaded(saveFile);
        var save = session.Game!.SaveFile;
        var rows = Events(session);
        var row = rows[^1]!;
        var index = save.NextOpenBoxSlot();
        rows.Select(r => r!["index"]!.GetValue<int>()).Should().Equal(Enumerable.Range(0, rows.Count));

        var added = Value(Dispatch(session, "box.addEncounter", Args(row["index"]!.GetValue<int>())))!;

        var pokemon = Value(Dispatch(session, "pokemon.get", $"[{added["at"]!.ToJsonString()}]"))!;
        pokemon["species"]!.GetValue<string>().Should().Be(row["species"]!.GetValue<string>());
        LegalIn(save, save.GetBoxSlotAtIndex(index)).Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData((int)Species.Turtwig)]
    public void EventsFailWithBadArgumentsForASpeciesOutsideTheGame(int species) =>
        Error(Dispatch(Loaded(SaveFilePath.Emerald), "encounters.events", Args(species))).Should().Be("bad-arguments");

    [Fact]
    public void EventsFailWithNotSupportedOnAHack() =>
        Error(Dispatch(Loaded(SaveFilePath.Unbound), "encounters.events", "[null]")).Should().Be("not-supported");

    [Fact]
    public void EventsFailWithNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "encounters.events", "[null]")).Should().Be("no-save");

    private static JsonArray Events(Session session, Species? species = null) =>
        Value(Dispatch(session, "encounters.events", Args(species is { } s ? (int)s : null!)))!.AsArray();

    private static bool LegalIn(SaveFile save, PKM pkm)
    {
        ParseSettings.InitFromSaveFileData(save);
        try
        {
            return new LegalityAnalysis(pkm).Valid;
        }
        finally
        {
            ParseSettings.ClearActiveTrainer();
            ParseSettings.AllowEraCartGB = false;
            ParseSettings.AllowEraCartGBA = true;
            ParseSettings.AllowEraSwitchGBA = false;
        }
    }
}
