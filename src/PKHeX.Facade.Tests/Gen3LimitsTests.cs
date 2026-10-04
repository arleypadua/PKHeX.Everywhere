using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class Gen3LimitsTests
{
    private const ushort AboveGen3Species = 412;
    private const ushort AboveGen3Move = 355;

    [Theory]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.FireRed)]
    public void AVanillaGen3SaveLoads(string path) =>
        Game.LoadFrom(File.ReadAllBytes(path), path).SaveFile.Should().BeAssignableTo<SAV3>();

    [Fact]
    public void ABoxPokemonAtGen3sLimitsLoads()
    {
        var bytes = Gen3SaveOverLimits.EmeraldWithFirstBoxPokemon(pk =>
        {
            pk.SpeciesInternal = AboveGen3Species - 1;
            pk.Move1 = AboveGen3Move - 1;
        });

        Game.LoadFrom(bytes, "emerald.sav").SaveFile.Should().BeOfType<SAV3E>();
    }

    [Fact]
    public void ABoxPokemonWithASpeciesAboveGen3sMaximumFailsToLoad()
    {
        var load = () => Game.LoadFrom(Gen3SaveOverLimits.EmeraldWithBoxSpecies(AboveGen3Species), "emerald.sav");

        load.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void ABoxPokemonWithAMoveAboveGen3sMaximumFailsToLoad()
    {
        var load = () => Game.LoadFrom(Gen3SaveOverLimits.EmeraldWithBoxMove(AboveGen3Move), "emerald.sav");

        load.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void APartyPokemonWithASpeciesAboveGen3sMaximumFailsToLoad()
    {
        var load = () => Game.LoadFrom(Gen3SaveOverLimits.EmeraldWithFirstPartyPokemon(pk => pk.SpeciesInternal = AboveGen3Species), "emerald.sav");

        load.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void AnEmptyBoxSlotWithAMoveAboveGen3sMaximumFailsToLoad()
    {
        var load = () => Game.LoadFrom(Gen3SaveOverLimits.EmeraldWithEmptyBoxSlot(pk => pk.Move1 = AboveGen3Move), "emerald.sav");

        load.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void ChoosingPKHeXLoadsASaveAboveGen3sLimits()
    {
        var bytes = Gen3SaveOverLimits.EmeraldWithBoxSpecies(AboveGen3Species);

        Game.LoadFrom(bytes, "emerald.sav", SaveFormats.PKHeX).SaveFile.Should().BeOfType<SAV3E>();
    }

    [Fact]
    public void ChoosingPKHeXLoadsASaveWithAMoveAboveGen3sMaximum()
    {
        var bytes = Gen3SaveOverLimits.EmeraldWithBoxMove(AboveGen3Move);

        Game.LoadFrom(bytes, "emerald.sav", SaveFormats.PKHeX).SaveFile.Should().BeOfType<SAV3E>();
    }
}
