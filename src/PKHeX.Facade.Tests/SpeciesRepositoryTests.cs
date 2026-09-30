using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class SpeciesRepositoryTests
{
    [Fact]
    public void ShouldResolveSpeciesOfPokemonFromAnotherGame()
    {
        var emerald = Game.LoadFrom(SaveFilePath.Emerald);

        var loaded = Pokemon.LoadFrom(CyclizarFileBytes(), emerald);

        loaded.Species.Species.Should().Be(Species.Cyclizar);
        emerald.IsAwareOf(loaded).Should().BeFalse();
    }

    [Fact]
    public void ShouldResolveUnknownSpecies()
    {
        var game = Game.LoadFrom(SaveFilePath.Emerald);

        game.SpeciesRepository.Get((Species)32000).Name.Should().Be("Unknown (32000)");
    }

    private static byte[] CyclizarFileBytes()
    {
        var gen9 = Game.EmptyOf(GameVersionRepository.Instance.Get(GameVersion.SL));
        // a blank PK9 with obedience level 0 is detected as PK8
        var cyclizar = new Pokemon(new PK9 { ObedienceLevel = 1 }, gen9);
        cyclizar.Species = SpeciesRepository.All[Species.Cyclizar];
        gen9.IsAwareOf(cyclizar).Should().BeTrue();
        return cyclizar.ToFile().Bytes;
    }
}
