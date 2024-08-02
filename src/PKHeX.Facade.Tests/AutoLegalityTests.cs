using FluentAssertions;
using PKHeX.Core;
using PKHeX.Facade.Extensions;

namespace PKHeX.Facade.Tests;

public class AutoLegalityTests
{
    [Theory]
    [Games(GameVersion.GP)]
    public async Task ShouldGetLegal(Game game)
    {
        AutoLegality.ApplyDefaultConfiguration();
        
        var pikachu = game.Trainer.Party.Pokemons.Single(p => p.Species == Species.Pikachu);
        
        pikachu.ChangeLevel(50);
        pikachu.Level.Should().Be(50);
        pikachu.Legality().Valid.Should().BeFalse();

        var result = await pikachu.ToLegalAsync();
        
        result.Level.Should().Be(50);
        result.Legality().Valid.Should().BeTrue();
    }
    
    [Theory]
    [Games(GameVersion.GP)]
    public async Task ShouldGetLegal_TransferEvol(Game game)
    {
        AutoLegality.ApplyDefaultConfiguration();
        
        var gloom = game.Trainer.Party.Pokemons.Single(p => p.Species == Species.Gloom);
        gloom.Species = game.SpeciesRepository.Get(Species.Vileplume);

        await gloom.ApplyLegalAsync();
        
        gloom.Legality().Valid.Should().BeTrue();
    }
    
    [Theory]
    [Games(GameVersion.GP)]
    public async Task ShouldApplyAutoLegality(Game game)
    {
        AutoLegality.ApplyDefaultConfiguration();
        
        var pikachu = game.Trainer.Party.Pokemons.Single(p => p.Species == Species.Pikachu);
        var pid = pikachu.PID;
        
        pikachu.ChangeLevel(50);
        pikachu.Level.Should().Be(50);
        pikachu.Legality().Valid.Should().BeFalse();

        await pikachu.ApplyLegalAsync();
        
        pikachu.Level.Should().Be(50);
        pikachu.Legality().Valid.Should().BeTrue();
        pikachu.PID.Should().Be(pid);
    }
    
    [Theory(Skip = "SoulSilver legalization doesn't seem to work properly")]
    [Games(GameVersion.SS)]
    public async Task ShouldApplyAutoLegality_SoulSiver(Game game)
    {
        AutoLegality.ApplyDefaultConfiguration(TimeSpan.FromSeconds(1));
        
        var goldeen = game.Trainer.PokemonBox.All.Single(p => p.Species == Species.Goldeen);
        var pid = goldeen.PID;
        
        goldeen.ChangeLevel(30);
        goldeen.Species = game.SpeciesRepository.Get(Species.Seaking);
        
        goldeen.Level.Should().Be(30);
        goldeen.Species.Species.Should().Be(Species.Seaking);
        goldeen.Legality().Valid.Should().BeFalse();

        await goldeen.ApplyLegalAsync();
        
        goldeen.Level.Should().Be(30);
        goldeen.Legality().Valid.Should().BeTrue();
        goldeen.PID.Should().Be(pid);
    }
}