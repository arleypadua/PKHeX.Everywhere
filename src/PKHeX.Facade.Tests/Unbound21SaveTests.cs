using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class Unbound21SaveTests
{
    private static Game Load() => SaveFilePath.Load(SaveFilePath.Unbound21);

    [Fact]
    public void LoadsAsUnbound()
    {
        var game = Load();
        game.Format!.Id.Should().Be("unbound");
        game.SaveFile.ChecksumsValid.Should().BeTrue();
    }

    [Fact]
    public void ReadsTheTrainer()
    {
        var game = Load();
        game.Trainer.Name.Should().Be("Rick");
        game.Trainer.Money.Amount.Should().Be(1_258_126);
    }

    [Fact]
    public void ReadsTheParty() =>
        Load().Trainer.Party.Pokemons.Select(pokemon => (pokemon.Species.Name, pokemon.Level)).Should().Equal(
            ("Samurott", 100), ("Gengar", 100), ("Comfey", 100), ("Garchomp", 94), ("Infernape", 100), ("Staraptor", 100));

    [Fact]
    public void ReadsHowManyPokemonEachBoxHolds()
    {
        var box = Load().Trainer.PokemonBox;
        var slots = box.Boxes[0].Slots;
        var perBox = box.Boxed().CountBy(boxed => boxed.Index / slots).ToDictionary();

        Enumerable.Range(0, 25).Select(number => perBox.GetValueOrDefault(number)).Should().Equal(
            30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 12, 28, 27, 29, 27, 18, 0, 0, 3, 21, 2, 11, 7, 14, 12);
    }

    [Fact]
    public void ReadsEveryPokemonAsAKnownSpecies() =>
        Load().Trainer.PokemonBox.Boxed().Should().AllSatisfy(boxed => boxed.Pokemon.IsUnknown.Should().BeFalse());

    [Theory]
    [InlineData(nameof(InventoryType.Items), 283)]
    [InlineData(nameof(InventoryType.Balls), 17)]
    [InlineData(nameof(InventoryType.TMHMs), 128)]
    [InlineData(nameof(InventoryType.Berries), 53)]
    public void ReadsHowManyKindsOfItemEachPocketHolds(string pocket, int kinds) =>
        Load().Trainer.Inventories[pocket].AllExceptNone().Should().HaveCount(kinds);

    [Fact]
    public void ReadsTheItemsItsTablesLeaveUnmappedAsUnknown()
    {
        var game = Load();
        var bag = game.Trainer.Inventories.InventoryItems.Values.SelectMany(pocket => pocket.Items).Where(item => item.IsUnknown);
        var held = game.Trainer.PokemonBox.Boxed().Select(boxed => boxed.Pokemon.HeldItem).Where(item => item.IsUnknown);

        bag.Select(item => item.Index).Should().BeEquivalentTo([73, 79, 89, 174, 444, 615]);
        held.Select(item => item.Index).Should().Equal(728);
    }
}
