using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade.Tests.Base;
using Pokemon = PKHeX.Facade.Pokemons.Pokemon;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class TeamHandlerTests
{
    private static readonly PokemonHandle FirstBoxSlot = PokemonHandle.InBox(0, 0);

    [Theory]
    [InlineData(GameVersion.Stadium, 70, new[] { "Anything Goes", "Poké Cup", "Petit Cup", "Pika Cup", "Prime Cup", "Gym Leader Castle", "Vs. Mewtwo" })]
    [InlineData(GameVersion.StadiumJ, 16, new string?[] { null })]
    [InlineData(GameVersion.Stadium2, 60, new[] { "Anything Goes", "Little Cup", "Poké Cup", "Prime Cup", "Gym Leader Castle", "Vs. Rival" })]
    public void ListReturnsEveryTeamWithItsCup(GameVersion version, int count, string?[] cups)
    {
        var teams = Value(Dispatch(Blank(version), "team.list", "[]"))!.AsArray();

        teams.Select(t => t!["team"]!.GetValue<int>()).Should().Equal(Enumerable.Range(0, count));
        teams.Select(t => t!["cup"]?.GetValue<string>()).Distinct().Should().Equal(cups);
        teams.Should().AllSatisfy(t =>
        {
            t!["name"].Should().BeNull();
            t["slots"]!.GetValue<int>().Should().Be(6);
            t["filled"]!.GetValue<int>().Should().Be(0);
        });
    }

    [Theory]
    [InlineData(GameVersion.Stadium)]
    [InlineData(GameVersion.Stadium2)]
    public void APlacedBoxPokemonIsInTheTeamAfterExportAndLoad(GameVersion version)
    {
        var session = WithBoxed(version, Species.Pikachu);

        var placed = Value(Dispatch(session, "team.place", Args(FirstBoxSlot, new TeamSlot(1, 3))))!;

        placed["at"]!.ToJsonString().Should().Be("""{"source":"team","slot":0,"box":null,"team":1}""");
        var reloaded = ExportedAndLoaded(session);
        Members(reloaded, 1).Should().Equal("Pikachu");
        Value(Dispatch(reloaded, "box.get", "[]"))!.AsArray().Select(p => p!["species"]!.GetValue<string>()).Should().Equal("Pikachu");
        Value(Dispatch(reloaded, "team.list", "[]"))![1]!["filled"]!.GetValue<int>().Should().Be(1);
    }

    [Theory]
    [InlineData(GameVersion.Stadium)]
    [InlineData(GameVersion.Stadium2)]
    public void ClearingAMiddleSlotMovesTheRestUpAndSurvivesExportAndLoad(GameVersion version)
    {
        var session = WithBoxed(version, Species.Pikachu, Species.Mew, Species.Onix);
        for (var slot = 0; slot < 3; slot++)
            Value(Dispatch(session, "team.place", Args(PokemonHandle.InBox(0, slot), new TeamSlot(2, slot))));

        Value(Dispatch(session, "team.clear", Args(new TeamSlot(2, 1))));

        Members(session, 2).Should().Equal("Pikachu", "Onix");
        Members(ExportedAndLoaded(session), 2).Should().Equal("Pikachu", "Onix");
    }

    [Theory]
    [InlineData(GameVersion.Stadium)]
    [InlineData(GameVersion.Stadium2)]
    public void AnEditedTeamMemberSurvivesExportAndLoad(GameVersion version)
    {
        var session = WithBoxed(version, Species.Pikachu, Species.Mew);
        Value(Dispatch(session, "team.place", Args(PokemonHandle.InBox(0, 0), new TeamSlot(0, 0))));
        Value(Dispatch(session, "team.place", Args(PokemonHandle.InBox(0, 1), new TeamSlot(0, 1))));
        var mew = PokemonHandle.InTeam(0, 1);

        Value(Dispatch(session, "pokemon.edit", Args(mew)));
        Value(Dispatch(session, "pokemon.update", Args(PokemonHandle.Draft(), new { level = 50 })));
        Value(Dispatch(session, "pokemon.commit", "[]"));

        Value(Dispatch(ExportedAndLoaded(session), "pokemon.details", Args(mew)))!["level"]!.GetValue<int>().Should().Be(50);
        Value(Dispatch(session, "pokemon.details", Args(PokemonHandle.InBox(0, 1))))!["level"]!.GetValue<int>().Should().Be(5);
    }

    // PKHeX can't write a Pocket Monsters Stadium box slot, so the member is placed through the Facade.
    [Fact]
    public void AnEditedPocketMonstersStadiumTeamMemberSurvivesExportAndLoad()
    {
        var session = Blank(GameVersion.StadiumJ);
        session.Game!.Teams!.Place(0, 0, Make(session.Game, Species.Mew));
        var mew = PokemonHandle.InTeam(0, 0);

        Value(Dispatch(session, "pokemon.edit", Args(mew)));
        Value(Dispatch(session, "pokemon.update", Args(PokemonHandle.Draft(), new { level = 50 })));
        Value(Dispatch(session, "pokemon.commit", "[]"));

        Value(Dispatch(ExportedAndLoaded(session), "pokemon.details", Args(mew)))!["level"]!.GetValue<int>().Should().Be(50);
    }

    [Fact]
    public void UpdatingATeamMemberReportsItsTeam()
    {
        var session = WithBoxed(GameVersion.Stadium2, Species.Pikachu);
        Value(Dispatch(session, "team.place", Args(FirstBoxSlot, new TeamSlot(4, 0))));
        var reported = new List<string>();
        session.Changed += reported.AddRange;

        Value(Dispatch(session, "pokemon.update", Args(PokemonHandle.InTeam(4, 0), new { level = 30 })));

        reported.Should().Contain("team/4");
        Value(Dispatch(session, "pokemon.get", Args(PokemonHandle.InTeam(4, 0))))!["level"]!.GetValue<int>().Should().Be(30);
    }

    [Fact]
    public void PlacingReportsTheTeam()
    {
        var session = WithBoxed(GameVersion.Stadium, Species.Pikachu);
        var reported = new List<string>();
        session.Changed += reported.AddRange;

        Value(Dispatch(session, "team.place", Args(FirstBoxSlot, new TeamSlot(5, 0))));

        reported.Should().Contain("team/5");
    }

    [Fact]
    public void ClearingDropsADraftOfAMemberThatMoves()
    {
        var session = WithBoxed(GameVersion.Stadium, Species.Pikachu, Species.Mew);
        Value(Dispatch(session, "team.place", Args(PokemonHandle.InBox(0, 0), new TeamSlot(0, 0))));
        Value(Dispatch(session, "team.place", Args(PokemonHandle.InBox(0, 1), new TeamSlot(0, 1))));
        Value(Dispatch(session, "pokemon.edit", Args(PokemonHandle.InTeam(0, 1))));

        Value(Dispatch(session, "team.clear", Args(new TeamSlot(0, 0))));

        Error(Dispatch(session, "pokemon.commit", "[]")).Should().Be("no-draft");
    }

    [Fact]
    public void AnUnknownTeamOrSlotIsOutOfRange()
    {
        var session = WithBoxed(GameVersion.Stadium2, Species.Pikachu);

        Error(Dispatch(session, "team.get", Args(60))).Should().Be("out-of-range");
        Error(Dispatch(session, "team.place", Args(FirstBoxSlot, new TeamSlot(60, 0)))).Should().Be("out-of-range");
        Error(Dispatch(session, "team.place", Args(FirstBoxSlot, new TeamSlot(0, 6)))).Should().Be("out-of-range");
        Error(Dispatch(session, "team.clear", Args(new TeamSlot(0, -1)))).Should().Be("out-of-range");
        Error(Dispatch(session, "pokemon.get", Args(PokemonHandle.InTeam(0, 0)))).Should().Be("not-found");
    }

    [Fact]
    public void ASaveWithoutTeamsListsNoneAndCantPlace()
    {
        var session = Loaded(SaveFilePath.Emerald);

        Value(Dispatch(session, "team.list", "[]"))!.AsArray().Should().BeEmpty();
        Error(Dispatch(session, "team.place", Args(PokemonHandle.Party(0), new TeamSlot(0, 0)))).Should().Be("not-supported");
        Error(Dispatch(session, "team.get", Args(0))).Should().Be("not-supported");
        Error(Dispatch(session, "team.clear", Args(new TeamSlot(0, 0)))).Should().Be("not-supported");
        Error(Dispatch(session, "pokemon.get", Args(PokemonHandle.InTeam(0, 0)))).Should().Be("not-supported");
    }

    [Fact]
    public void ListFailsWithNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "team.list", "[]")).Should().Be("no-save");

    private static Session Blank(GameVersion version)
    {
        var session = new Session();
        Value(Dispatch(session, "game.loadBlank", Args((int)version)));
        return session;
    }

    private static Session WithBoxed(GameVersion version, params Species[] species)
    {
        var session = Blank(version);
        var game = session.Game!;
        foreach (var each in species)
            game.Trainer.PokemonBox.AddOnEmptySlot(Make(game, each)).Should().BeTrue();

        return session;
    }

    private static Pokemon Make(PKHeX.Facade.Game game, Species species)
    {
        var pkm = game.SaveFile.BlankPKM;
        pkm.Species = (ushort)species;
        pkm.CurrentLevel = 5;
        pkm.ClearNickname();
        return new Pokemon(pkm, game);
    }

    private static Session ExportedAndLoaded(Session session)
    {
        var bytes = Value(Dispatch(session, "game.export", "[]"))!["bytes"]!.GetValue<string>();
        var reloaded = new Session();
        Value(Dispatch(reloaded, "game.load", Args(bytes, "stadium.sav", null!)));
        return reloaded;
    }

    private static IEnumerable<string> Members(Session session, int team) =>
        Value(Dispatch(session, "team.get", Args(team)))!.AsArray().Select(p => p!["species"]!.GetValue<string>());
}
