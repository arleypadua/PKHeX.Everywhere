using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Tests;

public class PokemonConversionTests
{
    [Fact]
    public void ShouldConvertPokemonFromOlderGenerationToSaveFormat()
    {
        var emerald = Game.LoadFrom(SaveFilePath.Emerald);
        var hgss = Game.LoadFrom(SaveFilePath.HgSs);
        var bytes = emerald.Trainer.Party.Pokemons.First().ToFile().Bytes;

        var converted = Pokemon.LoadFrom(bytes, hgss).ConvertTo(hgss, out var result);

        result.Should().Be(EntityConverterResult.Success);
        converted!.Pkm.Should().BeOfType<PK4>();
        hgss.Trainer.PokemonBox.AddOnEmptySlot(converted).Should().BeTrue();
    }

    [Fact]
    public void ShouldNotConvertPokemonToOlderGeneration()
    {
        var emerald = Game.LoadFrom(SaveFilePath.Emerald);
        var hgss = Game.LoadFrom(SaveFilePath.HgSs);

        var converted = hgss.Trainer.Party.Pokemons.First().ConvertTo(emerald, out var result);

        converted.Should().BeNull();
        result.Should().Be(EntityConverterResult.NoTransferRoute);
    }
}
