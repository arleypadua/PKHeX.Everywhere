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

    private static int[] Ids(JsonNode items) => items.AsArray().Select(i => i!["id"]!.GetValue<int>()).ToArray();

    private static JsonNode Item(JsonArray pouches, string pouch, int itemId) => pouches
        .Single(p => p!["name"]!.GetValue<string>() == pouch)!
        .AsObject()
        .Where(p => p.Key is "items" or "addable")
        .SelectMany(p => p.Value!.AsArray())
        .Single(i => i!["id"]!.GetValue<int>() == itemId)!;
}
