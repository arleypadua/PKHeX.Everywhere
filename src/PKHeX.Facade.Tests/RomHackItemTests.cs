using AwesomeAssertions;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade.Tests;

public class RomHackItemTests
{
    private const string UnknownItem = "Unknown item #79";
    private const int HoldsUnknownItem = (19 * 30) + 27;

    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.Unbound);

    // Unknown item #79's slot in each save slot of the fixture.
    private static readonly int[] UnknownItemSlots = [0x9B2C, 0x18B2C];
    private const ushort OutsideTheTable = 2000;

    private static Inventory Items(Game game) => game.Trainer.Inventories["Items"];

    private static Inventory.Item Unknown(Game game) => Items(game).Items.Single(item => item.Name == UnknownItem);

    private static int[] ChangedOffsets(Game game)
    {
        var exported = game.ToByteArray();
        return Enumerable.Range(0, exported.Length).Where(offset => exported[offset] != Fixture[offset]).ToArray();
    }

    [Fact]
    public void AnUnknownItemIsListedWithTheOtherItems()
    {
        var unknown = Unknown(SaveFilePath.Load(SaveFilePath.Unbound));

        unknown.IsUnknown.Should().BeTrue();
        unknown.Id.Should().NotBe((ushort)ItemDefinition.None);
        unknown.Count.Should().Be(92);
    }

    [Fact]
    public void UnknownItemsHaveDistinctIds()
    {
        var unknown = Items(SaveFilePath.Load(SaveFilePath.Unbound)).Items.Where(item => item.IsUnknown).ToArray();

        unknown.Should().HaveCount(8);
        unknown.Select(item => item.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AnUnknownItemsCountChangesAndSurvivesExport()
    {
        var game = SaveFilePath.Load(SaveFilePath.Unbound);

        Items(game).TrySet(Unknown(game).Id, 5).Should().BeTrue();

        game.SaveAndReload(reloaded => Unknown(reloaded).Count.Should().Be(5));
    }

    [Fact]
    public void ChangingAnUnknownItemsCountKeepsTheOtherSlotsRawBytes()
    {
        var game = SaveFilePath.Load(SaveFilePath.Unbound);

        Items(game).TrySet(Unknown(game).Id, 5);

        var changed = ChangedOffsets(game);
        changed.Should().NotBeEmpty();
        (changed.Max() - changed.Min()).Should().BeLessThan(2);
    }

    [Fact]
    public void RemovingAnUnknownItemClearsItsSlot()
    {
        var game = SaveFilePath.Load(SaveFilePath.Unbound);

        Items(game).Remove(Unknown(game).Id);

        var exported = game.ToByteArray();
        var changed = ChangedOffsets(game);
        changed.Should().NotBeEmpty();
        (changed.Max() - changed.Min()).Should().BeLessThan(4);
        changed.Select(offset => exported[offset]).Should().OnlyContain(value => value == 0);
        game.SaveAndReload(reloaded => Items(reloaded).Items.Should().NotContain(item => item.Name == UnknownItem));
    }

    [Fact]
    public void AnUnknownItemIsNeverOfferedWhenAddingAnItem()
    {
        var game = SaveFilePath.Load(SaveFilePath.Unbound);

        foreach (var inventory in game.Trainer.Inventories.InventoryItems.Values)
            inventory.CurrentSupportedItems.Should().NotContain(item => item.IsUnknown);
    }

    [Fact]
    public void AnUnknownItemCantBeAddedToAnotherPocket()
    {
        var game = SaveFilePath.Load(SaveFilePath.Unbound);

        var add = () => game.Trainer.Inventories["Balls"].TrySet(Unknown(game).Id, 1);

        add.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void APokemonHoldingAnUnknownItemShowsIt()
    {
        var pokemon = SaveFilePath.Load(SaveFilePath.Unbound).Trainer.PokemonBox.All[HoldsUnknownItem];

        pokemon.HeldItem.Should().BeEquivalentTo(new { Name = "Unknown item #640", IsUnknown = true });
        pokemon.Details().HeldItemIsUnknown.Should().BeTrue();
    }

    [Fact]
    public void TheHeldItemChoicesIncludeAnUnknownItemOnlyForThePokemonHoldingIt()
    {
        var game = SaveFilePath.Load(SaveFilePath.Unbound);
        var holder = game.Trainer.PokemonBox.All[HoldsUnknownItem];

        holder.Options().HeldItems.Where(item => item.IsUnknown).Should().Equal(holder.HeldItem);
        game.Trainer.Party.Pokemons[0].Options().HeldItems.Should().NotContain(item => item.IsUnknown);
        game.Options.HeldItems.Should().NotContain(item => item.Id == holder.HeldItem.Id);
    }

    [Fact]
    public void AnUnknownHeldItemSurvivesAnEditToAnotherField()
    {
        var game = SaveFilePath.Load(SaveFilePath.Unbound);
        var id = game.Trainer.PokemonBox.All[HoldsUnknownItem].HeldItem.Id;

        game.Trainer.PokemonBox.All[HoldsUnknownItem].Update(new PokemonPatch(HeldItem: id, Level: 60));

        game.SaveAndReload(reloaded => reloaded.Trainer.PokemonBox.All[HoldsUnknownItem].HeldItem.Name.Should().Be("Unknown item #640"));
    }

    [Fact]
    public void RemovingAnUnknownHeldItemWritesNoItemAndSurvivesExport()
    {
        var game = SaveFilePath.Load(SaveFilePath.Unbound);

        game.Trainer.PokemonBox.All[HoldsUnknownItem].Update(new PokemonPatch(HeldItem: ItemDefinition.None));

        game.SaveAndReload(reloaded => reloaded.Trainer.PokemonBox.All[HoldsUnknownItem].HeldItem.IsNone.Should().BeTrue());
    }

    [Fact]
    public void AnUnknownHeldItemCanBeSwappedForAnotherItem()
    {
        var game = SaveFilePath.Load(SaveFilePath.Unbound);
        var leftovers = ItemRepository.GetItemByName("Leftovers")!;

        game.Trainer.PokemonBox.All[HoldsUnknownItem].Update(new PokemonPatch(HeldItem: leftovers.Id));

        game.SaveAndReload(reloaded => reloaded.Trainer.PokemonBox.All[HoldsUnknownItem].HeldItem.Id.Should().Be(leftovers.Id));
    }

    [Fact]
    public void AnotherPokemonCantBeGivenAnUnknownItem()
    {
        var game = SaveFilePath.Load(SaveFilePath.Unbound);
        var id = game.Trainer.PokemonBox.All[HoldsUnknownItem].HeldItem.Id;

        var give = () => game.Trainer.Party.Pokemons[0].Update(new PokemonPatch(HeldItem: id));

        give.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(nameof(PokemonPatch.HeldItem));
    }

    [Fact]
    public void AnIndexOutsideTheTableIsntListedAndKeepsItsBytesThroughAnEditInItsPocket()
    {
        var save = new UnboundSave(Fixture.ToArray());
        foreach (var slot in UnknownItemSlots) WriteUInt16LittleEndian(save.Data[slot..], OutsideTheTable);
        var game = Game.LoadFrom(save.Write().ToArray(), SaveFilePath.Unbound);
        var before = game.ToByteArray();

        Items(game).Items.Should().NotContain(item => item.Name == UnknownItem || item.Name == $"Unknown item #{OutsideTheTable}");
        Items(game).Remove(Items(game).Items.First(item => !item.IsNone).Id);

        var exported = game.ToByteArray();
        foreach (var slot in UnknownItemSlots) exported[slot..(slot + 4)].Should().Equal(before[slot..(slot + 4)]);
    }
}
