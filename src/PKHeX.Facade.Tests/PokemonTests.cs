using FluentAssertions;
using PKHeX.Core;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class PokemonTests
{
    [Fact]
    public void ShouldLoadPokemonFromFile()
    {
        var pokemon = PokemonFile.LoadFor(GameVersion.SL);
        pokemon.Species.Species.Should().Be(Species.Golduck);
    }

    [Fact]
    public void ShouldAllowToInheritOwnerFromSave()
    {
        var game = AGame(GameVersion.SL, "someone");
        var pokemon = PokemonFile.LoadFor(GameVersion.SL, game);
        
        pokemon.Owner.Name.Should().NotBe(game.Trainer.Name);
        pokemon.Owner.Name = game.Trainer.Name;
        pokemon.Owner.Name.Should().Be(game.Trainer.Name);
    }

    [Theory]
    [SupportedSaveFiles(Except = [GameVersion.C])] // cloning in crystal is not working
    public async Task ShouldClonePokemonAndKeepEverythingTheSame(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);

        var pokemon = game.Trainer.Party.Pokemons.First();
        var clone = pokemon.Clone();
        var cloneFromBinary = Pokemon.LoadFrom(clone.ToFile().Bytes);
        var cloneWithLegality = Pokemon.LoadFrom(clone.ToFile().Bytes);
        await cloneWithLegality.ToLegalAsync();

        pokemon.Id.Should().Be(clone.Id);
        pokemon.Id.Should().Be(cloneFromBinary.Id);
        pokemon.Id.Should().Be(cloneWithLegality.Id);
        
        pokemon.UniqueId.Should().Be(clone.UniqueId);
        pokemon.UniqueId.Should().Be(cloneFromBinary.UniqueId);
        pokemon.UniqueId.Should().Be(cloneWithLegality.UniqueId);
        
        pokemon.Pkm.EncryptionConstant.Should().Be(clone.Pkm.EncryptionConstant);
        pokemon.Pkm.EncryptionConstant.Should().Be(cloneFromBinary.Pkm.EncryptionConstant);
        pokemon.Pkm.EncryptionConstant.Should().Be(cloneWithLegality.Pkm.EncryptionConstant);
    }

    private Game AGame(GameVersion version, string trainerName) =>
        Game.EmptyOf(GameVersionRepository.Instance.Get(version), trainerName);
}