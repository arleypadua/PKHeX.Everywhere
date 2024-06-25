using FluentAssertions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class InventoryTests
{
    [Theory]
    [SupportedSaveFiles]
    public void InventoryRepository_ShouldReturnExpectedItem(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        var masterball = game.ItemRepository.GetItem(1);
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
}