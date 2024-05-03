using FluentAssertions;
using PKHeX.Core;

namespace PKHeX.Facade.Tests;

public class InventoryTests
{
    [Fact]
    public void InventoryRepository_ShouldReturnExpectedItem()
    {
        var game = GameFixture.CreateTestGame();
        var masterball = game.ItemRepository.GetItem(1);
        masterball.Should().Be(MasterBall);
    }

    [Fact]
    public void Inventories_ShouldContainBallsInventory()
    {
        var game = GameFixture.CreateTestGame();
        game.Trainer.Inventories.InventoryTypes.Should().Contain("Balls");
        
        var ballInventory = game.Trainer.Inventories.InventoryItems["Balls"];
        ballInventory.SupportedItems.Should().Contain(MasterBall);
    }

    [Fact]
    public void Inventories_ShouldAllowChangingItemAmount()
    {
        var game = GameFixture.CreateTestGame();
        var ballInventory = game.Trainer.Inventories.InventoryItems["Balls"];
        ballInventory.Set(MasterBall.Id, 5);

        game.SaveAndReload(reloadedGame =>
        {
            reloadedGame.Trainer.Inventories.InventoryItems["Balls"].Items
                .Should().ContainSingle(i => i.Id == MasterBall.Id && i.Count == 5);
        });
    }

    [Fact]
    public void Inventories_ShouldAllowRemovingItem()
    {
        var game = GameFixture.CreateTestGame();
        var ballInventory = game.Trainer.Inventories.InventoryItems["Balls"];
        ballInventory.Remove(MasterBall.Id);

        game.SaveAndReload(reloadedGame =>
        {
            reloadedGame.Trainer.Inventories.InventoryItems["Balls"].Items
                .Should().NotContain(i => i.Id == MasterBall.Id);
        });
    }
}