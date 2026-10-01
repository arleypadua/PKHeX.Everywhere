using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonPatchTests
{
    [Theory]
    [SupportedSaveFiles]
    public void AppliesNicknameAndLevel(string saveFile)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];
        var level = pokemon.Level == 50 ? 51 : 50;

        pokemon.Update(new PokemonPatch(Nickname: "Sparky", Level: level));

        pokemon.Details().Should().BeEquivalentTo(new { Nickname = "Sparky", Level = level });
    }

    [Theory]
    [SupportedSaveFiles]
    public void LeavesFieldsMissingFromThePatchUnchanged(string saveFile)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];
        var nickname = pokemon.Nickname;

        pokemon.Update(new PokemonPatch(Level: 42));

        pokemon.Nickname.Should().Be(nickname);
        pokemon.Level.Should().Be(42);
    }

    [Fact]
    public void AnEmptyNicknameResetsItToTheSpeciesName()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons[0];
        pokemon.Update(new PokemonPatch(Nickname: "Sparky"));

        pokemon.Update(new PokemonPatch(Nickname: " "));

        pokemon.NicknameSet.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void RejectsALevelOutsideOneToHundred(int level)
    {
        var pokemon = Game.LoadFrom(SaveFilePath.Emerald).Trainer.Party.Pokemons[0];

        var update = () => pokemon.Update(new PokemonPatch(Level: level));

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(nameof(PokemonPatch.Level));
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald, "Sparkysparky")]
    [InlineData(SaveFilePath.Crystal, "Sparkysparky")]
    [InlineData(SaveFilePath.Emerald, "Spark✨")]
    public void RejectsANicknameTheSaveCantStore(string saveFile, string nickname)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];

        var update = () => pokemon.Update(new PokemonPatch(Nickname: nickname));

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(nameof(PokemonPatch.Nickname));
    }

    [Fact]
    public void AnInvalidFieldLeavesTheWholePatchUnapplied()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons[0];
        var nickname = pokemon.Nickname;

        var update = () => pokemon.Update(new PokemonPatch(Nickname: "Sparky", Level: 101));

        update.Should().Throw<InvalidPatchException>();
        pokemon.Nickname.Should().Be(nickname);
    }

    [Fact]
    public void AppliesAnIllegalLevelAndReportsIt()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons[0];
        pokemon.MetConditions.Level.Should().BeGreaterThan(1);

        pokemon.Update(new PokemonPatch(Level: 1));

        pokemon.Level.Should().Be(1);
        pokemon.Details().Legality.Valid.Should().BeFalse();
        pokemon.Details().Legality.Messages.Should().NotBeEmpty();
    }

    [Fact]
    public void ReportsNoMessagesForALegalPokemon()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons.First(p => p.Details().Legality.Valid);

        pokemon.Details().Legality.Messages.Should().BeEmpty();
    }
}
