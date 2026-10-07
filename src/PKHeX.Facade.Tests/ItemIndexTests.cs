using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class ItemIndexTests
{
    private const int HoldsUnknownItem = (19 * 30) + 27;

    private static Inventory.Item Owned(Game game, string pouch, string name) =>
        game.Trainer.Inventories[pouch].Items.Single(item => item.Name == name);

    [Fact]
    public void AnUnboundHeldItemHasItsHackIndex()
    {
        var pokemon = SaveFilePath.Load(SaveFilePath.Unbound).Trainer.Party.Pokemons[0];

        pokemon.HeldItem.Name.Should().Be("Light Clay");
        pokemon.HeldItem.Id.Should().Be(269);
        pokemon.HeldItemIndex.Should().Be(90);
        pokemon.Details().HeldItemIndex.Should().Be(90);
        pokemon.Options().HeldItems.Single(item => item.Id == 269).Index.Should().Be(90);
    }

    [Fact]
    public void AnUnboundUnknownHeldItemHasItsHackIndex()
    {
        var pokemon = SaveFilePath.Load(SaveFilePath.Unbound).Trainer.PokemonBox.All[HoldsUnknownItem];

        pokemon.HeldItem.IsUnknown.Should().BeTrue();
        pokemon.HeldItemIndex.Should().Be(640);
        pokemon.Options().HeldItems.Single(item => item.IsUnknown).Index.Should().Be(640);
    }

    [Fact]
    public void AnUnboundBagItemHasItsHackIndex()
    {
        var game = SaveFilePath.Load(SaveFilePath.Unbound);
        var fullRestore = Owned(game, "Items", "Full Restore");

        fullRestore.Id.Should().Be(23);
        fullRestore.Index.Should().Be(19);
    }

    [Fact]
    public void AnUnboundUnknownBagItemHasItsHackIndex() =>
        Owned(SaveFilePath.Load(SaveFilePath.Unbound), "Items", "Unknown item #79").Index.Should().Be(79);

    [Fact]
    public void AnItemTheBagCanAddHasItsHackIndex()
    {
        var game = SaveFilePath.Load(SaveFilePath.Unbound);
        game.Trainer.Inventories["Items"].Remove(23);

        game.Trainer.Inventories["Items"].CurrentSupportedItems.Single(item => item.Id == 23).Index.Should().Be(19);
    }

    [Fact]
    public void AnImperiumHeldItemHasItsHackIndex()
    {
        var pokemon = SaveFilePath.Load(SaveFilePath.Imperium).Trainer.Party.Pokemons[0];

        pokemon.HeldItem.Name.Should().Be("Sitrus Berry");
        pokemon.HeldItemIndex.Should().Be(523);
        pokemon.Details().HeldItemIndex.Should().Be(523);
    }

    [Fact]
    public void AnImperiumBagItemHasItsHackIndex()
    {
        var abilityCapsule = Owned(SaveFilePath.Load(SaveFilePath.Imperium), "Items", "Ability Capsule");

        abilityCapsule.Id.Should().Be(645);
        abilityCapsule.Index.Should().Be(79);
    }

    [Fact]
    public void AVanillaEmeraldHeldItemIndexIsItsId()
    {
        var pokemon = SaveFilePath.Load(SaveFilePath.Emerald).Trainer.Party.Pokemons[0];
        pokemon.Pkm.HeldItem = 200;

        pokemon.HeldItemIndex.Should().Be(200);
        pokemon.Details().HeldItemIndex.Should().Be(200);
        pokemon.Options().HeldItems.Should().OnlyContain(item => item.Index == item.Id);
    }

    [Fact]
    public void AVanillaEmeraldBagItemIndexIsItsId()
    {
        var yellowShard = Owned(SaveFilePath.Load(SaveFilePath.Emerald), "Items", "Yellow Shard");

        yellowShard.Index.Should().Be(yellowShard.Id);
    }
}
