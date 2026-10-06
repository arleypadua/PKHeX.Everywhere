using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Cfru;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonSpeciesIndexTests
{
    private const int ShadowWarrior = (22 * 30) + 18;
    private const ushort ShadowWarriorIndex = 706;
    private const int FirstOfBox24 = 24 * 30;
    private const ushort UnboundEggSlot = 412;

    [Fact]
    public void AGen3HoennSpeciesHasItsInternalIndex()
    {
        var pokemon = SaveFilePath.Load(SaveFilePath.Emerald).Trainer.Party.Pokemons[0];
        pokemon.Pkm.Species = (ushort)Species.Treecko;

        ((PK3)pokemon.Pkm).SpeciesInternal.Should().Be(277);
        pokemon.SpeciesIndex.Should().Be(277);
        pokemon.Details().SpeciesIndex.Should().Be(277);
    }

    [Fact]
    public void AGen1SpeciesHasItsInternalIndex()
    {
        var pokemon = SaveFilePath.Load(SaveFilePath.Yellow).Trainer.Party.Pokemons[0];
        pokemon.Pkm.Species = (ushort)Species.Pikachu;

        pokemon.SpeciesIndex.Should().Be(0x54);
    }

    [Fact]
    public void AGen4SpeciesIndexIsItsNationalNumber()
    {
        var pokemon = SaveFilePath.Load(SaveFilePath.HgSs).Trainer.Party.Pokemons[0];

        pokemon.SpeciesIndex.Should().Be(pokemon.Pkm.Species);
    }

    [Fact]
    public void AGen9SpeciesHasItsInternalIndex()
    {
        var pokemon = new Pokemon(new PK9 { Species = (ushort)Species.Tarountula }, null!);

        pokemon.SpeciesIndex.Should().Be(SpeciesConverter.GetInternal9((ushort)Species.Tarountula)).And.NotBe((int)Species.Tarountula);
    }

    [Fact]
    public void AShadowWarriorHasItsHackIndex()
    {
        var pokemon = SaveFilePath.Load(SaveFilePath.UnboundUnknownSpecies).Trainer.PokemonBox.All[ShadowWarrior];

        pokemon.SpeciesIndex.Should().Be(ShadowWarriorIndex);
        pokemon.Details().SpeciesIndex.Should().Be(ShadowWarriorIndex);
    }

    [Fact]
    public void AnUnknownSpeciesHasItsHackIndex()
    {
        var save = new UnboundSave(File.ReadAllBytes(SaveFilePath.Unbound));
        var slot = (CfruPokemon)save.GetBoxSlotAtIndex(FirstOfBox24);
        slot.SpeciesIndex = UnboundEggSlot;
        save.SetBoxSlotAtIndex(slot, FirstOfBox24);
        var pokemon = Game.LoadFrom(save.Write().ToArray(), SaveFilePath.Unbound).Trainer.PokemonBox.All[FirstOfBox24];

        pokemon.IsUnknown.Should().BeTrue();
        pokemon.SpeciesIndex.Should().Be(UnboundEggSlot);
        pokemon.Details().SpeciesIndex.Should().Be(UnboundEggSlot);
    }
}
