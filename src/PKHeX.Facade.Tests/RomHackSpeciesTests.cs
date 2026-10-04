using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Cfru;
using PKHeX.Everywhere.RomHacks.Cfru.RadicalRed;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class RomHackSpeciesTests
{
    private const int LoveAlcremie = (22 * 30) + 18;
    private const int FemaleIndeedee = (22 * 30) + 25;
    private const int FirstOfBox24 = 24 * 30;
    private const int RadicalRedSlot = 1;

    private const ushort CharizardGigantamax = 1261;
    private const ushort AntiqueSinistea = 1194;
    private const ushort ManaphyEgg = 252;
    private const ushort ToxtricityGigantamax = 1284;
    private const ushort LowKeyToxtricityGigantamax = 1285;

    private static Pokemon At(Game game, int index) => game.Trainer.PokemonBox.All[index];

    private static ushort SpeciesIndex(Pokemon pokemon) => ((CfruPokemon)pokemon.Pkm).SpeciesIndex;

    private static Game UnboundWith(ushort speciesIndex) =>
        With(new UnboundSave(File.ReadAllBytes(SaveFilePath.Unbound)), SaveFilePath.Unbound, FirstOfBox24, speciesIndex);

    private static Game RadicalRedWith(ushort speciesIndex) =>
        With(new RadicalRedSave(File.ReadAllBytes(SaveFilePath.RadicalRed)), SaveFilePath.RadicalRed, RadicalRedSlot, speciesIndex);

    private static Game With(CfruSave save, string path, int slot, ushort speciesIndex)
    {
        var pokemon = (CfruPokemon)save.GetBoxSlotAtIndex(slot);
        pokemon.SpeciesIndex = speciesIndex;
        save.SetBoxSlotAtIndex(pokemon, slot);
        return Game.LoadFrom(save.Write().ToArray(), path, SaveFilePath.FormatOf(path));
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

    [Theory]
    [InlineData(492, Species.MimeJr, 0)]
    [InlineData(527, Species.PorygonZ, 0)]
    [InlineData(832, Species.Meowstic, 1)]
    [InlineData(849, Species.Oinkologne, 1)]
    [InlineData(989, Species.TypeNull, 0)]
    [InlineData(1157, Species.Sirfetchd, 0)]
    [InlineData(1158, Species.MrRime, 0)]
    [InlineData(1202, Species.Indeedee, 1)]
    [InlineData(1213, Species.Farfetchd, 1)]
    [InlineData(1216, Species.MrMime, 1)]
    [InlineData(1306, Species.Basculegion, 1)]
    [InlineData(818, Species.Pumpkaboo, 1)]
    [InlineData(853, Species.Pumpkaboo, 3)]
    [InlineData(855, Species.Pumpkaboo, 0)]
    [InlineData(1235, Species.Tauros, 3)]
    [InlineData(1240, Species.Tauros, 2)]
    [InlineData(1359, Species.Ogerpon, 2)]
    public void ARadicalRedPokemonReadsAsItsSpeciesAndFormAndKeepsItsIndexThroughAnEdit(int speciesIndex, Species species, int form) =>
        EditAndReload(RadicalRedWith((ushort)speciesIndex), RadicalRedSlot, pokemon =>
        {
            pokemon.IsUnknown.Should().BeFalse();
            pokemon.IsEditable.Should().BeTrue();
            pokemon.Species.Species.Should().Be(species);
            pokemon.Pkm.Form.Should().Be((byte)form);
        });

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
    private const ushort ShadowWarriorIndex = 706;
    private const ushort ZygardeCell = 835;
    private const ushort ZygardeCore = 836;
    private const ushort GalarianMimeJr = 1224;
    private const ushort Chillet = 1375;

    // The stat formula of Gen 3 onwards, with base stats in the order HP, Attack, Defense, Special Attack, Special Defense, Speed.
    private static StatValues StatsFrom(Pokemon pokemon, int[] baseStats)
    {
        var pkm = pokemon.Pkm;
        int Raw(int stat, int pkhexIndex) => ((2 * baseStats[stat]) + pkm.GetIV(pkhexIndex) + (pkm.GetEV(pkhexIndex) / 4)) * pkm.CurrentLevel / 100;
        int Other(int stat, int pkhexIndex)
        {
            var value = Raw(stat, pkhexIndex) + 5;
            var (raised, lowered, natureIndex) = ((int)pkm.Nature / 5, (int)pkm.Nature % 5, pkhexIndex - 1);
            return raised == lowered ? value : natureIndex == raised ? value * 11 / 10 : natureIndex == lowered ? value * 9 / 10 : value;
        }

        return new StatValues(Raw(0, 0) + pkm.CurrentLevel + 10, Other(1, 1), Other(2, 2), Other(3, 4), Other(4, 5), Other(5, 3));
    }

    [Fact]
    public void AShadowWarriorShowsByNameWithItsOwnTypesStatsAbilityAndGender()
    {
        var pokemon = At(SaveFilePath.Load(SaveFilePath.UnboundUnknownSpecies), ShadowWarrior);

        pokemon.Species.Name.Should().Be("Shadow Warrior");
        pokemon.Types.Tuple.Should().Be(((int)MoveType.Ghost, (int)MoveType.Dark));
        pokemon.Details().Stats.Should().Be(StatsFrom(pokemon, [60, 115, 85, 80, 85, 115]));
        ((Ability)pokemon.Pkm.Ability).Should().BeOneOf(Ability.ToughClaws, Ability.WonderGuard);
        pokemon.Gender.Should().Be(Gender.Genderless);
        pokemon.Options().Species.Should().Equal(new Choice(pokemon.Species.Id, "Shadow Warrior"));
    }

    [Fact]
    public void AShadowWarriorIsStillReadOnly()
    {
        var pokemon = At(SaveFilePath.Load(SaveFilePath.UnboundUnknownSpecies), ShadowWarrior);

        pokemon.IsEmpty.Should().BeFalse();
        pokemon.IsUnknown.Should().BeTrue();
        pokemon.IsEditable.Should().BeFalse();
        pokemon.Details().IsEditable.Should().BeFalse();
    }

    [Theory]
    [InlineData(ZygardeCell, "Zygarde Cell", 50)]
    [InlineData(ZygardeCore, "Zygarde Core", 75)]
    public void ZygardesCellAndCoreShowByNameWithTheirOwnData(ushort index, string name, int baseStat)
    {
        var pokemon = At(UnboundWith(index), FirstOfBox24);

        pokemon.Species.Name.Should().Be(name);
        pokemon.Types.Tuple.Should().Be(((int)MoveType.Dragon, (int)MoveType.Ground));
        pokemon.Details().Stats.Should().Be(StatsFrom(pokemon, Enumerable.Repeat(baseStat, 6).ToArray()));
        ((Ability)pokemon.Pkm.Ability).Should().Be(Ability.AuraBreak);
        pokemon.Gender.Should().Be(Gender.Genderless);
        pokemon.IsEditable.Should().BeFalse();
    }

    [Theory]
    [InlineData(0u, false)]
    [InlineData(1u, false)]
    [InlineData(0u, true)]
    public void AHackSpeciesWithOneAbilityHasItWhicheverSlotThePokemonUses(uint pid, bool hiddenAbility)
    {
        var save = new UnboundSave(File.ReadAllBytes(SaveFilePath.Unbound));
        var pokemon = (CfruPokemon)save.GetBoxSlotAtIndex(FirstOfBox24);
        pokemon.SpeciesIndex = ZygardeCell;
        pokemon.PID = pid;
        pokemon.HasHiddenAbility = hiddenAbility;

        ((Ability)pokemon.Ability).Should().Be(Ability.AuraBreak);
    }

    [Theory]
    [InlineData(Chillet, "Chillet")]
    [InlineData(GalarianMimeJr, "Galarian Mime Jr.")]
    public void ARadicalRedHackSpeciesShowsByNameAndStaysUnknown(ushort index, string name)
    {
        var game = RadicalRedWith(index);
        var pokemon = At(game, RadicalRedSlot);

        pokemon.Species.Name.Should().Be(name);
        pokemon.IsUnknown.Should().BeTrue();
        pokemon.IsEditable.Should().BeFalse();
        pokemon.Invoking(p => p.Update(new PokemonPatch(Nickname: "Renamed"))).Should().Throw<UnknownSpeciesException>();
        game.SpeciesRepository.AllGameSpecies.Select(species => species.Name).Should().NotContain(name);
        game.SaveAndReload(reloaded => SpeciesIndex(At(reloaded, RadicalRedSlot)).Should().Be(index));
    }

    [Fact]
    public void HackSpeciesWithDataAreInTheSpeciesListByName()
    {
        var species = SaveFilePath.Load(SaveFilePath.Unbound).SpeciesRepository.AllGameSpecies.Select(s => s.Name);

        species.Should().Contain(["Shadow Warrior", "Zygarde Cell", "Zygarde Core"]);
    }

    [Fact]
    public void APokemonTurnedIntoAHackSpeciesSavesItsIndexAndTurnsBack()
    {
        var game = SaveFilePath.Load(SaveFilePath.Unbound);
        var original = At(game, FirstOfBox24);
        var (originalIndex, originalSpecies) = (SpeciesIndex(original), original.Pkm.Species);
        var shadowWarrior = game.SpeciesRepository.AllGameSpecies.Single(species => species.Name == "Shadow Warrior");

        original.Update(new PokemonPatch(Species: shadowWarrior.Id));

        game.SaveAndReload(reloaded =>
        {
            var pokemon = At(reloaded, FirstOfBox24);
            SpeciesIndex(pokemon).Should().Be(ShadowWarriorIndex);
            pokemon.Species.Name.Should().Be("Shadow Warrior");

            // The Facade keeps a hack species read-only, so it turns back through the PKM.
            pokemon.Pkm.Species = originalSpecies;

            reloaded.SaveAndReload(back => SpeciesIndex(At(back, FirstOfBox24)).Should().Be(originalIndex));
        });
    }

    [Fact]
    public void ChangingTheFormGigantamaxOrFormArgumentOfAHackSpeciesKeepsItsIndex()
    {
        var pokemon = (CfruPokemon)At(UnboundWith(ShadowWarriorIndex), FirstOfBox24).Pkm;

        pokemon.Form = 1;
        pokemon.CanGigantamax = true;
        pokemon.FormArgument = 1;

        pokemon.SpeciesIndex.Should().Be(ShadowWarriorIndex);
    }

    [Theory]
    [InlineData(ShadowWarriorIndex, "Warrior", false)]
    [InlineData(ShadowWarriorIndex, "Shadow", true)]
    [InlineData(ZygardeCell, "Zygarde", false)]
    public void AnUnboundHackSpeciesIsNicknamedWhenItsNicknameDiffersFromTheNameTheGameStores(ushort index, string nickname, bool nicknamed)
    {
        var pokemon = new UnboundPokemon { SpeciesIndex = index, Language = (int)LanguageID.English, Nickname = nickname };

        pokemon.IsNicknamed.Should().Be(nicknamed);
    }

    [Theory]
    [InlineData("Chillet", false)]
    [InlineData("Chilly", true)]
    public void ARadicalRedHackSpeciesIsNicknamedWhenItsNicknameDiffersFromItsName(string nickname, bool nicknamed)
    {
        var pokemon = new RadicalRedPokemon { SpeciesIndex = Chillet, Language = (int)LanguageID.English, Nickname = nickname };

        pokemon.IsNicknamed.Should().Be(nicknamed);
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
