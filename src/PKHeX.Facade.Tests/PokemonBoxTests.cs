using AwesomeAssertions;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonBoxTests
{
    [Theory]
    [SupportedSaveFiles]
    public void BoxShouldContainPokemon(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var allValid = game.Trainer.PokemonBox.All
            .Where(p => p.Species != SpeciesDefinition.None)
            .ToList();
        
        allValid.Should().HaveCountGreaterThan(0);
        allValid.Should().AllSatisfy(p =>
        {
            p.BaseStats.Attack.Should().BeGreaterThan(0);
            p.BaseStats.Defense.Should().BeGreaterThan(0);
            p.BaseStats.Health.Should().BeGreaterThan(0);
            p.BaseStats.Speed.Should().BeGreaterThan(0);
            p.BaseStats.Total.Should().BeGreaterThan(0);
            p.BaseStats.SpecialAttack.Should().BeGreaterThan(0);
            p.BaseStats.SpecialDefense.Should().BeGreaterThan(0);
        });
    }

    [Theory]
    [SupportedSaveFiles]
    public void BoxedListsTheNonEmptySlotsInOrder(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var partyMembers = Enumerable.Range(0, game.Trainer.Party.Pokemons.Count).Select(game.Trainer.Party.BoxIndexOf).ToHashSet();
        var expected = game.Trainer.PokemonBox.All
            .Select((pokemon, index) => (index, pokemon))
            .Where(p => p.pokemon.Species != SpeciesDefinition.None && !partyMembers.Contains(p.index))
            .ToList();

        game.Trainer.PokemonBox.Boxed().Should().Equal(expected);
    }

    [Theory]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    [InlineData(SaveFilePath.LetsGoEevee)]
    public void BoxedLeavesOutLetsGoPartyMembers(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var partyMembers = Enumerable.Range(0, game.Trainer.Party.Pokemons.Count)
            .Select(slot => game.Trainer.Party.BoxIndexOf(slot)!.Value)
            .ToList();

        partyMembers.Should().NotBeEmpty();
        game.Trainer.PokemonBox.Boxed().Select(p => p.Index).Should().NotIntersectWith(partyMembers);
    }

    [Fact]
    public void BoxesListsEveryYellowBoxWithoutNames()
    {
        var game = SaveFilePath.Load(SaveFilePath.Yellow);

        game.Trainer.PokemonBox.Boxes.Should().Equal(Enumerable.Range(0, 12).Select(number => new Box(number, null, 20)));
    }

    [Theory]
    [InlineData(SaveFilePath.Crystal, "BOX1")]
    [InlineData(SaveFilePath.Emerald, "BOX1")]
    [InlineData(SaveFilePath.HgSs, "BOX 1")]
    [InlineData(SaveFilePath.FireRed, "10A EUR1")]
    public void BoxesCarryTheNamesTheSaveStores(string saveFile, string firstName)
    {
        var boxes = SaveFilePath.Load(saveFile).Trainer.PokemonBox.Boxes;

        boxes[0].Name.Should().Be(firstName);
        boxes.Should().AllSatisfy(b => b.Name.Should().NotBeNullOrEmpty());
    }

    [Theory]
    [SupportedSaveFiles]
    public void EveryBoxedPokemonIsInAListedBox(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var boxes = game.Trainer.PokemonBox.Boxes;

        boxes.Select(b => b.Number).Should().Equal(Enumerable.Range(0, boxes.Count));
        game.Trainer.PokemonBox.Boxed().Should().AllSatisfy(boxed =>
        {
            var number = boxed.Index / game.SaveFile.BoxSlotCount;
            number.Should().BeLessThan(boxes.Count);
            (boxed.Index % game.SaveFile.BoxSlotCount).Should().BeLessThan(boxes[number].Slots);
        });
    }
}
