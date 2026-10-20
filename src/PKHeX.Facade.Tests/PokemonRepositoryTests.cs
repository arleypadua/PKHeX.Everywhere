using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonRepositoryTests
{
    [Theory]
    [SupportedSaveFiles(Except = [SaveFilePath.Unbound, SaveFilePath.Unbound21, SaveFilePath.RadicalRed, SaveFilePath.Imperium, SaveFilePath.HeartAndSoul])] // the hacks don't support Encounters
    public void ShouldEncounterPokemons(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var encounters = game.PokemonRepository.FindEncounter(game.GameVersionApproximation.Version, Species.Abra).ToList();
        encounters.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(GameVersion.E, GameVersion.E)]
    [InlineData(GameVersion.COLO, GameVersion.CXD)]
    [InlineData(GameVersion.XD, GameVersion.CXD)]
    [InlineData(GameVersion.RSBOX, GameVersion.E)]
    public void DefaultEncounterVersion_IsOneOfTheEncounterVersions(GameVersion save, GameVersion expected)
    {
        var repository = Game.EmptyOf(GameVersionRepository.Instance.Get(save)).PokemonRepository;

        repository.DefaultEncounterVersion().Version.Should().Be(expected);
        repository.EncounterVersions().Should().Contain(repository.DefaultEncounterVersion());
    }
}
