using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade;

public class Pokemon
{
    private readonly PKM _pokemon;
    private readonly Game _game;

    public Pokemon(PKM pokemon, Game game)
    {
        _pokemon = pokemon;
        _game = game;
    }

    public Species Species => (Species)_pokemon.Species;
    public string Nickname => _pokemon.Nickname;
    public int Level => _pokemon.CurrentLevel;
    public PokemonMove Move1 => new PokemonMove(_pokemon, _game.MoveRepository.GetMove, PokemonMove.MoveIndex.Move1);
    public PokemonMove Move2 => new PokemonMove(_pokemon, _game.MoveRepository.GetMove, PokemonMove.MoveIndex.Move2);
    public PokemonMove Move3 => new PokemonMove(_pokemon, _game.MoveRepository.GetMove, PokemonMove.MoveIndex.Move3);
    public PokemonMove Move4 => new PokemonMove(_pokemon, _game.MoveRepository.GetMove, PokemonMove.MoveIndex.Move4);
    public Gender Gender => Gender.FromByte(_pokemon.Gender);
}

public class PokemonMove
{
    private PKM _pokemon;
    private Func<ushort, MoveDefinition> _getMove;
    private readonly MoveIndex _moveIndex;

    public PokemonMove(
        PKM pokemon,
        Func<ushort, MoveDefinition> getMove,
        MoveIndex moveIndex)
    {
        _pokemon = pokemon;
        _getMove = getMove;
        _moveIndex = moveIndex;
    }

    public MoveDefinition Move
    {
        get => _moveIndex switch
        {
            MoveIndex.Move1 => _getMove(_pokemon.Move1),
            MoveIndex.Move2 => _getMove(_pokemon.Move2),
            MoveIndex.Move3 => _getMove(_pokemon.Move3),
            MoveIndex.Move4 => _getMove(_pokemon.Move4),
            _ => throw new ArgumentOutOfRangeException()
        };
        // set to be implemented
    }

    public enum MoveIndex
    {
        Move1 = 0,
        Move2 = 1,
        Move3 = 2,
        Move4 = 3
    }
}