using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class EventsHandlerTests
{
    private const int OldSeaMap = 376;

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Unbound, SaveFilePath.Unbound21, SaveFilePath.RadicalRed, SaveFilePath.Imperium])] // the hacks don't support Events
    public void GetReturnsTheEventsOfTheSave(string saveFile)
    {
        var session = Loaded(saveFile);

        var events = Value(Dispatch(session, "events.get", "[]"))!;

        var expected = session.Game!.Events!;
        var gen3 = expected.Gen3;
        events.ToJsonString().Should().Be(JsonSerializer.Serialize(new
        {
            flags = expected.Flags.Select(f => new { index = f.Index, name = f.Name, category = f.Category, value = f.Value }),
            work = expected.Work.Select(w => new
            {
                index = w.Index,
                name = w.Name,
                category = w.Category,
                value = w.Value,
                options = w.Options.Select(o => new { name = o.Name, value = o.Value }),
            }),
            flagCount = expected.FlagCount,
            workCount = expected.WorkCount,
            workMin = expected.WorkMin,
            workMax = expected.WorkMax,
            gen3 = gen3 is null
                ? null
                : new
                {
                    tickets = gen3.Tickets.All.Select(t => t.Name),
                    anyTicketMissing = !gen3.Tickets.Missing.IsEmpty,
                    oldSeaMapNeedsConfirmation = gen3.Tickets.OldSeaMapNeedsConfirmation,
                    islands = gen3.Islands.Select(f => new { index = f.Index, name = f.Name, category = f.Category, value = f.Value }),
                },
        }));
        events["flags"]!.AsArray().Should().NotBeEmpty();
        Indices(events["flags"]!).Should().OnlyHaveUniqueItems();
        Indices(events["work"]!).Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(GameVersion.RD)]
    [InlineData(GameVersion.YW)]
    public void GetReturnsEveryGen1FlagWithoutLabels(GameVersion version)
    {
        var session = Gen1(version);
        var save = (SAV1)session.Game!.SaveFile;

        var events = Value(Dispatch(session, "events.get", "[]"))!;

        events["flags"]!.AsArray().Should().BeEmpty();
        events["work"]!.AsArray().Should().BeEmpty();
        events["flagCount"]!.GetValue<int>().Should().Be(save.EventFlagCount);
        events["workCount"]!.GetValue<int>().Should().Be(save.EventWorkCount);
        Value(Dispatch(session, "game.get", "[]"))!["hasEvents"]!.GetValue<bool>().Should().BeTrue();
    }

    [Theory]
    [InlineData(GameVersion.RD)]
    [InlineData(GameVersion.YW)]
    public void SetFlagChangesAGen1Flag(GameVersion version)
    {
        var session = Gen1(version);
        var index = session.Game!.Events!.FlagCount - 1;
        var expected = !Flag(session, index);

        Dispatch(session, "events.setFlag", Args(index, expected));

        Flag(session, index).Should().Be(expected);
    }

    [Fact]
    public void GetReturnsNullWhenTheSaveHasNoEvents() =>
        Value(Dispatch(NoEvents(), "events.get", "[]")).Should().BeNull();

    [Fact]
    public void GetReturnsNoGen3OutsideGen3() =>
        Value(Dispatch(Loaded(SaveFilePath.HgSs), "events.get", "[]"))!["gen3"].Should().BeNull();

    [Fact]
    public void GetReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "events.get", "[]")).Should().Be("no-save");

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Unbound, SaveFilePath.Unbound21, SaveFilePath.RadicalRed, SaveFilePath.Imperium])] // the hacks don't support Events
    public void SetFlagChangesTheFlag(string saveFile)
    {
        var session = Loaded(saveFile);
        var flag = session.Game!.Events!.Flags.Last();
        var expected = !flag.Value;

        Value(Dispatch(session, "events.setFlag", Args(flag.Index, expected))).Should().BeNull();

        Flag(session, flag.Index).Should().Be(expected);
        session.Game.SaveAndReload(reloaded => reloaded.Events!.GetFlag(flag.Index).Should().Be(expected));
    }

    [Fact]
    public void SetFlagChangesTheEventsTopic()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Dispatch(session, "events.setFlag", Args(0, true));

        changes.Should().BeEquivalentTo([new[] { Topics.Events }]);
    }

    [Fact]
    public void SetFlagOnAnEmeraldIslandShowsInGen3()
    {
        var session = Loaded(SaveFilePath.Emerald);
        var faraway = session.Game!.Events!.Gen3!.Islands.Single(i => i.Name == "Reachable: Faraway Island");
        var expected = !faraway.Value;

        Dispatch(session, "events.setFlag", Args(faraway.Index, expected));

        Value(Dispatch(session, "events.get", "[]"))!["gen3"]!["islands"]!.AsArray()
            .Single(i => i!["name"]!.GetValue<string>() == faraway.Name)!["value"]!.GetValue<bool>()
            .Should().Be(expected);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void SetFlagFailsWithOutOfRangeForAnIndexOutsideTheFlags(int index) =>
        Error(Dispatch(Loaded(SaveFilePath.HgSs), "events.setFlag", Args(index, true))).Should().Be("out-of-range");

    [Fact]
    public void SetFlagFailsWithNotFoundWhenTheSaveHasNoEvents() =>
        Error(Dispatch(NoEvents(), "events.setFlag", Args(0, true))).Should().Be("not-found");

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Unbound, SaveFilePath.Unbound21, SaveFilePath.RadicalRed, SaveFilePath.Imperium])] // the hacks don't support Events
    public void FlagReturnsTheValueAtAnIndex(string saveFile)
    {
        var session = Loaded(saveFile);
        var flag = session.Game!.Events!.Flags.First();

        Flag(session, flag.Index).Should().Be(flag.Value);
    }

    [Fact]
    public void FlagReturnsAnUnlabelledFlag()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var events = session.Game!.Events!;
        var unlabelled = Enumerable.Range(0, events.FlagCount).First(i => events.Flags.All(f => f.Index != i));
        Dispatch(session, "events.setFlag", Args(unlabelled, true));

        Flag(session, unlabelled).Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void FlagFailsWithOutOfRangeForAnIndexOutsideTheFlags(int index) =>
        Error(Dispatch(Loaded(SaveFilePath.HgSs), "events.flag", Args(index))).Should().Be("out-of-range");

    [Fact]
    public void FlagFailsWithNotFoundWhenTheSaveHasNoEvents() =>
        Error(Dispatch(NoEvents(), "events.flag", Args(0))).Should().Be("not-found");

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Unbound, SaveFilePath.Unbound21, SaveFilePath.RadicalRed, SaveFilePath.Imperium])] // the hacks don't support Events
    public void SetWorkChangesTheWork(string saveFile)
    {
        var session = Loaded(saveFile);
        var work = session.Game!.Events!.Work.Last();
        var expected = work.Value == 7 ? 8 : 7;

        Value(Dispatch(session, "events.setWork", Args(work.Index, expected))).Should().BeNull();

        WorkValue(session, work.Index).Should().Be(expected);
        session.Game.SaveAndReload(reloaded => reloaded.Events!.GetWork(work.Index).Should().Be(expected));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void SetWorkFailsWithOutOfRangeForAnIndexOutsideTheWork(int index) =>
        Error(Dispatch(Loaded(SaveFilePath.HgSs), "events.setWork", Args(index, 1))).Should().Be("out-of-range");

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void SetWorkFailsWithOutOfRangeForAValueOutsideTheBounds(int offset)
    {
        var session = Loaded(SaveFilePath.Crystal);
        var events = session.Game!.Events!;
        var value = offset < 0 ? events.WorkMin + offset : events.WorkMax + offset;

        Error(Dispatch(session, "events.setWork", Args(events.Work[0].Index, value))).Should().Be("out-of-range");
    }

    [Fact]
    public void SetWorkFailsWithNotFoundWhenTheSaveHasNoEvents() =>
        Error(Dispatch(NoEvents(), "events.setWork", Args(0, 1))).Should().Be("not-found");

    [Fact]
    public void GiveTicketsReturnsTheAddedNames()
    {
        var session = Loaded(SaveFilePath.Emerald);
        var expected = session.Game!.Events!.Gen3!.Tickets.Missing.Select(t => t.Name).ToArray();

        var added = Value(Dispatch(session, "events.giveTickets", Args(true)))!;

        added.AsArray().Select(n => n!.GetValue<string>()).Should().Equal(expected);
        session.Game.Events.Gen3.Tickets.Missing.Should().BeEmpty();
    }

    [Fact]
    public void GiveTicketsPublishesItemChangedPerTicket()
    {
        var session = Loaded(SaveFilePath.Emerald);
        var missing = session.Game!.Events!.Gen3!.Tickets.Missing;
        var published = new List<IEngineEvent>();
        session.Published += published.Add;

        Dispatch(session, "events.giveTickets", Args(true));

        published.Should().Equal(missing.Select(t => (IEngineEvent)new ItemChanged(t.Id, 1)));
    }

    [Fact]
    public void GiveTicketsChangesTheInventoryTopic()
    {
        var session = Loaded(SaveFilePath.Emerald);
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Dispatch(session, "events.giveTickets", Args(true));

        changes.Should().BeEquivalentTo([new[] { Topics.Inventory }]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GiveTicketsHonorsIncludeOldSeaMap(bool includeOldSeaMap)
    {
        var session = Loaded(SaveFilePath.Emerald);

        Dispatch(session, "events.giveTickets", Args(includeOldSeaMap));

        session.Game!.Trainer.Inventories["KeyItems"].Items.Any(i => i.Id == OldSeaMap).Should().Be(includeOldSeaMap);
    }

    [Fact]
    public void GiveTicketsFailsWithPouchFullWhenKeyItemsIsFull()
    {
        var session = Loaded(SaveFilePath.Emerald);
        var keyItems = session.Game!.Trainer.Inventories["KeyItems"];
        var tickets = session.Game.Events!.Gen3!.Tickets.All.Select(t => t.Id).ToHashSet();
        foreach (var item in keyItems.CurrentSupportedItems.Where(i => !tickets.Contains(i.Id)).ToList())
            keyItems.TrySet(item.Id, 1);
        var published = new List<IEngineEvent>();
        session.Published += published.Add;

        Error(Dispatch(session, "events.giveTickets", Args(true))).Should().Be("pouch-full");

        published.Should().BeEmpty();
    }

    [Fact]
    public void GiveTicketsFailsWithNotFoundOutsideGen3() =>
        Error(Dispatch(Loaded(SaveFilePath.HgSs), "events.giveTickets", Args(true))).Should().Be("not-found");

    private static Session Gen1(GameVersion version)
    {
        if (version == GameVersion.YW)
            return Loaded(SaveFilePath.Yellow);

        var session = new Session();
        session.Load(Game.EmptyOf(GameVersionRepository.Instance.Get(version)), $"{version}.sav");
        return session;
    }

    private static Session NoEvents()
    {
        var session = new Session();
        session.Load(Game.EmptyOf(GameVersionRepository.Instance.Get(GameVersion.PLA)), "legends.bin");
        return session;
    }

    private static int[] Indices(JsonNode entries) => entries.AsArray().Select(e => e!["index"]!.GetValue<int>()).ToArray();

    private static bool Flag(Session session, int index) =>
        Value(Dispatch(session, "events.flag", Args(index)))!.GetValue<bool>();

    private static int WorkValue(Session session, int index) => Value(Dispatch(session, "events.get", "[]"))!["work"]!
        .AsArray()
        .Single(w => w!["index"]!.GetValue<int>() == index)!["value"]!.GetValue<int>();
}
