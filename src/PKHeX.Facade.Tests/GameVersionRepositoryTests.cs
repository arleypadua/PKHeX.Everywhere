using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class GameVersionRepositoryTests
{
    [Fact]
    public void BlankVersionsAreTheSavedVersionsAndTheStadiumsSortedByName()
    {
        var blank = GameVersionRepository.Instance.Blank;

        blank.Should().NotBeEmpty();
        blank.Where(v => v.Aggregated).Select(v => v.Version).Order().Should().Equal(GameVersion.StadiumJ, GameVersion.Stadium, GameVersion.Stadium2);
        blank.Select(v => v.Name).Should().BeInAscendingOrder();
    }

    [Fact]
    public void FindBlankReturnsNullForAnAggregatedOrUnknownVersion()
    {
        GameVersionRepository.Instance.FindBlank((int)GameVersion.SW)!.Version.Should().Be(GameVersion.SW);
        GameVersionRepository.Instance.FindBlank((int)GameVersion.HGSS).Should().BeNull();
        GameVersionRepository.Instance.FindBlank(-1).Should().BeNull();
    }
}
