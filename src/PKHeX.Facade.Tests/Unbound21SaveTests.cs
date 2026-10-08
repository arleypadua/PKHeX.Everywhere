using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

// What issue #144 lists for the fixture.
public class Unbound21SaveTests
{
    private static readonly Game Game = SaveFilePath.Load(SaveFilePath.Unbound21);

    [Fact]
    public void LoadsAsUnbound()
    {
        Game.Format!.Id.Should().Be("unbound");
        Game.SaveFile.ChecksumsValid.Should().BeTrue();
    }

    [Fact]
    public void ReadsTheTrainer()
    {
        Game.Trainer.Name.Should().Be("Rick");
        Game.Trainer.Money.Amount.Should().Be(1_258_126);
    }

    [Fact]
    public void ReadsTheParty() =>
        Game.Trainer.Party.Pokemons.Select(pokemon => (pokemon.Species.Name, pokemon.Level)).Should().Equal(
            ("Samurott", 100), ("Gengar", 100), ("Comfey", 100), ("Garchomp", 94), ("Infernape", 100), ("Staraptor", 100));

    [Fact]
    public void ReadsHowManyPokemonEachBoxHolds()
    {
        var slots = Game.Trainer.PokemonBox.Boxes[0].Slots;
        var perBox = Game.Trainer.PokemonBox.Boxed().CountBy(boxed => boxed.Index / slots).ToDictionary();

        Enumerable.Range(0, 25).Select(box => perBox.GetValueOrDefault(box)).Should().Equal(
            30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 12, 28, 27, 29, 27, 18, 0, 0, 3, 21, 2, 11, 7, 14, 12);
    }

    [Fact]
    public void ReadsEveryPokemonAsAKnownSpecies() =>
        Game.Trainer.PokemonBox.Boxed().Should().AllSatisfy(boxed => boxed.Pokemon.IsUnknown.Should().BeFalse());

    [Theory]
    [InlineData("Items", 283)]
    [InlineData("Balls", 17)]
    [InlineData("TMHMs", 128)]
    [InlineData("Berries", 53)]
    public void ReadsHowManyKindsOfItemEachPocketHolds(string pocket, int kinds) =>
        Game.Trainer.Inventories[pocket].AllExceptNone().Should().HaveCount(kinds);
}
