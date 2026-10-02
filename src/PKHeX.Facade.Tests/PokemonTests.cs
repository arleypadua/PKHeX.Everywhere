using AwesomeAssertions;
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

    [Theory]
    [Games(GameVersion.E, GameVersion.HGSS, GameVersion.C)]
    public void HeldItem_ShouldResolveFromTheGameItems(Game game)
    {
        var leftovers = game.ItemRepository.GetGameItemByName("Leftovers")!;
        var pokemon = game.Trainer.Party.Pokemons.First();

        pokemon.HeldItem = leftovers;

        pokemon.HeldItem.Should().Be(leftovers);
    }

    [Theory]
    [InlineData(Ball.Cherish, "Cherish Ball")]
    [InlineData(Ball.Fast, "Fast Ball")]
    [InlineData(Ball.Dream, "Dream Ball")]
    [InlineData(Ball.LAOrigin, "Origin Ball")]
    public void Ball_ShouldResolveFromTheBallNames(Ball ball, string name)
    {
        var game = Game.EmptyOf(GameVersionRepository.Instance.Get(GameVersion.PLA));
        var pokemon = new Pokemon(new PA8 { Ball = (byte)ball }, game);

        pokemon.Ball.Should().Be(new ItemDefinition((ushort)ball, name));
    }

    [Fact]
    public void IsAlpha_ShouldSurviveSerialization()
    {
        var game = Game.EmptyOf(GameVersionRepository.Instance.Get(GameVersion.PLA));
        var pokemon = new Pokemon(new PA8(), game);

        pokemon.SupportsAlpha.Should().BeTrue();
        pokemon.IsAlpha = true;

        Pokemon.LoadFrom(pokemon.ToFile().Bytes, game).IsAlpha.Should().BeTrue();
    }

    [Fact]
    public void SupportsAlpha_ShouldBeFalseForGamesWithoutAlphas()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.Emerald).Trainer.Party.Pokemons.First();

        pokemon.SupportsAlpha.Should().BeFalse();
        pokemon.IsAlpha.Should().BeFalse();
        pokemon.Invoking(p => p.IsAlpha = true).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UniqueId_ShouldBeDistinctForPokemonWithoutPid()
    {
        var game = Game.LoadFrom(SaveFilePath.Yellow);

        var ids = game.Trainer.Party.Pokemons
            .Concat(game.Trainer.PokemonBox.All)
            .Where(p => p.Species != SpeciesDefinition.None)
            .Select(p => p.UniqueId.Value)
            .ToList();

        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void HiddenPower_ShouldResolveTypeAndPowerFromIvs()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.Emerald).Trainer.Party.Pokemons.First();

        pokemon.HiddenPower.Should().Be(new HiddenPowerDefinition(
            GameInfo.Strings.types[pokemon.Pkm.HPType + 1],
            pokemon.Pkm.HPPower));
    }

    [Fact]
    public void HiddenPower_ShouldFollowIvChanges()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.Emerald).Trainer.Party.Pokemons.First();
        var before = pokemon.HiddenPower!.Type;

        pokemon.Pkm.HPType = 1;

        pokemon.HiddenPower!.Type.Should().NotBe(before);
    }

    [Fact]
    public void HiddenPower_ShouldBeNullForGamesWithoutIt()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.LetsGoPikachu).Trainer.Party.Pokemons.First();

        pokemon.HiddenPower.Should().BeNull();
    }

    private Game AGame(GameVersion version, string trainerName) =>
        Game.EmptyOf(GameVersionRepository.Instance.Get(version), trainerName);

    [Fact]
    public void MakeCopy_ShouldRerollPidAndClearNickname()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons[0];
        pokemon.Pkm.SetNickname("Sparky");

        var copy = pokemon.MakeCopy();

        copy.PID.Should().NotBe(pokemon.PID);
        copy.Species.Should().Be(pokemon.Species);
        copy.Pkm.IsNicknamed.Should().BeFalse();
        copy.IsShiny.Should().Be(pokemon.IsShiny);
    }

    [Fact]
    public void MakeCopy_ShouldKeepAShinyPokemonShiny()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons[0];
        pokemon.Pkm.SetIsShiny(true);

        pokemon.MakeCopy().IsShiny.Should().BeTrue();
    }
}
