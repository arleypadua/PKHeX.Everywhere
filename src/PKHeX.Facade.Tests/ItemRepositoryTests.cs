using FluentAssertions;
using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class ItemRepositoryTests
{
    [Theory]
    [Games(GameVersion.HG)]
    public void ShouldLoadGameSpecificBalls(Game game)
    {
        ItemRepository.GetBall(Ball.Sport).Should().NotBeNull();
    }

    [Theory]
    [Games(GameVersion.E)]
    public void ShouldLoadRareCandy(Game game)
    {
        game.Trainer.Should().NotBeNull();
    }

    [Theory]
    [SupportedSaveFiles]
    public void ShouldLoadAndEnumerateAllItemsInTheRepository(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        foreach (var inventoryType in game.Trainer.Inventories.InventoryTypes)
        {
            var items = game.Trainer.Inventories[inventoryType].Where(i => i.Definition.Id != ItemDefinition.None)
                .ToList();

            items.Count.Should().BeGreaterThanOrEqualTo(0);
        }
    }
}