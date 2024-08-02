using FluentAssertions;
using PKHeX.Core;
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
        
        pokemon.Owner.InheritFrom(game);

        pokemon.Owner.BelongsTo(game.Trainer).Should().BeTrue();
    }

    private Game AGame(GameVersion version, string trainerName) =>
        Game.EmptyOf(GameVersionRepository.Instance.Get(version), trainerName);
}