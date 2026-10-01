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

public class PokemonTrainerPatchTests
{
    [Theory]
    [SupportedSaveFiles]
    public void AppliesTheOriginalTrainer(string saveFile)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];

        pokemon.Update(new PokemonPatch(TrainerId: 12345, OriginalTrainerName: "Red", OriginalTrainerGender: Gender.Male));

        pokemon.Details().Should().BeEquivalentTo(new { TrainerId = 12345u, OriginalTrainerName = "Red", OriginalTrainerGender = Gender.Male });
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.HgSs)]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    public void AppliesTheSecretIdAndAFemaleOriginalTrainer(string saveFile)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];

        pokemon.Update(new PokemonPatch(SecretId: 1234, OriginalTrainerGender: Gender.Female));

        pokemon.Details().Should().BeEquivalentTo(new { SecretId = 1234u, OriginalTrainerGender = Gender.Female });
    }

    [Fact]
    public void AppliesTheHandlingTrainer()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.LetsGoPikachu).Trainer.Party.Pokemons[0];

        pokemon.Update(new PokemonPatch(
            HandlingTrainerName: "Blue",
            HandlingTrainerGender: Gender.Female,
            CurrentHandler: Owner.Handler.SomeoneElse));

        pokemon.Details().Should().BeEquivalentTo(new
        {
            HandlingTrainerName = "Blue",
            HandlingTrainerGender = Gender.Female,
            CurrentHandler = Owner.Handler.SomeoneElse,
        });
    }

    [Fact]
    public void AppliesATrainerIdThatOnlyFitsWithTheNewSecretId()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.LetsGoPikachu).Trainer.Party.Pokemons[0];
        pokemon.Update(new PokemonPatch(TrainerId: 0, SecretId: 4294));

        pokemon.Update(new PokemonPatch(TrainerId: 999999, SecretId: 0));

        pokemon.Details().Should().BeEquivalentTo(new { TrainerId = 999999u, SecretId = 0u });
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald, 70000u)]
    [InlineData(SaveFilePath.LetsGoPikachu, 1000000u)]
    public void RejectsATrainerIdTheSaveCantStore(string saveFile, uint trainerId)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];

        var update = () => pokemon.Update(new PokemonPatch(TrainerId: trainerId));

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(nameof(PokemonPatch.TrainerId));
    }

    public static TheoryData<string, PokemonPatch, string> Unrepresentable() => new()
    {
        { SaveFilePath.Yellow, new PokemonPatch(SecretId: 1), nameof(PokemonPatch.SecretId) },
        { SaveFilePath.Yellow, new PokemonPatch(OriginalTrainerGender: Gender.Female), nameof(PokemonPatch.OriginalTrainerGender) },
        { SaveFilePath.HgSs, new PokemonPatch(OriginalTrainerGender: Gender.Genderless), nameof(PokemonPatch.OriginalTrainerGender) },
        { SaveFilePath.Emerald, new PokemonPatch(OriginalTrainerName: "Sparkysparky"), nameof(PokemonPatch.OriginalTrainerName) },
        { SaveFilePath.Emerald, new PokemonPatch(OriginalTrainerName: "Red✨"), nameof(PokemonPatch.OriginalTrainerName) },
        { SaveFilePath.Emerald, new PokemonPatch(HandlingTrainerName: "Blue"), nameof(PokemonPatch.HandlingTrainerName) },
        { SaveFilePath.Emerald, new PokemonPatch(HandlingTrainerGender: Gender.Female), nameof(PokemonPatch.HandlingTrainerGender) },
        { SaveFilePath.Emerald, new PokemonPatch(CurrentHandler: Owner.Handler.SomeoneElse), nameof(PokemonPatch.CurrentHandler) },
    };

    [Theory]
    [MemberData(nameof(Unrepresentable))]
    public void RejectsTrainerDetailsTheSaveCantStore(string saveFile, PokemonPatch patch, string field)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];

        var update = () => pokemon.Update(patch);

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(field);
    }

    [Fact]
    public void AnInvalidTrainerFieldLeavesTheWholePatchUnapplied()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.Emerald).Trainer.Party.Pokemons[0];
        var before = pokemon.Details();

        var update = () => pokemon.Update(new PokemonPatch(OriginalTrainerName: "Red", TrainerId: 70000));

        update.Should().Throw<InvalidPatchException>();
        pokemon.Details().Should().BeEquivalentTo(before);
    }

    [Fact]
    public void AppliesAnIllegalOriginalTrainerAndReportsIt()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons.First(p => p.Details().Legality.Valid);

        pokemon.Update(new PokemonPatch(OriginalTrainerName: ""));

        pokemon.Details().OriginalTrainerName.Should().BeEmpty();
        pokemon.Details().Legality.Valid.Should().BeFalse();
    }
}
