using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonDescriptionPatchTests
{
    [Theory]
    [InlineData(SaveFilePath.HgSs)]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    public void AppliesTheDescriptionFields(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        var pokemon = game.Trainer.Party.Pokemons[0];
        var before = pokemon.Details();
        var heldItem = game.Options.HeldItems.Last().Id;
        var ball = game.Options.Balls.First(b => b.Id != before.Ball).Id;
        var language = game.Options.Languages.First(l => l.Id != before.Language && l.Id != 0).Id;
        var friendship = before.Friendship == 70 ? 71 : 70;

        pokemon.Update(new PokemonPatch(HeldItem: heldItem, Ball: ball, Language: language, Friendship: friendship, IsShiny: !before.IsShiny));

        pokemon.Details().Should().BeEquivalentTo(new
        {
            HeldItem = heldItem,
            Ball = ball,
            Language = language,
            Friendship = friendship,
            IsShiny = !before.IsShiny,
        });
    }

    [Fact]
    public void AppliesSpeciesBeforeFormBeforeAbility()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.LetsGoPikachu).Trainer.Party.Pokemons
            .First(p => p.Species.Species != Species.Vulpix && !p.Form.HasForm);

        pokemon.Update(new PokemonPatch(Species: (int)Species.Vulpix, Form: 1, Ability: (int)Ability.SnowCloak));

        pokemon.Details().Should().BeEquivalentTo(new { Species = (int)Species.Vulpix, Form = 1, Ability = (int)Ability.SnowCloak });
    }

    [Fact]
    public void AppliesTheNatureWhereTheSaveStoresIt()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.LetsGoPikachu).Trainer.Party.Pokemons[0];
        var nature = pokemon.Pkm.Nature == Nature.Adamant ? Nature.Bold : Nature.Adamant;

        pokemon.Update(new PokemonPatch(Nature: (int)nature));

        pokemon.Details().Nature.Should().Be((int)nature);
        pokemon.Pkm.StatAlignment.Should().Be(nature);
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.HgSs)]
    [InlineData(SaveFilePath.Crystal)]
    public void RejectsANatureTheSaveDerivesFromThePid(string saveFile)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];
        var nature = pokemon.Pkm.Nature == Nature.Adamant ? Nature.Bold : Nature.Adamant;

        var update = () => pokemon.Update(new PokemonPatch(Nature: (int)nature));

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(nameof(PokemonPatch.Nature));
    }

    [Fact]
    public void ChangingTheSpeciesRenamesAPokemonWithoutANickname()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons.First(p => !p.NicknameSet);
        var evolution = pokemon.Options().Species.First(s => s.Id != pokemon.Species.Id);

        pokemon.Update(new PokemonPatch(Species: evolution.Id));

        pokemon.Nickname.Should().Be(evolution.Name.ToUpperInvariant());
    }

    [Fact]
    public void ChangingTheSpeciesKeepsANickname()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons[0];
        var evolution = pokemon.Options().Species.First(s => s.Id != pokemon.Species.Id);

        pokemon.Update(new PokemonPatch(Species: evolution.Id, Nickname: "Sparky"));
        pokemon.Update(new PokemonPatch(Species: pokemon.Options().Species.First(s => s.Id != evolution.Id).Id));

        pokemon.Nickname.Should().Be("Sparky");
    }

    [Fact]
    public void AppliesTheGenderAfterTheSpecies()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.Emerald).Trainer.Party.Pokemons[0];

        var update = () => pokemon.Update(new PokemonPatch(Species: (int)Species.Tauros, Gender: Gender.Female));

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(nameof(PokemonPatch.Gender));
    }

    [Theory]
    [SupportedSaveFiles]
    public void AppliesAGenderTheSpeciesCanHave(string saveFile)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons.First(p => p.Pkm.PersonalInfo.IsDualGender);
        var gender = pokemon.Gender == Gender.Male ? Gender.Female : Gender.Male;

        pokemon.Update(new PokemonPatch(Gender: gender));

        pokemon.Details().Gender.Should().Be(gender);
    }

    public static TheoryData<string, PokemonPatch, string> UnstorableValues() => new()
    {
        { SaveFilePath.Crystal, new PokemonPatch(Species: 252), nameof(PokemonPatch.Species) },
        { SaveFilePath.Emerald, new PokemonPatch(Species: 0), nameof(PokemonPatch.Species) },
        { SaveFilePath.Emerald, new PokemonPatch(Form: 30), nameof(PokemonPatch.Form) },
        { SaveFilePath.Emerald, new PokemonPatch(HeldItem: 5000), nameof(PokemonPatch.HeldItem) },
        { SaveFilePath.Emerald, new PokemonPatch(Ball: 20), nameof(PokemonPatch.Ball) },
        { SaveFilePath.Crystal, new PokemonPatch(Ball: 4), nameof(PokemonPatch.Ball) },
        { SaveFilePath.Emerald, new PokemonPatch(Language: 8), nameof(PokemonPatch.Language) },
        { SaveFilePath.Crystal, new PokemonPatch(Language: 2), nameof(PokemonPatch.Language) },
        { SaveFilePath.Emerald, new PokemonPatch(Ability: 300), nameof(PokemonPatch.Ability) },
        { SaveFilePath.Crystal, new PokemonPatch(Ability: 1), nameof(PokemonPatch.Ability) },
        { SaveFilePath.HgSs, new PokemonPatch(Friendship: 256), nameof(PokemonPatch.Friendship) },
        { SaveFilePath.HgSs, new PokemonPatch(Friendship: -1), nameof(PokemonPatch.Friendship) },
        { SaveFilePath.HgSs, new PokemonPatch(Gender: Gender.Unknown), nameof(PokemonPatch.Gender) },
        { SaveFilePath.HgSs, new PokemonPatch(IsAlpha: true), nameof(PokemonPatch.IsAlpha) },
    };

    [Theory]
    [MemberData(nameof(UnstorableValues))]
    public void RejectsAValueTheSaveCantStore(string saveFile, PokemonPatch patch, string field)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];
        var before = pokemon.Pkm.Data.ToArray();

        var update = () => pokemon.Update(patch with { Nickname = "Sparky" });

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(field);
        pokemon.Pkm.Data.ToArray().Should().Equal(before);
    }

    [Fact]
    public void AppliesAnAbilityTheSpeciesCantHaveAndReportsIt()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons[0];
        var foreign = Enumerable.Range(1, 100).First(id => pokemon.Pkm.PersonalInfo.GetIndexOfAbility(id) < 0);

        pokemon.Update(new PokemonPatch(Ability: foreign));

        pokemon.Details().Ability.Should().Be(foreign);
        pokemon.Details().Legality!.Valid.Should().BeFalse();
    }

    [Fact]
    public void SetsTheAlphaFlagWhereTheGameHasAlphaPokemon()
    {
        var game = Game.EmptyOf(GameVersionRepository.Instance.Get(GameVersion.PLA));
        var pokemon = new Pokemon(new PA8 { Species = (ushort)Species.Pikachu }, game);
        pokemon.Details().IsAlpha.Should().BeFalse();

        pokemon.Update(new PokemonPatch(IsAlpha: true));

        pokemon.Details().IsAlpha.Should().BeTrue();
    }

    [Fact]
    public void ReportsNoAlphaFlagWhereTheGameHasNoAlphaPokemon() =>
        Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons[0].Details().IsAlpha.Should().BeNull();

    [Fact]
    public void TurnsAPokemonIntoAnEgg()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons[0];

        pokemon.Update(new PokemonPatch(IsEgg: true, Friendship: 10));

        pokemon.Details().Should().BeEquivalentTo(new { IsEgg = true, Friendship = 10 });
    }

    [Theory]
    [SupportedSaveFiles]
    public void OffersTheEvolutionLineAndTheSpeciesAbilities(string saveFile)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];

        var options = pokemon.Options();

        options.Species.Should().Contain(new Choice(pokemon.Species.Id, pokemon.Species.Name));
        if (pokemon.Pkm.Format <= 2) options.Abilities.Should().BeEmpty();
        else options.Abilities[0].Id.Should().Be(pokemon.Pkm.PersonalInfo.GetAbilityAtIndex(0));
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald, false)]
    [InlineData(SaveFilePath.HgSs, true)]
    [InlineData(SaveFilePath.LetsGoPikachu, true)]
    public void OffersEveryAbilityWhereTheSaveStoresAny(string saveFile, bool every)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];

        var abilities = pokemon.Options().Abilities;

        if (every) abilities.Should().HaveCount(pokemon.Pkm.MaxAbilityID);
        else abilities.Should().AllSatisfy(a => pokemon.Pkm.PersonalInfo.GetIndexOfAbility(a.Id).Should().BeGreaterThanOrEqualTo(0));
    }

    [Fact]
    public void OffersTheFormsOfTheSpecies()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.LetsGoPikachu).Trainer.Party.Pokemons.First(p => !p.Form.HasForm);
        pokemon.Options().Forms.Should().BeEmpty();

        pokemon.Update(new PokemonPatch(Species: (int)Species.Vulpix));

        pokemon.Options().Forms.Select(f => f.Name).Should().Contain("Alola");
    }
}
