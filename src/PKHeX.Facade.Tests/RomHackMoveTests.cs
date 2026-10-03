using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Cfru;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class RomHackMoveTests
{
    private const ushort UnmappedMove = 90;
    private const ushort OutsideTheMoveTable = 1010;
    private const int Slot = 3;

    public static TheoryData<bool> PartyOrBox => [true, false];

    private static Pokemon Target(Game game, bool party) =>
        party ? game.Trainer.Party.Pokemons[0] : game.Trainer.PokemonBox.All[0];

    private static ushort[] MoveIndexes(Pokemon pokemon) =>
        Enumerable.Range(0, 4).Select(((CfruPokemon)pokemon.Pkm).GetMoveIndex).ToArray();

    private static Game UnboundWithMove(ushort index, bool party)
    {
        var save = new UnboundSave(File.ReadAllBytes(SaveFilePath.Unbound));
        var pokemon = (CfruPokemon)(party ? save.GetPartySlotAtIndex(0) : save.GetBoxSlotAtIndex(0));
        pokemon.SetMoveIndex(Slot, index);
        if (party) save.SetPartySlotAtIndex(pokemon, 0);
        else save.SetBoxSlotAtIndex(pokemon, 0);
        return Game.LoadFrom(save.Write().ToArray(), SaveFilePath.Unbound);
    }

    [Theory]
    [MemberData(nameof(PartyOrBox))]
    public void AnUnknownMoveShowsInItsSlot(bool party)
    {
        var moves = Target(UnboundWithMove(UnmappedMove, party), party).Details().Moves;

        moves[Slot].Name.Should().Be($"Unknown move #{UnmappedMove}");
        moves[Slot].Id.Should().NotBe((int)Move.None);
        moves.Select(move => move.IsUnknown).Should().Equal(false, false, false, true);
    }

    [Theory]
    [MemberData(nameof(PartyOrBox))]
    public void AnUnknownMoveSurvivesAnEditToAnotherFieldAndExport(bool party)
    {
        var game = UnboundWithMove(UnmappedMove, party);

        Target(game, party).Update(new PokemonPatch(Nickname: "Edited"));

        game.SaveAndReload(reloaded =>
        {
            var pokemon = Target(reloaded, party);
            pokemon.Nickname.Should().Be("Edited");
            MoveIndexes(pokemon)[Slot].Should().Be(UnmappedMove);
            pokemon.Details().Moves[Slot].Name.Should().Be($"Unknown move #{UnmappedMove}");
        });
    }

    [Theory]
    [MemberData(nameof(PartyOrBox))]
    public void ClearingAnUnknownMoveWritesNoMove(bool party)
    {
        var game = UnboundWithMove(UnmappedMove, party);
        var moves = Target(game, party).Details().Moves.Select(move => move.Id).ToArray();

        Target(game, party).Update(new PokemonPatch(Moves: [moves[0], moves[1], moves[2], 0]));

        game.SaveAndReload(reloaded =>
        {
            var pokemon = Target(reloaded, party);
            MoveIndexes(pokemon)[Slot].Should().Be(0);
            pokemon.Details().Moves.Select(move => move.Id).Should().Equal(moves[0], moves[1], moves[2], 0);
            pokemon.Details().Moves.Should().NotContain(move => move.IsUnknown);
        });
    }

    [Theory]
    [MemberData(nameof(PartyOrBox))]
    public void AnUnknownMoveCanBeReplaced(bool party)
    {
        var game = UnboundWithMove(UnmappedMove, party);
        var moves = Target(game, party).Details().Moves.Select(move => move.Id).ToArray();

        Target(game, party).Update(new PokemonPatch(Moves: [moves[0], moves[1], moves[2], (int)Move.Thunderbolt]));

        game.SaveAndReload(reloaded => Target(reloaded, party).Details().Moves.Select(move => move.Id)
            .Should().Equal(moves[0], moves[1], moves[2], (int)Move.Thunderbolt));
    }

    [Fact]
    public void AnUnknownMoveIsOnlyAChoiceForTheSlotHoldingIt()
    {
        var pokemon = Target(UnboundWithMove(UnmappedMove, party: false), party: false);
        var moves = pokemon.Details().Moves.Select(move => move.Id).ToArray();

        pokemon.Options().Moves.Should().NotContain(move => move.Id == moves[Slot]);
        pokemon.Game.Options.Moves.Should().NotContain(move => move.Id == moves[Slot]);
        var moveIt = () => pokemon.Update(new PokemonPatch(Moves: [moves[0], moves[1], moves[Slot], moves[2]]));
        moveIt.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(nameof(PokemonPatch.Moves));
    }

    [Fact]
    public void AnIndexOutsideTheMoveTableStaysAsItIs()
    {
        var game = UnboundWithMove(OutsideTheMoveTable, party: true);

        Target(game, party: true).Update(new PokemonPatch(Nickname: "Edited"));

        game.SaveAndReload(reloaded => MoveIndexes(Target(reloaded, party: true))[Slot].Should().Be(OutsideTheMoveTable));
    }
}
