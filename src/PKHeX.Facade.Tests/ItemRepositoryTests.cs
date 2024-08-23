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
}