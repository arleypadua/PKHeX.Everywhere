using FluentAssertions;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonBoxTests
{
    [Theory]
    [SupportedSaveFiles]
    public void BoxShouldContainPokemon(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        var allValid = game.Trainer.PokemonBox.All
            .Where(p => p.Species != SpeciesDefinition.None)
            .ToList();
        
        allValid.Should().HaveCountGreaterThan(0);
        allValid.Should().AllSatisfy(p =>
        {
            p.BaseStats.Attack.Should().BeGreaterThan(0);
            p.BaseStats.Defense.Should().BeGreaterThan(0);
            p.BaseStats.Health.Should().BeGreaterThan(0);
            p.BaseStats.Speed.Should().BeGreaterThan(0);
            p.BaseStats.Total.Should().BeGreaterThan(0);
            p.BaseStats.SpecialAttack.Should().BeGreaterThan(0);
            p.BaseStats.SpecialDefense.Should().BeGreaterThan(0);
        });
    }
}
