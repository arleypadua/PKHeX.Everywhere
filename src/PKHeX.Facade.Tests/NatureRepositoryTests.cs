using AwesomeAssertions;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class NatureRepositoryTests
{
    [Fact]
    public void GetNatureNamesANature() =>
        NatureRepository.GetNature(3).Should().Be(new NatureDefinition(3, "Adamant"));

    [Fact]
    public void GetNatureNamesNatureZero() =>
        NatureRepository.GetNature(0).Should().Be(new NatureDefinition(0, "Hardy"));

    [Fact]
    public void GetNatureNamesAnUnknownNature() =>
        NatureRepository.GetNature(25).Should().Be(new NatureDefinition(25, "Unknown Nature 25"));
}
