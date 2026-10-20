using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class ShadowPokemonTests
{
    [Theory]
    [InlineData(GameVersion.COLO)]
    [InlineData(GameVersion.XD)]
    public void ReadsAShadowPokemon(GameVersion version)
    {
        var pokemon = GameFixture.WithShadowPokemon(version).Trainer.PokemonBox.All[0];

        pokemon.Details().Should().BeEquivalentTo(new { IsShadow = true, ShadowId = 10, Purification = 3000 });
    }

    [Theory]
    [InlineData(GameVersion.COLO)]
    [InlineData(GameVersion.XD)]
    public void APurifiedPokemonStaysPurifiedAfterExport(GameVersion version)
    {
        var game = GameFixture.WithShadowPokemon(version);
        var box = game.Trainer.PokemonBox;

        box.All[0].Update(new PokemonPatch { IsShadow = false });
        box.Commit();

        game.SaveAndReload(reloaded =>
            reloaded.Trainer.PokemonBox.All[0].Details().Should().BeEquivalentTo(new { IsShadow = false, ShadowId = 10, Purification = 0 }));
    }

    [Theory]
    [InlineData(GameVersion.COLO)]
    [InlineData(GameVersion.XD)]
    public void SetsThePurificationOfAShadowPokemon(GameVersion version)
    {
        var game = GameFixture.WithShadowPokemon(version);
        var box = game.Trainer.PokemonBox;

        box.All[0].Update(new PokemonPatch { Purification = 1200 });
        box.Commit();

        game.SaveAndReload(reloaded =>
            reloaded.Trainer.PokemonBox.All[0].Details().Should().BeEquivalentTo(new { IsShadow = true, Purification = 1200 }));
    }

    [Fact]
    public void RejectsMakingAPurifiedPokemonShadowAgain()
    {
        var pokemon = GameFixture.WithShadowPokemon(GameVersion.COLO).Trainer.PokemonBox.All[0];
        pokemon.Update(new PokemonPatch { IsShadow = false });

        var update = () => pokemon.Update(new PokemonPatch { IsShadow = true });

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(nameof(PokemonPatch.IsShadow));
    }

    [Fact]
    public void RejectsAPurificationForAPurifiedPokemon()
    {
        var pokemon = GameFixture.WithShadowPokemon(GameVersion.XD).Trainer.PokemonBox.All[0];
        pokemon.Update(new PokemonPatch { IsShadow = false });

        var update = () => pokemon.Update(new PokemonPatch { Purification = 100 });

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(nameof(PokemonPatch.Purification));
    }

    [Fact]
    public void RejectsShadowFieldsOutsideColosseumAndXD()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.Emerald).Trainer.Party.Pokemons[0];

        var update = () => pokemon.Update(new PokemonPatch { IsShadow = false });

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(nameof(PokemonPatch.IsShadow));
    }

    [Fact]
    public void ReportsNoShadowFieldsOutsideColosseumAndXD()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.Emerald).Trainer.Party.Pokemons[0];

        pokemon.Details().Should().BeEquivalentTo(new { IsShadow = (bool?)null, ShadowId = (int?)null, Purification = (int?)null });
    }
}
