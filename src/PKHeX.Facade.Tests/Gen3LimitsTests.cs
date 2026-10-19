using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class Gen3LimitsTests
{
    private const ushort AboveGen3Species = 412;
    private const ushort AboveGen3Move = 355;

    private static void ShouldAskWhetherItIsAROMHack(Func<Game> load)
    {
        var choice = load.Should().Throw<FormatChoiceRequiredException>().Which;
        choice.Candidates.Should().BeEmpty();
        choice.PKHeXGame.Should().Be("Emerald");
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.FireRed)]
    public void AVanillaGen3SaveLoads(string path) =>
        Game.LoadFrom(File.ReadAllBytes(path), path).SaveFile.Should().BeAssignableTo<SAV3>();

    [Fact]
    public void ABoxPokemonAtGen3sLimitsLoads()
    {
        var bytes = EditedEmerald.WithFirstBoxPokemon(pk =>
        {
            pk.SpeciesInternal = AboveGen3Species - 1;
            pk.Move1 = AboveGen3Move - 1;
        });

        Game.LoadFrom(bytes, "emerald.sav").SaveFile.Should().BeOfType<SAV3E>();
    }

    [Fact]
    public void ABoxPokemonWithASpeciesAboveGen3sMaximumAsksWhetherItIsAROMHack()
    {
        var load = () => Game.LoadFrom(EditedEmerald.WithBoxSpecies(AboveGen3Species), "emerald.sav");

        ShouldAskWhetherItIsAROMHack(load);
    }

    [Fact]
    public void ABoxPokemonWithAMoveAboveGen3sMaximumAsksWhetherItIsAROMHack()
    {
        var load = () => Game.LoadFrom(EditedEmerald.WithBoxMove(AboveGen3Move), "emerald.sav");

        ShouldAskWhetherItIsAROMHack(load);
    }

    [Fact]
    public void APartyPokemonWithASpeciesAboveGen3sMaximumAsksWhetherItIsAROMHack()
    {
        var load = () => Game.LoadFrom(EditedEmerald.WithFirstPartyPokemon(pk => pk.SpeciesInternal = AboveGen3Species), "emerald.sav");

        ShouldAskWhetherItIsAROMHack(load);
    }

    [Fact]
    public void AnEmptyBoxSlotWithAMoveAboveGen3sMaximumAsksWhetherItIsAROMHack()
    {
        var load = () => Game.LoadFrom(EditedEmerald.WithEmptyBoxSlot(pk => pk.Move1 = AboveGen3Move), "emerald.sav");

        ShouldAskWhetherItIsAROMHack(load);
    }

    [Fact]
    public void AnEggWithASpeciesAboveGen3sMaximumAsksWhetherItIsAROMHack()
    {
        var load = () => Game.LoadFrom(EditedEmerald.WithFirstBoxPokemon(pk =>
        {
            pk.IsEgg = true;
            pk.SpeciesInternal = AboveGen3Species;
        }), "emerald.sav");

        ShouldAskWhetherItIsAROMHack(load);
    }

    [Fact]
    public void ChoosingPKHeXLoadsASaveAboveGen3sLimits()
    {
        var bytes = EditedEmerald.WithBoxSpecies(AboveGen3Species);

        Game.LoadFrom(bytes, "emerald.sav", SaveFormats.PKHeX).SaveFile.Should().BeOfType<SAV3E>();
    }

    [Fact]
    public void ChoosingPKHeXLoadsASaveWithAMoveAboveGen3sMaximum()
    {
        var bytes = EditedEmerald.WithBoxMove(AboveGen3Move);

        Game.LoadFrom(bytes, "emerald.sav", SaveFormats.PKHeX).SaveFile.Should().BeOfType<SAV3E>();
    }
}
