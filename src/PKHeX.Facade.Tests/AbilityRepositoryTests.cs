using AwesomeAssertions;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class AbilityRepositoryTests
{
    [Fact]
    public void GetAbilityNamesAnAbility() =>
        AbilityRepository.GetAbility(22).Should().Be(new AbilityDefinition(22, "Intimidate"));

    [Fact]
    public void GetAbilityNamesAbilityZeroAsPKHeXDoes() =>
        AbilityRepository.GetAbility(0).Should().Be(new AbilityDefinition(0, "(None)"));

    [Fact]
    public void GetAbilityNamesAnUnknownAbility() =>
        AbilityRepository.GetAbility(65000).Should().Be(new AbilityDefinition(65000, "Unknown Ability 65000"));
}
