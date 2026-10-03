using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Cfru;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class RomHackSpeciesTests
{
    private const int LoveAlcremie = (22 * 30) + 18;
    private const int FemaleIndeedee = (22 * 30) + 25;
    private const int FirstOfBox24 = 24 * 30;

    private const ushort CharizardGigantamax = 1261;
    private const ushort AntiqueSinistea = 1194;
    private const ushort ManaphyEgg = 252;
    private const ushort ToxtricityGigantamax = 1284;
    private const ushort LowKeyToxtricityGigantamax = 1285;

    private static Pokemon At(Game game, int index) => game.Trainer.PokemonBox.All[index];

    private static ushort SpeciesIndex(Pokemon pokemon) => ((CfruPokemon)pokemon.Pkm).SpeciesIndex;

    private static Game UnboundWith(ushort speciesIndex)
    {
        var save = new UnboundSave(File.ReadAllBytes(SaveFilePath.Unbound));
        var pokemon = (CfruPokemon)save.GetBoxSlotAtIndex(FirstOfBox24);
        pokemon.SpeciesIndex = speciesIndex;
        save.SetBoxSlotAtIndex(pokemon, FirstOfBox24);
        return Game.LoadFrom(save.Write().ToArray(), SaveFilePath.Unbound);
    }

    private static void EditAndReload(Game game, int index, Action<Pokemon> check)
    {
        var pokemon = At(game, index);
        var speciesIndex = SpeciesIndex(pokemon);
        check(pokemon);

        pokemon.Update(new PokemonPatch(Nickname: "Edited", Level: 60));
        check(pokemon);

        game.SaveAndReload(reloaded =>
        {
            var exported = At(reloaded, index);
            exported.Nickname.Should().Be("Edited");
            exported.Level.Should().Be(60);
            SpeciesIndex(exported).Should().Be(speciesIndex);
            check(exported);
        });
    }

    [Fact]
    public void AnAlcremieWithASweetShowsAsAlcremieAndKeepsTheSweetThroughAnEdit() =>
        EditAndReload(SaveFilePath.Load(SaveFilePath.Unbound), LoveAlcremie, pokemon =>
        {
            pokemon.IsUnknown.Should().BeFalse();
            pokemon.Species.Species.Should().Be(Species.Alcremie);
            pokemon.Pkm.Form.Should().Be(0);
            pokemon.Pkm.Should().BeAssignableTo<IFormArgument>()
                .Which.FormArgument.Should().Be((uint)AlcremieDecoration.Love);
        });

    [Fact]
    public void AFemaleIndeedeeShowsAsIndeedeesFemaleFormAndKeepsItThroughAnEdit() =>
        EditAndReload(SaveFilePath.Load(SaveFilePath.Unbound), FemaleIndeedee, pokemon =>
        {
            pokemon.IsUnknown.Should().BeFalse();
            pokemon.Species.Species.Should().Be(Species.Indeedee);
            pokemon.Pkm.Form.Should().Be(1);
            pokemon.Gender.Should().Be(Gender.Female);
        });

    [Fact]
    public void AGigantamaxPokemonKeepsTheFlagThroughAnEdit() =>
        EditAndReload(UnboundWith(CharizardGigantamax), FirstOfBox24, pokemon =>
        {
            pokemon.Species.Species.Should().Be(Species.Charizard);
            pokemon.Pkm.Form.Should().Be(0);
            pokemon.Pkm.Should().BeAssignableTo<IGigantamax>().Which.CanGigantamax.Should().BeTrue();
        });

    [Fact]
    public void AnAntiqueSinisteaKeepsItsFormThroughAnEdit() =>
        EditAndReload(UnboundWith(AntiqueSinistea), FirstOfBox24, pokemon =>
        {
            pokemon.Species.Species.Should().Be(Species.Sinistea);
            pokemon.Pkm.Form.Should().Be(1);
        });

    [Fact]
    public void TheManaphyEggStaysAnEggThroughAnEdit() =>
        EditAndReload(UnboundWith(ManaphyEgg), FirstOfBox24, pokemon =>
        {
            pokemon.Species.Species.Should().Be(Species.Manaphy);
            pokemon.Pkm.IsEgg.Should().BeTrue();
        });

    [Fact]
    public void HatchingTheManaphyEggMakesItAManaphy()
    {
        var game = UnboundWith(ManaphyEgg);

        At(game, FirstOfBox24).Update(new PokemonPatch(IsEgg: false));

        game.SaveAndReload(reloaded =>
        {
            var hatched = At(reloaded, FirstOfBox24);
            hatched.Species.Species.Should().Be(Species.Manaphy);
            hatched.Pkm.IsEgg.Should().BeFalse();
        });
    }

    [Fact]
    public void MakingAManaphyAnEggKeepsItsIndex()
    {
        var game = UnboundWith(ManaphyEgg);
        At(game, FirstOfBox24).Update(new PokemonPatch(IsEgg: false));
        var manaphy = SpeciesIndex(At(game, FirstOfBox24));

        At(game, FirstOfBox24).Update(new PokemonPatch(IsEgg: true));

        var egg = At(game, FirstOfBox24);
        SpeciesIndex(egg).Should().Be(manaphy);
        egg.Pkm.IsEgg.Should().BeTrue();
    }

    [Fact]
    public void ChangingTheFormOfAGigantamaxPokemonKeepsTheFlag()
    {
        var game = UnboundWith(ToxtricityGigantamax);

        At(game, FirstOfBox24).Update(new PokemonPatch(Form: 1));

        var lowKey = At(game, FirstOfBox24);
        SpeciesIndex(lowKey).Should().Be(LowKeyToxtricityGigantamax);
        ((IGigantamax)lowKey.Pkm).CanGigantamax.Should().BeTrue();
    }

    [Fact]
    public void ChangingTheSpeciesOfAGigantamaxPokemonWithoutAGigantamaxFormDropsTheFlag()
    {
        var game = UnboundWith(CharizardGigantamax);

        At(game, FirstOfBox24).Update(new PokemonPatch(Species: (int)Species.Charmeleon));

        var charmeleon = At(game, FirstOfBox24);
        charmeleon.Species.Species.Should().Be(Species.Charmeleon);
        ((IGigantamax)charmeleon.Pkm).CanGigantamax.Should().BeFalse();
    }

    private const int ShadowWarrior = (22 * 30) + 18;

    [Fact]
    public void AShadowWarriorIsUnknownAndNamedByTheSave()
    {
        var pokemon = At(SaveFilePath.Load(SaveFilePath.UnboundUnknownSpecies), ShadowWarrior);

        pokemon.IsEmpty.Should().BeFalse();
        pokemon.IsUnknown.Should().BeTrue();
        pokemon.IsEditable.Should().BeFalse();
        pokemon.Species.Name.Should().Be("Unknown (#706)");
        pokemon.Details().IsUnknown.Should().BeTrue();
        pokemon.Details().IsEditable.Should().BeFalse();
    }

    [Fact]
    public void AKnownSpeciesIsEditable()
    {
        var pokemon = At(SaveFilePath.Load(SaveFilePath.UnboundUnknownSpecies), FirstOfBox24);

        pokemon.IsUnknown.Should().BeFalse();
        pokemon.IsEditable.Should().BeTrue();
        pokemon.Details().IsEditable.Should().BeTrue();
    }

    [Fact]
    public void AShadowWarriorCantBeEditedOrCopiedAndKeepsItsBytes()
    {
        var game = SaveFilePath.Load(SaveFilePath.UnboundUnknownSpecies);
        var pokemon = At(game, ShadowWarrior);
        var bytes = pokemon.Pkm.Data.ToArray();

        pokemon.Invoking(p => p.Update(new PokemonPatch(Nickname: "Renamed"))).Should().Throw<UnknownSpeciesException>();
        pokemon.Invoking(p => p.MakeCopy()).Should().Throw<UnknownSpeciesException>();

        game.SaveAndReload(reloaded => At(reloaded, ShadowWarrior).Pkm.Data.ToArray().Should().Equal(bytes));
    }
}
