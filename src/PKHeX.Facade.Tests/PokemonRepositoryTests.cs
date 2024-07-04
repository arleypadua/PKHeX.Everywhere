using FluentAssertions;
using PKHeX.Core;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonRepositoryTests
{
    [Theory]
    [SupportedSaveFiles]
    public void ShouldEncounterPokemons(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        var encounters = game.PokemonRepository.FindEncounter(Species.Abra).ToList();
        encounters.Should().NotBeEmpty();
    }
}