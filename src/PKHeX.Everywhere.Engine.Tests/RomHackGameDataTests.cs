using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class RomHackGameDataTests
{
    private static JsonNode Options(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.options", Args(at)))!;

    private static int[] Ids(Session session, string call) =>
        Value(Dispatch(session, call, "[]"))!.AsArray().Select(choice => choice!["id"]!.GetValue<int>()).ToArray();

    [Theory]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    public void NatureAbilityAndGenderAreLocked(string saveFile)
    {
        var session = Loaded(saveFile);

        foreach (var at in new[] { PokemonHandle.Party(0), PokemonHandle.InBox(0, 1) })
            Options(session, at)["locked"]!.AsArray().Select(field => field!.GetValue<string>())
                .Should().Contain(["nature", "ability", "gender"]);
    }

    [Theory]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    public void LanguagesAreTheGen3Ones(string saveFile) =>
        Ids(Loaded(saveFile), "game.languages").Should()
            .Contain((int)LanguageID.English)
            .And.NotContain([(int)LanguageID.Korean, (int)LanguageID.ChineseS, (int)LanguageID.ChineseT]);

    [Theory]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    public void OriginGamesAreTheGen3Ones(string saveFile) =>
        Ids(Loaded(saveFile), "game.originGames").Should()
            .Contain((int)GameVersion.FR)
            .And.NotContain([(int)GameVersion.D, (int)GameVersion.P, (int)GameVersion.Pt, (int)GameVersion.HG, (int)GameVersion.SS]);

    [Theory]
    [InlineData(SaveFilePath.RadicalRed, 0, 1, "Route 1", false)]
    [InlineData(SaveFilePath.RadicalRed, 0, 8, "Viridian Forest", false)]
    [InlineData(SaveFilePath.Unbound, null, 0, "Location #179", true)]
    [InlineData(SaveFilePath.Unbound, 0, 0, "Location #179", true)]
    public void MetLocationsAreNamedForTheHack(string saveFile, int? box, int slot, string name, bool locked)
    {
        var session = Loaded(saveFile);
        var at = box is { } inBox ? PokemonHandle.InBox(inBox, slot) : PokemonHandle.Party(slot);
        var metLocation = Value(Dispatch(session, "pokemon.details", Args(at)))!["metLocation"]!.GetValue<int>();
        var options = Options(session, at);

        options["metLocations"]!.AsArray().Single(choice => choice!["id"]!.GetValue<int>() == metLocation)!["name"]!.GetValue<string>()
            .Should().Be(name);
        options["locked"]!.AsArray().Select(field => field!.GetValue<string>()).Contains("metLocation").Should().Be(locked);
    }

    [Theory]
    [InlineData(SaveFilePath.Unbound, true)]
    [InlineData(SaveFilePath.RadicalRed, true)]
    [InlineData(SaveFilePath.Emerald, false)]
    public void StatsAreApproximateForTheHacks(string saveFile, bool approximate) =>
        Value(Dispatch(Loaded(saveFile), "game.get", "[]"))!["statsApproximate"]!.GetValue<bool>().Should().Be(approximate);
}
