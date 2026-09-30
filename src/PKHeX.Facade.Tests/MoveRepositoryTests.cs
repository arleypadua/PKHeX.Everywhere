using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class MoveRepositoryTests
{
    [Fact]
    public void ShouldListPossibleMovesForBattleRevolution()
    {
        var pkm = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons.First().Pkm;
        var battleRevolution = Game.EmptyOf(GameVersionRepository.Instance.Get(GameVersion.BATREV));
        var pokemon = new Pokemon(pkm, battleRevolution);

        MoveRepository.Instance.PossibleMovesFor(pokemon).Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldResolveUnknownMove()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.Emerald).Trainer.Party.Pokemons.First();
        pokemon.Pkm.Move1 = 1070;

        pokemon.Moves.Values.First().Move.Name.Should().Be("Unknown (1070)");
        FluentActions.Invoking(() => MoveRepository.Instance.PossibleMovesFor(pokemon)).Should().NotThrow();
    }
}
