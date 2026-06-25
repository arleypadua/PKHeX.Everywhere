using AwesomeAssertions;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class InventoryTests
{
    [Theory]
    [SupportedSaveFiles]
    public void InventoryRepository_ShouldReturnExpectedItem(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        var masterball = ItemRepository.GetItem(1);
        masterball.Should().Be(MasterBall);
    }

    [Theory]
    [SupportedSaveFiles]
    public void Inventories_ShouldContainBallsInventory(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        game.Trainer.Inventories.InventoryTypes.Should().Contain("Balls");
        
        var ballInventory = game.Trainer.Inventories.InventoryItems["Balls"];
        ballInventory.AllSupportedItems.Should().Contain(MasterBall);
    }

    [Theory]
    [SupportedSaveFiles]
    public void Inventories_ShouldAllowChangingItemAmount(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        var ballInventory = game.Trainer.Inventories.InventoryItems["Balls"];
        ballInventory.Set(MasterBall.Id, 5);

        game.SaveAndReload(reloadedGame =>
        {
            reloadedGame.Trainer.Inventories.InventoryItems["Balls"].Items
                .Should().ContainSingle(i => i.Id == MasterBall.Id && i.Count == 5);
        });
    }

    [Theory]
    [SupportedSaveFiles]
    public void Inventories_ShouldAllowRemovingItem(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        var ballInventory = game.Trainer.Inventories.InventoryItems["Balls"];
        ballInventory.Remove(MasterBall.Id);

        game.SaveAndReload(reloadedGame =>
        {
            reloadedGame.Trainer.Inventories.InventoryItems["Balls"].Items
                .Should().NotContain(i => i.Id == MasterBall.Id);
        });
    }

    [Theory]
    [SupportedSaveFiles]
    public void Invories_CanAddRareCandies(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);

        var rareCandyBag =
            game.Trainer.Inventories.InventoryItems.Values.FirstOrDefault(i =>
                i.AllSupportedItems.Any(s => s.Name == "Rare Candy"));
        
        rareCandyBag.Should().NotBeNull();

        var rareCandyDefinition = rareCandyBag!.AllSupportedItems.First(s => s.Name == "Rare Candy");
        rareCandyBag.Set(rareCandyDefinition.Id, 5);

        game.SaveAndReload(reloadedGame =>
        {
            reloadedGame.Trainer.Inventories[rareCandyBag.Type].Items
                .Should().ContainSingle(i => i.Id == rareCandyDefinition.Id && i.Count == 5);
        });
    }

    [Theory]
    [SupportedSaveFiles]
    public void Inventories_ShouldPersistChangesInMultiplePouchesSimultaneously(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);

        var ballInventory = game.Trainer.Inventories.InventoryItems["Balls"];
        ballInventory.Set(MasterBall.Id, 7);

        var otherInventory = game.Trainer.Inventories.InventoryItems
            .First(kvp => kvp.Key != "Balls"
                       && kvp.Key != "KeyItems"
                       && kvp.Key != "TMHMs"
                       && kvp.Value.AllSupportedItems.Any(i => !i.IsNone));
        var otherItem = otherInventory.Value.AllSupportedItems.First(i => !i.IsNone);
        var expectedCount = Math.Min(9, otherInventory.Value.MaxItemCountAllowed);
        otherInventory.Value.Set(otherItem.Id, Convert.ToUInt16(expectedCount));

        game.SaveAndReload(reloadedGame =>
        {
            reloadedGame.Trainer.Inventories["Balls"].Items
                .Should().ContainSingle(i => i.Id == MasterBall.Id && i.Count == 7);
            reloadedGame.Trainer.Inventories[otherInventory.Key].Items
                .Should().ContainSingle(i => i.Id == otherItem.Id && i.Count == expectedCount);
        });
    }
}