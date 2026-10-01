using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Events;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class EventsTests
{
    public static TheoryData<GameVersion> LabelledVersions =>
    [
        GameVersion.C, GameVersion.GD,
        GameVersion.R, GameVersion.E, GameVersion.FR,
        GameVersion.D, GameVersion.Pt, GameVersion.HG,
        GameVersion.B, GameVersion.B2,
        GameVersion.X, GameVersion.OR,
        GameVersion.SN, GameVersion.US,
        GameVersion.GP,
        GameVersion.BD,
    ];

    [Fact]
    public void Events_Emerald_ShouldRoundTripLabelledFlag()
    {
        var game = Game.LoadFrom(SaveFilePath.Emerald);
        var cut = game.Events!.Flags.Single(f => f.Name == "Received HM01 Cut");
        var original = cut.Value;

        cut.Value = !original;

        Reload(game, reloaded =>
            reloaded.Events!.Flags.Single(f => f.Index == cut.Index).Value.Should().Be(!original));
    }

    [Theory]
    [MemberData(nameof(LabelledVersions))]
    public void Events_ShouldRoundTripLabelledFlag(GameVersion version)
    {
        var game = Load(version);
        var flag = game.Events!.Flags.Last();
        var original = flag.Value;

        flag.Value = !original;

        Reload(game, reloaded =>
            reloaded.Events!.Flags.Single(f => f.Index == flag.Index && f.Category == flag.Category)
                .Value.Should().Be(!original));
    }

    [Theory]
    [MemberData(nameof(LabelledVersions))]
    public void Events_ShouldRoundTripLabelledWork(GameVersion version)
    {
        var game = Load(version);
        var work = game.Events!.Work.Last();
        var expected = work.Value == 7 ? 8 : 7;

        work.Value = expected;

        Reload(game, reloaded =>
            reloaded.Events!.Work.Single(w => w.Index == work.Index).Value.Should().Be(expected));
    }

    [Theory]
    [MemberData(nameof(LabelledVersions))]
    public void Events_ShouldRoundTripFlagByIndex(GameVersion version)
    {
        var game = Load(version);
        var index = game.Events!.FlagCount - 1;
        var original = game.Events.GetFlag(index);

        game.Events.SetFlag(index, !original);

        Reload(game, reloaded => reloaded.Events!.GetFlag(index).Should().Be(!original));
    }

    [Theory]
    [InlineData(GameVersion.RD)]
    [InlineData(GameVersion.SW)]
    [InlineData(GameVersion.SL)]
    [InlineData(GameVersion.PLA)]
    public void Events_ShouldBeUnavailableWithoutLabels(GameVersion version)
    {
        Load(version).Events.Should().BeNull();
    }

    [Fact]
    public void Events_ShouldRejectFlagOutOfRange()
    {
        var events = Load(GameVersion.E).Events!;

        var act = () => events.SetFlag(events.FlagCount, true);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Events_ShouldExposePredefinedWorkValues()
    {
        var starter = Load(GameVersion.E).Events!.Work.Single(w => w.Name == "Starter Pokémon");

        starter.Options.Should().Equal(
            new EventWorkOption("Treecko", 0),
            new EventWorkOption("Torchic", 1),
            new EventWorkOption("Mudkip", 2));
    }

    private static Game Load(GameVersion version) => version switch
    {
        GameVersion.E or GameVersion.C or GameVersion.HG or GameVersion.GP => Game.LoadFrom(SaveFilePath.PathFrom(version)),
        _ => Game.EmptyOf(GameVersionRepository.Instance.Get(version), "ASH"),
    };

    // Blank saves can't be written and parsed back, so a fresh Game re-reads the same save data instead.
    private static void Reload(Game game, Action<Game> afterReload)
    {
        if (game.SaveFile.Metadata.FilePath is null)
            afterReload(new Game(game.SaveFile));
        else
            game.SaveAndReload(afterReload);
    }
}
