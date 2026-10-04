using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class InventoryTests
{
    [Fact]
    public void Inventories_EmptyLegendsArceus_ShouldLoadWithoutPockets()
    {
        var game = Game.EmptyOf(GameVersionRepository.Instance.Get(GameVersion.PLA));

        game.Trainer.Inventories.InventoryTypes.Should().BeEmpty();
    }

    [Theory]
    [InlineData(SaveFilePath.Crystal, 243)]
    [InlineData(SaveFilePath.Emerald, 339)]
    public void MaxCountOf_HM_ShouldBeOne(string saveFile, ushort hm01)
    {
        var tmhms = SaveFilePath.Load(saveFile).Trainer.Inventories["TMHMs"];

        tmhms.MaxCountOf(hm01).Should().Be(1);
        tmhms.MaxCountOf((ushort)(hm01 - 1)).Should().Be(tmhms.MaxItemCountAllowed);
    }

    [Theory]
    [InlineData("Items", 113, 76)]
    [InlineData("BattleItems", 656, 55)]
    public void MaxCountOf_LetsGoKeyItemOrMegaStone_ShouldBeOne(string pouch, ushort limited, ushort regular)
    {
        var inventory = Game.LoadFrom(SaveFilePath.LetsGoPikachu).Trainer.Inventories[pouch];

        inventory.MaxCountOf(limited).Should().Be(1);
        inventory.MaxCountOf(regular).Should().Be(inventory.MaxItemCountAllowed);
    }

    [Theory]
    [SupportedSaveFiles]
    public void InventoryRepository_ShouldReturnExpectedItem(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var masterball = ItemRepository.GetItem(1);
        masterball.Should().Be(MasterBall);
    }

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Imperium])] // Imperium's bag isn't read yet
    public void Inventories_ShouldContainBallsInventory(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        game.Trainer.Inventories.InventoryTypes.Should().Contain("Balls");
        
        var ballInventory = game.Trainer.Inventories.InventoryItems["Balls"];
        ballInventory.AllSupportedItems.Should().Contain(MasterBall);
    }

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Imperium])] // Imperium's bag isn't read yet
    public void Inventories_ShouldAllowChangingItemAmount(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var ballInventory = game.Trainer.Inventories.InventoryItems["Balls"];
        ballInventory.Set(MasterBall.Id, 5);

        game.SaveAndReload(reloadedGame =>
        {
            reloadedGame.Trainer.Inventories.InventoryItems["Balls"].Items
                .Should().ContainSingle(i => i.Id == MasterBall.Id && i.Count == 5);
        });
    }

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Imperium])] // Imperium's bag isn't read yet
    public void Inventories_ShouldAllowRemovingItem(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var ballInventory = game.Trainer.Inventories.InventoryItems["Balls"];
        ballInventory.Remove(MasterBall.Id);

        game.SaveAndReload(reloadedGame =>
        {
            reloadedGame.Trainer.Inventories.InventoryItems["Balls"].Items
                .Should().NotContain(i => i.Id == MasterBall.Id);
        });
    }

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Imperium])] // Imperium's bag isn't read yet
    public void Invories_CanAddRareCandies(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);

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
    public void Inventories_EditsToTwoPouches_ShouldBothSurvive(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var pouches = game.Trainer.Inventories.InventoryItems.Values.Where(pouch => !pouch.CurrentSupportedItems.IsEmpty).Take(2).ToArray();
        var edits = pouches.Select(pouch => (pouch.Type, Item: pouch.CurrentSupportedItems.First())).ToArray();

        foreach (var (type, item) in edits) game.Trainer.Inventories[type].Set(item.Id, 1);

        game.SaveAndReload(reloaded =>
        {
            foreach (var (type, item) in edits)
                reloaded.Trainer.Inventories[type].Items.Should().ContainSingle(i => i.Id == item.Id && i.Count == 1);
        });
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald, "Items")]
    [InlineData(SaveFilePath.Crystal, "Items")]
    public void TrySet_NewItemInAFullPouch_ShouldReturnFalseAndLeaveThePouchUnchanged(string saveFile, string pouch)
    {
        var game = SaveFilePath.Load(saveFile);
        var inventory = game.Trainer.Inventories[pouch];
        var rejected = Fill(inventory);
        var before = inventory.Items.Select(i => (i.Id, i.Count)).ToList();

        inventory.TrySet(rejected.Id, 1).Should().BeFalse();

        inventory.Items.Select(i => (i.Id, i.Count)).Should().Equal(before);
        game.SaveAndReload(reloaded => reloaded.Trainer.Inventories[pouch].Items
            .Should().NotContain(i => i.Id == rejected.Id));
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald, "Items")]
    [InlineData(SaveFilePath.Crystal, "Items")]
    public void TrySet_OwnedItemInAFullPouch_ShouldReturnTrue(string saveFile, string pouch)
    {
        var inventory = SaveFilePath.Load(saveFile).Trainer.Inventories[pouch];
        Fill(inventory);
        var owned = inventory.AllExceptNone().First();

        inventory.TrySet(owned.Id, 2).Should().BeTrue();

        inventory.Items.Should().ContainSingle(i => i.Id == owned.Id && i.Count == 2);
    }

    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Imperium])] // Imperium's bag isn't read yet
    public void TrySet_OwnedItem_ShouldChangeItsCountWhereItIs(string saveFile)
    {
        var inventory = SaveFilePath.Load(saveFile).Trainer.Inventories.InventoryItems.Values.First(i => i.AllExceptNone().Any(item => item.Count > 1));
        var owned = inventory.AllExceptNone().Last(item => item.Count > 1);
        var before = inventory.Items.Select(i => i.Id).ToList();

        inventory.TrySet(owned.Id, 1).Should().BeTrue();

        inventory.Items.Select(i => i.Id).Should().Equal(before);
        inventory.Items.Single(i => i.Id == owned.Id).Count.Should().Be(1);
    }

    private static ItemDefinition Fill(Inventory inventory) =>
        inventory.CurrentSupportedItems.First(item => !inventory.TrySet(item.Id, 1));
}
