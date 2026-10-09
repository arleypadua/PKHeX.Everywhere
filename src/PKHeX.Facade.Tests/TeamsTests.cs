using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade.Tests;

public class TeamsTests
{
    public static TheoryData<GameVersion> Stadiums => [GameVersion.Stadium, GameVersion.StadiumJ, GameVersion.Stadium2];

    [Fact]
    public void StadiumListsSeventyTeamsTenPerCup() =>
        Blank(GameVersion.Stadium).Teams!.All.Should().SatisfyRespectively(CupsOf(10,
            "Anything Goes", "Poké Cup", "Petit Cup", "Pika Cup", "Prime Cup", "Gym Leader Castle", "Vs. Mewtwo"));

    [Fact]
    public void Stadium2ListsSixtyTeamsTenPerCup() =>
        Blank(GameVersion.Stadium2).Teams!.All.Should().SatisfyRespectively(CupsOf(10,
            "Anything Goes", "Little Cup", "Poké Cup", "Prime Cup", "Gym Leader Castle", "Vs. Rival"));

    [Fact]
    public void PocketMonstersStadiumListsSixteenTeamsWithoutCups()
    {
        var teams = Blank(GameVersion.StadiumJ).Teams!.All;

        teams.Select(team => team.Number).Should().Equal(Enumerable.Range(0, 16));
        teams.Should().AllSatisfy(team => team.Cup.Should().BeNull());
    }

    [Theory]
    [MemberData(nameof(Stadiums))]
    public void ABlankTeamIsEmptyAndUnnamed(GameVersion version) =>
        Blank(version).Teams!.All.Should().AllSatisfy(team =>
        {
            team.Name.Should().BeNull();
            team.Slots.Should().Be(6);
            team.Members.Should().BeEmpty();
        });

    [Theory]
    [MemberData(nameof(Stadiums))]
    public void ABlankStadiumSaveLoadsAgain(GameVersion version) =>
        Blank(version).SaveAndReload(reloaded => reloaded.SaveFile.Version.Should().Be(version));

    [Theory]
    [MemberData(nameof(Stadiums))]
    public void APlacedPokemonIsInTheTeamAfterReloading(GameVersion version)
    {
        var game = Blank(version);

        game.Teams!.Place(3, 0, Make(game, Species.Pikachu, 42)).Should().Be(0);

        game.SaveAndReload(reloaded =>
        {
            var member = reloaded.Teams!.Get(3).Members.Should().ContainSingle().Subject;
            ((Species)member.Pkm.Species).Should().Be(Species.Pikachu);
            member.Level.Should().Be(42);
        });
    }

    [Theory]
    [MemberData(nameof(Stadiums))]
    public void PlacingPastTheMembersFillsTheFirstEmptySlot(GameVersion version)
    {
        var game = Blank(version);
        game.Teams!.Place(0, 0, Make(game, Species.Pikachu, 5));

        game.Teams.Place(0, 4, Make(game, Species.Mew, 5)).Should().Be(1);

        SpeciesOf(game, 0).Should().Equal(Species.Pikachu, Species.Mew);
    }

    [Theory]
    [MemberData(nameof(Stadiums))]
    public void PlacingOnAMemberReplacesIt(GameVersion version)
    {
        var game = Blank(version);
        game.Teams!.Place(0, 0, Make(game, Species.Pikachu, 5));
        game.Teams.Place(0, 1, Make(game, Species.Mew, 5));

        game.Teams.Place(0, 0, Make(game, Species.Onix, 5)).Should().Be(0);

        SpeciesOf(game, 0).Should().Equal(Species.Onix, Species.Mew);
    }

    [Theory]
    [MemberData(nameof(Stadiums))]
    public void ClearingAMiddleSlotMovesTheRestUp(GameVersion version)
    {
        var game = Blank(version);
        foreach (var species in new[] { Species.Pikachu, Species.Mew, Species.Onix })
            game.Teams!.Place(1, 5, Make(game, species, 5));

        game.Teams!.Clear(1, 1);

        game.SaveAndReload(reloaded => SpeciesOf(reloaded, 1).Should().Equal(Species.Pikachu, Species.Onix));
    }

    [Theory]
    [MemberData(nameof(Stadiums))]
    public void ClearingAnEmptySlotChangesNothing(GameVersion version)
    {
        var game = Blank(version);
        game.Teams!.Place(1, 0, Make(game, Species.Pikachu, 5));

        game.Teams.Clear(1, 3);

        SpeciesOf(game, 1).Should().Equal(Species.Pikachu);
    }

    [Theory]
    [MemberData(nameof(Stadiums))]
    public void AReplacedMemberIsInTheTeamAfterReloading(GameVersion version)
    {
        var game = Blank(version);
        game.Teams!.Place(2, 0, Make(game, Species.Pikachu, 5));
        game.Teams.Place(2, 1, Make(game, Species.Mew, 5));

        var edited = game.Teams.Get(2).Members[1].Clone();
        edited.ChangeLevel(50);
        game.Teams.Replace(2, 1, edited);

        game.SaveAndReload(reloaded => reloaded.Teams!.Get(2).Members.Select(p => p.Level).Should().Equal(5, 50));
    }

    [Theory]
    [MemberData(nameof(Stadiums))]
    public void ReplacingAnEmptySlotFails(GameVersion version)
    {
        var game = Blank(version);

        game.Invoking(g => g.Teams!.Replace(0, 0, Make(g, Species.Mew, 5))).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MemberData(nameof(Stadiums))]
    public void AnUnknownTeamOrSlotIsOutOfRange(GameVersion version)
    {
        var game = Blank(version);
        var teams = game.Teams!;
        var mew = Make(game, Species.Mew, 5);

        teams.Invoking(t => t.Get(teams.Count)).Should().Throw<ArgumentOutOfRangeException>();
        teams.Invoking(t => t.Get(-1)).Should().Throw<ArgumentOutOfRangeException>();
        teams.Invoking(t => t.Place(0, 6, mew)).Should().Throw<ArgumentOutOfRangeException>();
        teams.Invoking(t => t.Place(0, -1, mew)).Should().Throw<ArgumentOutOfRangeException>();
        teams.Invoking(t => t.Clear(teams.Count, 0)).Should().Throw<ArgumentOutOfRangeException>();
    }

    // The games check each team's footer the way they check a box's: a magic number, then the sum of the bytes before the checksum.
    [Fact]
    public void AWrittenStadiumTeamEndsWithItsFooterAndChecksum()
    {
        var game = Blank(GameVersion.Stadium);
        game.Teams!.Place(10, 0, Make(game, Species.Pikachu, 5));

        var team = game.SaveFile.Data.Slice(((SAV1Stadium)game.SaveFile).GetTeamOffset(30), 0x160);
        team[0x0F].Should().Be(1);
        ReadUInt32LittleEndian(team[^6..]).Should().Be(0x454B4F50);
        ReadUInt16BigEndian(team[^2..]).Should().Be(Checksums.CheckSum16(team[..^2]));
    }

    [Fact]
    public void AWrittenStadium2TeamEndsWithItsFooterAndChecksum()
    {
        var game = Blank(GameVersion.Stadium2);
        game.Teams!.Place(10, 0, Make(game, Species.Pikachu, 5));

        var team = game.SaveFile.Data.Slice(SAV2Stadium.GetTeamOffset(10), 0x180);
        team[0x01].Should().Be(1);
        ReadUInt32LittleEndian(team[^6..]).Should().Be(0x30763350);
        ReadUInt16BigEndian(team[^2..]).Should().Be(Checksums.CheckSum16(team[..^2]));
    }

    [Fact]
    public void ASaveWithoutTeamsHasNone() =>
        SaveFilePath.Load(SaveFilePath.Emerald).Teams.Should().BeNull();

    private static Game Blank(GameVersion version) =>
        Game.EmptyOf(GameVersionRepository.Instance.FindBlank((int)version)!);

    private static Pokemon Make(Game game, Species species, int level)
    {
        var pkm = game.SaveFile.BlankPKM;
        pkm.Species = (ushort)species;
        pkm.CurrentLevel = (byte)level;
        pkm.ClearNickname();
        return new Pokemon(pkm, game);
    }

    private static IEnumerable<Species> SpeciesOf(Game game, int team) =>
        game.Teams!.Get(team).Members.Select(p => (Species)p.Pkm.Species);

    private static Action<Team>[] CupsOf(int perCup, params string[] cups) => cups
        .SelectMany(cup => Enumerable.Repeat(cup, perCup))
        .Select((cup, number) => (Action<Team>)(team =>
        {
            team.Number.Should().Be(number);
            team.Cup.Should().Be(cup);
        }))
        .ToArray();
}
