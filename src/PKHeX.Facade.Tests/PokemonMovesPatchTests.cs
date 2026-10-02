using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonMovesPatchTests
{
    [Theory]
    [SupportedSaveFiles]
    public void DetailsCarryTheFourMoveSlots(string saveFile)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];

        var moves = pokemon.Details().Moves;

        moves.Select(m => m.Id).Should().Equal(pokemon.Moves.Values.Select(m => (int)m.Move.Id));
        moves.Select(m => m.Name).Should().Equal(pokemon.Moves.Values.Select(m => m.Move.Name));
        moves.Select(m => (m.Pp, m.MaxPp)).Should().Equal(pokemon.Moves.Values.Select(m => (m.PP.Current, m.PP.Max)));
    }

    [Theory]
    [SupportedSaveFiles]
    public void AppliesTheMovesAndFixesThem(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        var pokemon = game.Trainer.Party.Pokemons[0];
        var move = game.Options.Moves.First(m => pokemon.Moves.Values.All(current => current.Move.Id != m.Id)).Id;

        pokemon.Update(new PokemonPatch(Moves: [0, move, 0, 0]));

        pokemon.Details().Moves.Select(m => m.Id).Should().Equal(move, 0, 0, 0);
        pokemon.Move1.PP.Current.Should().Be(pokemon.Move1.PP.Max);
    }

    [Fact]
    public void AnAllEmptyMovesetLeavesTheMovesUnchanged()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons[0];
        var before = pokemon.Details().Moves;

        pokemon.Update(new PokemonPatch(Moves: [0, 0, 0, 0], Level: 42));

        pokemon.Details().Moves.Should().Equal(before);
        pokemon.Level.Should().Be(42);
    }

    [Theory]
    [InlineData(5000)]
    [InlineData(-1)]
    public void RejectsAMoveTheGameDoesntHave(int move)
    {
        var pokemon = Game.LoadFrom(SaveFilePath.Emerald).Trainer.Party.Pokemons[0];
        var before = pokemon.Details().Moves;

        var update = () => pokemon.Update(new PokemonPatch(Moves: [move, 0, 0, 0]));

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(nameof(PokemonPatch.Moves));
        pokemon.Details().Moves.Should().Equal(before);
    }

    [Theory]
    [InlineData(new[] { 1 })]
    [InlineData(new[] { 1, 2, 3, 4, 5 })]
    public void RejectsAMovesetThatIsntFourSlots(int[] moves)
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons[0];

        var update = () => pokemon.Update(new PokemonPatch(Moves: moves));

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(nameof(PokemonPatch.Moves));
    }

    [Theory]
    [SupportedSaveFiles]
    public void OptionsListTheLegalMoves(string saveFile)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];

        pokemon.Options().Moves.Should().Equal(MoveRepository.Instance.PossibleMovesFor(pokemon).Select(m => new Choice(m.Id, m.Name)));
    }

    [Fact]
    public void GameMovesAreEveryMoveInTheGame()
    {
        var game = Game.LoadFrom(SaveFilePath.Emerald);

        var moves = game.Options.Moves;

        moves.Should().Contain(m => m.Id == (int)Move.Thunderbolt);
        moves.Should().NotContain(m => m.Id == (int)Move.None || m.Id > game.SaveFile.MaxMoveID);
        moves.Select(m => m.Name).Should().BeInAscendingOrder();
    }
}
