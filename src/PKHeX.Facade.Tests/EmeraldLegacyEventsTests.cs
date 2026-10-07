using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class EmeraldLegacyEventsTests
{
    private static Game Load() => SaveFilePath.Load(SaveFilePath.EmeraldLegacy);

    [Fact]
    public void SupportsEvents() => Load().Events.Should().NotBeNull();

    [Fact]
    public void NamesFlagsFromTheHacksOwnConstants()
    {
        var events = Load().Events!;
        // Emerald's first badge flag is 0x867; Legacy's larger trainer flag range moves it to 0x8C7.
        var badge = events.Flags.Single(f => f.Index == 0x8C7);
        badge.Name.Should().Be("Badge01 Get");
        badge.Value.Should().BeTrue(); // the fixture has all eight badges
    }

    [Fact]
    public void DoesNotUseEmeraldsIndexForAShiftedFlag()
    {
        var events = Load().Events!;
        // 0x867 is Emerald's Badge01. In Legacy that index is something else entirely, so nothing
        // should be labelled as a badge there.
        events.Flags.Where(f => f.Index == 0x867).Should().NotContain(f => f.Name.Contains("Badge"));
    }

    [Fact]
    public void NamesTheIslandFlagsAtTheirShiftedIndices()
    {
        var events = Load().Events!;
        // PKHeX hardcodes Emerald's 0x8B3 for Southern Island; Legacy's is 0x913.
        events.Flags.Should().Contain(f => f.Index == 0x913 && f.Name.Contains("Southern Island"));
    }

    [Fact]
    public void NamesEveryWorkValue()
    {
        var events = Load().Events!;
        events.Work.Should().HaveCount(257);
        events.Work.Should().Contain(w => w.Index == 0 && w.Name == "Temp 0");
        events.Work.Should().Contain(w => w.Index == 0x100 && w.Name == "Norman Rematch Call Step Counter");
    }

    [Theory]
    [InlineData(0x23, 2)] // Starter Mon: Mudkip, and the party has a Swampert
    [InlineData(0x85, 7)] // Petalburg Gym: Norman defeated
    public void ReadsWorkAfterLegacysLongerFlagArray(int index, int expected) =>
        Load().Events!.GetWork(index).Should().Be(expected);

    [Fact]
    public void WritingWorkLeavesEveryFlagAlone()
    {
        var events = Load().Events!;
        var flags = Enumerable.Range(0, events.FlagCount).Select(events.GetFlag).ToArray();

        events.SetWork(0, ushort.MaxValue);

        Enumerable.Range(0, events.FlagCount).Select(events.GetFlag).Should().Equal(flags);
        events.GetWork(0).Should().Be(ushort.MaxValue);
    }

    [Fact]
    public void ReadsAndWritesAFlagThroughTheShiftedOffsets()
    {
        var game = Load();
        var events = game.Events!;
        var flag = events.Flags.Single(f => f.Index == 0x8C7);

        flag.Value = false;
        events.GetFlag(0x8C7).Should().BeFalse();
        flag.Value = true;
        events.GetFlag(0x8C7).Should().BeTrue();
    }

    // The island flags PKHeX hardcodes are Emerald's, and five of them moved. Until a Save format can
    // supply them, Legacy gets none rather than wrong ones.
    [Fact]
    public void DoesNotOfferEmeraldsHardcodedIslandList() =>
        Load().Events!.Gen3!.Islands.Should().BeEmpty();
}
