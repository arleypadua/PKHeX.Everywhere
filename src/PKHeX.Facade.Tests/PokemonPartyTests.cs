using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonPartyTests
{
    [Theory]
    [SupportedSaveFiles]
    public void PartyShouldContainPokemon(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        game.Trainer.Party.Pokemons.Should().HaveCountGreaterThan(0);
        game.Trainer.Party.Pokemons.Should().AllSatisfy(p =>
        {
            p.BaseStats.Attack.Should().BeGreaterThan(0);
            p.BaseStats.Defense.Should().BeGreaterThan(0);
            p.BaseStats.Health.Should().BeGreaterThan(0);
            p.BaseStats.Speed.Should().BeGreaterThan(0);
            p.BaseStats.Total.Should().BeGreaterThan(0);
            p.BaseStats.SpecialAttack.Should().BeGreaterThan(0);
            p.BaseStats.SpecialDefense.Should().BeGreaterThan(0);
        });
    }

    [Theory]
    [SupportedSaveFiles]
    public void PartyShouldBePersistedAcrossSaves(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var firstPokemon = game.Trainer.Party.Pokemons.First();
        
        firstPokemon.IsShiny.Should().BeFalse();
        
        firstPokemon.SetShiny(true);
        
        firstPokemon.IsShiny.Should().BeTrue();
        
        game.SaveAndReload(savedGame =>
        {
            var savedPokemon = savedGame.Trainer.Party.Pokemons.First();
            savedPokemon.PID.Should().Be(firstPokemon.PID);
            savedPokemon.IsShiny.Should().Be(firstPokemon.IsShiny);
        });
    }

    [Theory]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    [InlineData(SaveFilePath.LetsGoEevee)]
    public void LetsGoBoxShowsCommittedPartyChanges(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var member = game.Trainer.Party.Pokemons[0];
        var index = game.Trainer.Party.BoxIndexOf(0)!.Value;
        var level = member.Level == 50 ? 51 : 50;

        member.ChangeLevel(level);
        game.Trainer.Party.Commit();

        game.Trainer.PokemonBox.All[index].Level.Should().Be(level);
        game.Trainer.PokemonBox.BySpecies[member.Species.Species].Should().Contain(p => p.Level == level);
    }

    [Theory]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    [InlineData(SaveFilePath.LetsGoEevee)]
    public void LetsGoPartyShowsCommittedBoxChanges(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var boxed = game.Trainer.PokemonBox.All[game.Trainer.Party.BoxIndexOf(0)!.Value];
        var level = boxed.Level == 50 ? 51 : 50;

        boxed.ChangeLevel(level);
        game.Trainer.PokemonBox.Commit();

        game.Trainer.Party.Pokemons[0].Level.Should().Be(level);
        game.SaveAndReload(reloaded => reloaded.Trainer.Party.Pokemons[0].Level.Should().Be(level));
    }

    [Theory]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    [InlineData(SaveFilePath.LetsGoEevee)]
    public void LetsGoBoxChangesToAPartyMemberSurviveASaveExport(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var boxed = game.Trainer.PokemonBox.All[game.Trainer.Party.BoxIndexOf(0)!.Value];
        var level = boxed.Level == 50 ? 51 : 50;

        boxed.ChangeLevel(level);

        game.SaveAndReload(reloaded => reloaded.Trainer.Party.Pokemons[0].Level.Should().Be(level));
    }

    [Fact]
    public void OwnerTidShouldBeSixteenBitBeforeGen7()
    {
        var game = Game.LoadFrom(SaveFilePath.HgSs);
        game.Trainer.Party.Pokemons.Should().AllSatisfy(p =>
            p.Owner.TID.Should().Be(p.Pkm.TID16));
    }

    [Theory]
    [SupportedSaveFiles]
    public void UpdatingAPartyPokemonRecalculatesItsStats(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var saved = game.Trainer.Party.Pokemons[0];
        var edited = saved.Clone();
        edited.ChangeLevel(saved.Level == 100 ? 99 : saved.Level + 1);

        game.Trainer.AddOrUpdate(saved.UniqueId, edited, Pokemons.PokemonSource.Party);

        var expected = edited.Pkm.Clone();
        expected.ResetPartyStats();
        game.Trainer.Party.Pokemons[0].Pkm.Stat_HPMax.Should().Be(expected.Stat_HPMax).And.NotBe(saved.Pkm.Stat_HPMax);
    }
}
