using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.PlugIns;
using PKHeX.Facade;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class PlugInHookTests
{
    private const string TestPlugInId = "PKHeX.Everywhere.Engine.Tests.PlugIn";
    private const string EchoItem = "PKHeX.Everywhere.Engine.Tests.PlugIn.EchoItem";
    private const string FailOnItem = "PKHeX.Everywhere.Engine.Tests.PlugIn.FailOnItem";
    private const string WriteOnItem = "PKHeX.Everywhere.Engine.Tests.PlugIn.WriteOnItem";
    private const string RenameOnChange = "PKHeX.Everywhere.Engine.Tests.PlugIn.RenameOnChange";
    private const string RenameOnSave = "PKHeX.Everywhere.Engine.Tests.PlugIn.RenameOnSave";

    private static byte[] TestPlugIn =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{TestPlugInId}.dll"));

    private static (Session Session, PlugInHost Host, List<PlugInRan> Ran) Hosted(string saveFile = SaveFilePath.Emerald)
    {
        var session = Loaded(saveFile);
        var host = new PlugInHost(session);
        var ran = PlugInRuns(session);
        host.Register(TestPlugIn);
        return (session, host, ran);
    }

    private static void SetItem(Session session, ItemHandle at, int count) =>
        Value(Dispatch(session, "inventory.setItem", Args(at, count)));

    [Fact]
    public void ItemChangedRunsEnabledItemHooksWithTheItemIdAndCount()
    {
        var (session, host, ran) = Hosted();
        host.SetToggle(TestPlugInId, FailOnItem, false);
        var (at, _) = AddableItem(session.Game!)!.Value;

        SetItem(session, at, 2);

        ran.Should().ContainSingle().Which.HookId.Should().Be(EchoItem);
        ran[0].Outcome!.Message.Should().Be($"{at.ItemId} x2");
    }

    [Fact]
    public void ItemChangedSkipsToggledOffHooksAndDisabledPlugIns()
    {
        var (session, host, ran) = Hosted();
        var (at, _) = AddableItem(session.Game!)!.Value;

        host.SetToggle(TestPlugInId, EchoItem, false);
        SetItem(session, at, 2);

        ran.Select(r => r.HookId).Should().Equal(FailOnItem);

        ran.Clear();
        host.SetToggle(TestPlugInId, EchoItem, true);
        host.SetEnabled(TestPlugInId, false);
        SetItem(session, at, 3);

        ran.Should().BeEmpty();
    }

    [Fact]
    public void PokemonChangedRunsChangeHooksOnThePokemonAtTheHandle()
    {
        var (session, _, ran) = Hosted();

        session.Raise(new PokemonChanged(PokemonHandle.Party(0)));

        ran.Select(r => r.HookId).Should().Equal(RenameOnChange);
        session.Game!.SaveFile.GetPartySlotAtIndex(0).Nickname.Should().Be("Changed");
    }

    [Fact]
    public async Task PokemonSavedRunsSaveHooksOnThePokemonAtTheHandle()
    {
        var (session, host, ran) = Hosted();
        var (at, index) = FirstBoxPokemon(session.Game!)!.Value;

        await host.Handle(new PokemonSaved(at, session.Game.Trainer.PokemonBox.All[index].ToOverview()));

        ran.Select(r => r.HookId).Should().Equal(RenameOnSave);
        session.Game!.SaveFile.GetBoxSlotAtIndex(at.Box!.Value, at.Slot).Nickname.Should().Be("Saved");
    }

    [Fact]
    public void AThrowingHookIsPublishedAndRecordedWithoutStoppingOtherHooks()
    {
        var (session, host, ran) = Hosted();
        var (at, _) = AddableItem(session.Game!)!.Value;

        SetItem(session, at, 2);

        ran.Select(r => r.HookId).Should().BeEquivalentTo([EchoItem, FailOnItem]);
        ran.Single(r => r.HookId == FailOnItem).Failure.Should().Be(new HookFailure(nameof(InvalidOperationException), $"Failed on item {at.ItemId}"));

        var failure = host.Failures.Should().ContainSingle().Subject;
        failure.PlugInId.Should().Be(TestPlugInId);
        failure.HookId.Should().Be(FailOnItem);
        failure.Message.Should().Be($"Failed on item {at.ItemId}");

        var listed = Value(Dispatch(session, "plugins.failures", "[]"))!.AsArray().Should().ContainSingle().Subject!;
        listed["plugInId"]!.GetValue<string>().Should().Be(TestPlugInId);
        listed["hookId"]!.GetValue<string>().Should().Be(FailOnItem);
        listed["message"]!.GetValue<string>().Should().Be($"Failed on item {at.ItemId}");
        listed["stackTrace"]!.GetValue<string>().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task KeepsTheLastTwentyFailures()
    {
        var (_, host, _) = Hosted();

        for (var itemId = 1; itemId <= 21; itemId++) await host.Handle(new ItemChanged(itemId, 1));

        host.Failures.Should().HaveCount(20);
        host.Failures[0].Message.Should().Be("Failed on item 2");
        host.Failures[^1].Message.Should().Be("Failed on item 21");
    }

    [Fact]
    public async Task DismissingAFailureRemovesItFromTheListAndReportsPlugIns()
    {
        var (session, host, _) = Hosted();
        await host.Handle(new ItemChanged(1, 1));
        await host.Handle(new ItemChanged(2, 1));
        var changed = new List<string>();
        session.Changed += changed.AddRange;

        var first = Value(Dispatch(session, "plugins.failures", "[]"))!.AsArray()[0]!;
        Value(Dispatch(session, "plugins.dismissFailure", Args(first["id"]!.GetValue<int>())));

        var listed = Value(Dispatch(session, "plugins.failures", "[]"))!.AsArray();
        listed.Select(f => f!["message"]!.GetValue<string>()).Should().Equal("Failed on item 2");
        changed.Should().Equal(Topics.PlugIns);
    }

    [Fact]
    public void AHookThatWritesTheSaveDoesntTriggerOtherHooks()
    {
        var (session, host, ran) = Hosted();
        var (at, _) = AddableItem(session.Game!)!.Value;
        host.SetToggle(TestPlugInId, WriteOnItem, true);
        host.Find(TestPlugInId)!.Assembly.GetType(WriteOnItem)!.GetProperty("Write")!
            .SetValue(null, () => SetItem(session, at, 5));

        SetItem(session, at, 2);

        ran.Select(r => r.HookId).Should().BeEquivalentTo([EchoItem, FailOnItem, WriteOnItem]);
        session.Game!.Trainer.Inventories.InventoryItems[at.Pouch].AllExceptNone()
            .Single(i => i.Id == at.ItemId).Count.Should().Be(5);
    }
}
