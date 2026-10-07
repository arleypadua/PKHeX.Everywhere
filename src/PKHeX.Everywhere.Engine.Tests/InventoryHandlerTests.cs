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

        var pouches = Value(Dispatch(session, "inventory.get", "[]"))!;

        var expected = session.Game!.Trainer.Inventories.InventoryItems.Values
            .OrderBy(i => i.Type, StringComparer.Ordinal)
            .Select(i => new
            {
                name = i.Type,
                items = i.AllExceptNone().Select(item => new { id = item.Id, index = item.Index, name = item.Name, count = item.Count, maxCount = i.MaxCountOf(item.Id), isUnknown = item.IsUnknown }),
                addable = i.CurrentSupportedItems.Select(item => new { id = item.Id, index = item.Index, name = item.Name, maxCount = i.MaxCountOf(item.Id) }),
            });
        pouches.ToJsonString().Should().Be(JsonSerializer.Serialize(expected));
        pouches.AsArray().Should().NotBeEmpty();
    }

    [Theory]
    [SupportedSaveFiles]
    public void GetLeavesOutEmptySlotsAndOwnedItemsFromAddable(string saveFile)
    {
        var pouches = Value(Dispatch(Loaded(saveFile), "inventory.get", "[]"))!.AsArray();

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
        var pouches = Value(Dispatch(Loaded(saveFile), "inventory.get", "[]"))!.AsArray();

        var item = Item(pouches, pouch, itemId);
        item["maxCount"]!.GetValue<int>().Should().Be(1);
    }

    [Fact]
    public void GetReturnsNoPouchesWhenPkHeXCannotReadTheInventory()
    {
        var session = new Session();
        session.Load(Game.EmptyOf(GameVersionRepository.Instance.Get(GameVersion.PLA)), "legends.bin");

        Value(Dispatch(session, "inventory.get", "[]"))!.AsArray().Should().BeEmpty();
    }

    [Fact]
    public void GetReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "inventory.get", "[]")).Should().Be("no-save");

    [Theory]
    [SupportedSaveFiles]
    public void SetItemAddsAnItemTheSaveDoesNotOwn(string saveFile)
    {
        var session = Loaded(saveFile);
        var (at, maxCount) = AddableItem(session.Game!)!.Value;

        Value(Dispatch(session, "inventory.setItem", Args(at, maxCount))).Should().BeNull();

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
        Dispatch(session, "inventory.setItem", Args(at, maxCount));

        Value(Dispatch(session, "inventory.setItem", Args(at, 1))).Should().BeNull();

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
        Dispatch(session, "inventory.setItem", Args(at, 1));

        Value(Dispatch(session, "inventory.setItem", Args(at, 0))).Should().BeNull();

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

        Value(Dispatch(session, "inventory.setItem", Args(at, 0))).Should().BeNull();

        Count(session, at).Should().BeNull();
    }

    [Fact]
    public void SetItemChangesTheInventoryTopic()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var (at, _) = AddableItem(session.Game!)!.Value;
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Dispatch(session, "inventory.setItem", Args(at, 1));

        changes.Should().BeEquivalentTo([new[] { Topics.Inventory }]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    public void SetItemPublishesItemChangedWithTheNewCount(int count)
    {
        var session = Loaded(SaveFilePath.HgSs);
        var (at, maxCount) = AddableItem(session.Game!)!.Value;
        Dispatch(session, "inventory.setItem", Args(at, maxCount));
        var published = new List<IEngineEvent>();
        session.Published += published.Add;

        Dispatch(session, "inventory.setItem", Args(at, count));

        published.Should().Equal(new ItemChanged(at.ItemId, count));
    }

    [Fact]
    public void SetItemPublishesAfterInvalidatingItsTopics()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var (at, _) = AddableItem(session.Game!)!.Value;
        var seen = new List<string>();
        session.Changed += _ => seen.Add("changed");
        session.Published += _ => seen.Add("published");

        Dispatch(session, "inventory.setItem", Args(at, 1));

        seen.Should().Equal("changed", "published");
    }

    [Fact]
    public void SetItemPublishesNothingWhenItFails()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var (at, maxCount) = AddableItem(session.Game!)!.Value;
        var published = new List<IEngineEvent>();
        session.Published += published.Add;

        Error(Dispatch(session, "inventory.setItem", Args(at, maxCount + 1))).Should().Be("out-of-range");

        published.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void SetItemFailsWithOutOfRangeOutsideZeroToTheMaxCount(int offset)
    {
        var session = Loaded(SaveFilePath.HgSs);
        var (at, maxCount) = AddableItem(session.Game!)!.Value;
        var count = offset < 0 ? offset : maxCount + offset;

        Error(Dispatch(session, "inventory.setItem", Args(at, count))).Should().Be("out-of-range");
        Count(session, at).Should().BeNull();
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.Crystal)]
    public void SetItemFailsWithPouchFullForANewItemInAFullPouch(string saveFile)
    {
        var session = Loaded(saveFile);
        var inventory = session.Game!.Trainer.Inventories["Items"];
        var rejected = inventory.CurrentSupportedItems.First(item => !inventory.TrySet(item.Id, 1));
        var at = new ItemHandle("Items", rejected.Id);
        var changes = new List<string[]>();
        session.Changed += changes.Add;

        Error(Dispatch(session, "inventory.setItem", Args(at, 1))).Should().Be("pouch-full");

        Count(session, at).Should().BeNull();
        changes.Should().BeEmpty();
    }

    [Fact]
    public void SetItemFailsWithNotFoundForAPouchTheSaveDoesNotHave() =>
        Error(Dispatch(Loaded(SaveFilePath.HgSs), "inventory.setItem", Args(new ItemHandle("Treasure", 1), 1)))
            .Should().Be("not-found");

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SetItemFailsWithBadArgumentsForAnInvalidItemId(int itemId) =>
        Error(Dispatch(Loaded(SaveFilePath.HgSs), "inventory.setItem", Args(new ItemHandle("Balls", itemId), 1)))
            .Should().Be("bad-arguments");

    [Fact]
    public void SetItemFailsWithBadArgumentsForAnItemThePouchDoesNotSupport()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var inventories = session.Game!.Trainer.Inventories;
        var notABall = inventories["Items"].AllSupportedItems.First(item => !inventories["Balls"].Supports(item));

        Error(Dispatch(session, "inventory.setItem", Args(new ItemHandle("Balls", notABall.Id), 1)))
            .Should().Be("bad-arguments");
    }

    [Fact]
    public void SetItemWithACountOfZeroDoesNothingForAnItemTheSaveDoesNotOwn()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var (at, _) = AddableItem(session.Game!)!.Value;
        var before = Dispatch(session, "inventory.get", "[]");

        Value(Dispatch(session, "inventory.setItem", Args(at, 0))).Should().BeNull();

        Dispatch(session, "inventory.get", "[]").Should().Be(before);
    }

    [Fact]
    public void SetItemFailsWithNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "inventory.setItem", Args(new ItemHandle("Items", 1), 1))).Should().Be("no-save");

    private static int? Count(Session session, ItemHandle at) => Value(Dispatch(session, "inventory.get", "[]"))!
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
