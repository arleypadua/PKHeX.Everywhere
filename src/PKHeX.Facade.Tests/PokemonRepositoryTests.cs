using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonRepositoryTests
{
    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Unbound, SaveFilePath.RadicalRed])] // the hacks don't support Encounters
    public void ShouldEncounterPokemons(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var encounters = game.PokemonRepository.FindEncounter(game.GameVersionApproximation.Version, Species.Abra).ToList();
        encounters.Should().NotBeEmpty();
    }
}