using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class GameVersionRepositoryTests
{
    [Fact]
    public void BlankVersionsAreTheSavedVersionsSortedByName()
    {
        var blank = GameVersionRepository.Instance.Blank;

        blank.Should().NotBeEmpty();
        blank.Should().NotContain(v => v.Aggregated);
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
