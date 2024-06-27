using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Pokemons;

public class PokemonMove(
    PKM pokemon,
    PokemonMove.MoveIndex moveIndex)
{
    public MoveDefinition Move =>
        moveIndex switch
        {
            MoveIndex.Move1 => MoveRepository.Instance.GetMove(pokemon.Move1),
            MoveIndex.Move2 => MoveRepository.Instance.GetMove(pokemon.Move2),
            MoveIndex.Move3 => MoveRepository.Instance.GetMove(pokemon.Move3),
            MoveIndex.Move4 => MoveRepository.Instance.GetMove(pokemon.Move4),
            _ => throw new ArgumentOutOfRangeException()
        };

    public PowerPoints PP => PowerPoints.From(pokemon, moveIndex);

    public enum MoveIndex
    {
        Move1 = 0,
        Move2 = 1,
        Move3 = 2,
        Move4 = 3
    }

    public record PowerPoints(int Current, int Max)
    {
        public override string ToString() => $"{Current}/{Max}";

        public static PowerPoints From(PKM pokemon, MoveIndex moveIndex) => moveIndex switch
        {
            MoveIndex.Move1 => new(pokemon.Move1_PP, pokemon.GetMovePP(pokemon.Move1, pokemon.Move1_PPUps)),
            MoveIndex.Move2 => new(pokemon.Move2_PP, pokemon.GetMovePP(pokemon.Move2, pokemon.Move2_PPUps)),
            MoveIndex.Move3 => new(pokemon.Move3_PP, pokemon.GetMovePP(pokemon.Move3, pokemon.Move3_PPUps)),
            MoveIndex.Move4 => new(pokemon.Move4_PP, pokemon.GetMovePP(pokemon.Move4, pokemon.Move4_PPUps)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}