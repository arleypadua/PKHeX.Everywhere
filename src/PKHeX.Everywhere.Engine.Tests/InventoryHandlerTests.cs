using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class InventoryHandlerTests
{
    [Theory]
    [SupportedSaveFiles]
    public void GetReturnsEveryPouchOfTheSave(string saveFile)
    {
        var session = Loaded(saveFile);

        var pouches = Value(Dispatcher.Dispatch(session, "inventory.get", "[]"))!;

        var expected = session.Game!.Trainer.Inventories.InventoryItems.Values
            .OrderBy(i => i.Type, StringComparer.Ordinal)
            .Select(i => new
            {
                name = i.Type,
                items = i.AllExceptNone().Select(item => new { id = item.Id, name = item.Name, count = item.Count, maxCount = i.MaxCountOf(item.Id) }),
                addable = i.CurrentSupportedItems.Select(item => new { id = item.Id, name = item.Name, maxCount = i.MaxCountOf(item.Id) }),
            });
        pouches.ToJsonString().Should().Be(JsonSerializer.Serialize(expected));
        pouches.AsArray().Should().NotBeEmpty();
    }

    [Theory]
    [SupportedSaveFiles]
    public void GetLeavesOutEmptySlotsAndOwnedItemsFromAddable(string saveFile)
    {
        var pouches = Value(Dispatcher.Dispatch(Loaded(saveFile), "inventory.get", "[]"))!.AsArray();

        foreach (var pouch in pouches)
        {
            var owned = Ids(pouch!["items"]!);
            owned.Should().NotContain(0);
            Ids(pouch["addable"]!).Should().NotIntersectWith(owned);
        }
    }

    [Theory]
    [InlineData(SaveFilePath.Crystal, "TMHMs", 243)]
    [InlineData(SaveFilePath.Emerald, "TMHMs", 339)]
    [InlineData(SaveFilePath.LetsGoPikachu, "Items", 113)]
    [InlineData(SaveFilePath.LetsGoPikachu, "BattleItems", 656)]
    public void GetReturnsTheMaxCountOfEachItem(string saveFile, string pouch, int itemId)
    {
        var pouches = Value(Dispatcher.Dispatch(Loaded(saveFile), "inventory.get", "[]"))!.AsArray();

        var item = Item(pouches, pouch, itemId);
        item["maxCount"]!.GetValue<int>().Should().Be(1);
    }

    [Fact]
    public void GetReturnsNoPouchesWhenPkHeXCannotReadTheInventory()
    {
        var session = new Session();
        session.Load(Game.EmptyOf(GameVersionRepository.Instance.Get(GameVersion.PLA)), "legends.bin");

        Value(Dispatcher.Dispatch(session, "inventory.get", "[]"))!.AsArray().Should().BeEmpty();
    }

    [Fact]
    public void GetReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatcher.Dispatch(new Session(), "inventory.get", "[]")).Should().Be("no-save");

    [Theory]
    [SupportedSaveFiles]
    public void SetItemAddsAnItemTheSaveDoesNotOwn(string saveFile)
    {
        var session = Loaded(saveFile);
        var (at, maxCount) = AddableItem(session.Game!)!.Value;

        Value(Dispatcher.Dispatch(session, "inventory.setItem", Args(at, maxCount))).Should().BeNull();

        Count(session, at).Should().Be(maxCount);
        session.Game!.SaveAndReload(reloaded => reloaded.Trainer.Inventories[at.Pouch].Items
            .Should().ContainSingle(i => i.Id == at.ItemId && i.Count == maxCount));
    }

    [Theory]
    [SupportedSaveFiles]
    public void SetItemReplacesTheCountOfAnOwnedItem(string saveFile)
    {
        var session = Loaded(saveFile);
        var (at, maxCount) = AddableItem(session.Game!)!.Value;
        Dispatcher.Dispatch(session, "inventory.setItem", Args(at, maxCount));

        Value(Dispatcher.Dispatch(session, "inventory.setItem", Args(at, 1))).Should().BeNull();

        Count(session, at).Should().Be(1);
        session.Game!.SaveAndReload(reloaded => reloaded.Trainer.Inventories[at.Pouch].Items
            .Should().ContainSingle(i => i.Id == at.ItemId && i.Count == 1));
    }

    [Theory]
    [SupportedSaveFiles]
    public void SetItemRemovesAnItemWithACountOfZero(string saveFile)
    {
        var session = Loaded(saveFile);
        var (at, _) = AddableItem(session.Game!)!.Value;
        Dispatcher.Dispatch(session, "inventory.setItem", Args(at, 1));

        Value(Dispatcher.Dispatch(session, "inventory.setItem", Args(at, 0))).Should().BeNull();

        Count(session, at).Should().BeNull();
        session.Game!.SaveAndReload(reloaded => reloaded.Trainer.Inventories[at.Pouch].Items
            .Should().NotContain(i => i.Id == at.ItemId));
    }

    [Fact]
    public void SetItemRemovesAnOwnedItemThePouchDoesNotSupport()
    {
        var session = Loaded(SaveFilePath.Crystal);
        var at = new ItemHandle("KeyItems", 255);
        Count(session, at).Should().NotBeNull();

        Value(Dispatcher.Dispatch(session, "inventory.setItem", Args(at, 0))).Should().BeNull();

        Count(session, at).Should().BeNull();
    }

    [Fact]
    public void SetItemChangesTheInventoryTopic()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var (at, _) = AddableItem(session.Game!)!.Value;
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Dispatcher.Dispatch(session, "inventory.setItem", Args(at, 1));

        changes.Should().BeEquivalentTo([new[] { Topics.Inventory }]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void SetItemFailsWithOutOfRangeOutsideZeroToTheMaxCount(int offset)
    {
        var session = Loaded(SaveFilePath.HgSs);
        var (at, maxCount) = AddableItem(session.Game!)!.Value;
        var count = offset < 0 ? offset : maxCount + offset;

        Error(Dispatcher.Dispatch(session, "inventory.setItem", Args(at, count))).Should().Be("out-of-range");
        Count(session, at).Should().BeNull();
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.Crystal)]
    public void SetItemFailsWithPouchFullForANewItemInAFullPouch(string saveFile)
    {
        var session = Loaded(saveFile);
        var inventory = session.Game!.Trainer.Inventories["Items"];
        var rejected = inventory.CurrentSupportedItems.First(item => !inventory.Set(item.Id, 1));
        var at = new ItemHandle("Items", rejected.Id);
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Error(Dispatcher.Dispatch(session, "inventory.setItem", Args(at, 1))).Should().Be("pouch-full");

        Count(session, at).Should().BeNull();
        changes.Should().BeEmpty();
    }

    [Fact]
    public void SetItemFailsWithNotFoundForAPouchTheSaveDoesNotHave() =>
        Error(Dispatcher.Dispatch(Loaded(SaveFilePath.HgSs), "inventory.setItem", Args(new ItemHandle("Treasure", 1), 1)))
            .Should().Be("not-found");

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    public void SetItemFailsWithBadArgumentsForAnItemThePouchDoesNotSupport(int itemId)
    {
        var session = Loaded(SaveFilePath.HgSs);
        var balls = session.Game!.Trainer.Inventories["Balls"];
        var at = new ItemHandle("Balls", itemId == 1 ? FirstItemNotIn(session.Game!.Trainer.Inventories["Items"], balls) : itemId);

        Error(Dispatcher.Dispatch(session, "inventory.setItem", Args(at, 1))).Should().Be("bad-arguments");
    }

    [Fact]
    public void SetItemFailsWithNoSaveWithoutALoadedSave() =>
        Error(Dispatcher.Dispatch(new Session(), "inventory.setItem", Args(new ItemHandle("Items", 1), 1))).Should().Be("no-save");

    private static int FirstItemNotIn(Inventory source, Inventory target) =>
        source.AllSupportedItems.First(item => !target.Supports(item)).Id;

    private static int? Count(Session session, ItemHandle at) => Value(Dispatcher.Dispatch(session, "inventory.get", "[]"))!
        .AsArray()
        .Single(p => p!["name"]!.GetValue<string>() == at.Pouch)!["items"]!
        .AsArray()
        .SingleOrDefault(i => i!["id"]!.GetValue<int>() == at.ItemId)?["count"]!.GetValue<int>();

    private static int[] Ids(JsonNode items) => items.AsArray().Select(i => i!["id"]!.GetValue<int>()).ToArray();

    private static JsonNode Item(JsonArray pouches, string pouch, int itemId) => pouches
        .Single(p => p!["name"]!.GetValue<string>() == pouch)!
        .AsObject()
        .Where(p => p.Key is "items" or "addable")
        .SelectMany(p => p.Value!.AsArray())
        .Single(i => i!["id"]!.GetValue<int>() == itemId)!;
}
