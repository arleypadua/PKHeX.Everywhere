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
    private const int Rewritten = 24 * 30;

    private const ushort CharizardGigantamax = 1261;
    private const ushort AntiqueSinistea = 1194;
    private const ushort ManaphyEgg = 252;

    private static Pokemon At(Game game, int index) => game.Trainer.PokemonBox.All[index];

    private static ushort SpeciesIndex(Pokemon pokemon) => ((CfruPokemon)pokemon.Pkm).SpeciesIndex;

    private static Game UnboundWith(ushort speciesIndex)
    {
        var save = new UnboundSave(File.ReadAllBytes(SaveFilePath.Unbound));
        var pokemon = (CfruPokemon)save.GetBoxSlotAtIndex(Rewritten);
        pokemon.SpeciesIndex = speciesIndex;
        save.SetBoxSlotAtIndex(pokemon, Rewritten);
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
        EditAndReload(UnboundWith(CharizardGigantamax), Rewritten, pokemon =>
        {
            pokemon.Species.Species.Should().Be(Species.Charizard);
            pokemon.Pkm.Form.Should().Be(0);
            pokemon.Pkm.Should().BeAssignableTo<IGigantamax>().Which.CanGigantamax.Should().BeTrue();
        });

    [Fact]
    public void AnAntiqueSinisteaKeepsItsFormThroughAnEdit() =>
        EditAndReload(UnboundWith(AntiqueSinistea), Rewritten, pokemon =>
        {
            pokemon.Species.Species.Should().Be(Species.Sinistea);
            pokemon.Pkm.Form.Should().Be(1);
        });

    [Fact]
    public void TheManaphyEggStaysAnEggThroughAnEdit() =>
        EditAndReload(UnboundWith(ManaphyEgg), Rewritten, pokemon =>
        {
            pokemon.Species.Species.Should().Be(Species.Manaphy);
            pokemon.Pkm.IsEgg.Should().BeTrue();
        });

    [Fact]
    public void HatchingTheManaphyEggMakesItAManaphy()
    {
        var game = UnboundWith(ManaphyEgg);

        At(game, Rewritten).Update(new PokemonPatch(IsEgg: false));

        game.SaveAndReload(reloaded =>
        {
            var hatched = At(reloaded, Rewritten);
            hatched.Species.Species.Should().Be(Species.Manaphy);
            hatched.Pkm.IsEgg.Should().BeFalse();
        });
    }

    [Fact]
    public void ChangingTheFormOfAGigantamaxPokemonKeepsTheFlag()
    {
        var game = UnboundWith(1284); // Toxtricity, Gigantamax

        At(game, Rewritten).Update(new PokemonPatch(Form: 1));

        var lowKey = At(game, Rewritten);
        SpeciesIndex(lowKey).Should().Be(1285);
        ((IGigantamax)lowKey.Pkm).CanGigantamax.Should().BeTrue();
    }

    [Fact]
    public void ChangingTheSpeciesOfAGigantamaxPokemonWithoutAGigantamaxFormDropsTheFlag()
    {
        var game = UnboundWith(CharizardGigantamax);

        At(game, Rewritten).Update(new PokemonPatch(Species: (int)Species.Charmeleon));

        var charmeleon = At(game, Rewritten);
        charmeleon.Species.Species.Should().Be(Species.Charmeleon);
        ((IGigantamax)charmeleon.Pkm).CanGigantamax.Should().BeFalse();
    }
}
