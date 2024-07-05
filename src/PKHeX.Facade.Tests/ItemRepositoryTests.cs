using FluentAssertions;
using PKHeX.Core;

namespace PKHeX.Facade.Tests;

public class ItemRepositoryTests
{
    [Theory]
    [Games(GameVersion.HG)]
    public void ShouldLoadGameSpecificBalls(Game game)
    {
        game.ItemRepository.GetBall(Ball.Sport).Should().NotBeNull();
    }
}