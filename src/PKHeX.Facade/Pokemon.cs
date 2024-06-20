using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade;

public class Pokemon(PKM pokemon, Game game)
{
    public PKM Pkm => pokemon;
    public Game Game => game;

    public ItemDefinition Ball
    {
        get => game.ItemRepository.GetItem(pokemon.Ball);
        set => pokemon.Ball = Convert.ToByte(value.Id);
    }

    public EntityId Id => new(Pkm.TID16, Pkm.SID16);
    public Species Species => (Species)pokemon.Species;
    public string Nickname => pokemon.Nickname;
    public int Level => pokemon.CurrentLevel;
    public Stats EVs => Stats.EvFrom(pokemon);
    public Stats IVs => Stats.IvFrom(pokemon);
    public Stats Status => Stats.StatsFrom(pokemon);
    public PokemonMove Move1 => new(pokemon, PokemonMove.MoveIndex.Move1);
    public PokemonMove Move2 => new(pokemon, PokemonMove.MoveIndex.Move2);
    public PokemonMove Move3 => new(pokemon, PokemonMove.MoveIndex.Move3);
    public PokemonMove Move4 => new(pokemon, PokemonMove.MoveIndex.Move4);
    public Gender Gender => Gender.FromByte(pokemon.Gender);
    public bool IsShiny => pokemon.IsShiny;

    public Dictionary<PokemonMove.MoveIndex, PokemonMove> Moves => new()
    {
        { PokemonMove.MoveIndex.Move1, Move1 },
        { PokemonMove.MoveIndex.Move2, Move2 },
        { PokemonMove.MoveIndex.Move3, Move3 },
        { PokemonMove.MoveIndex.Move4, Move4 },
    };

    public void ChangeLevel(int level)
    {
        var clamped = Math.Clamp(level, 1, 100);
        pokemon.CurrentLevel = Convert.ToByte(clamped);
    }

    public void ChangeNickname(string nickname)
    {
        pokemon.Nickname = nickname;
    }

    public void SetShiny(bool shiny)
    {
        pokemon.SetIsShiny(shiny);
    }

    public void ChangeMove(PokemonMove.MoveIndex moveIndex, MoveDefinition newMove)
    {
        var newMoveSet = new Moveset(
            moveIndex == PokemonMove.MoveIndex.Move1 ? newMove.Id : Move1.Move.Id,
            moveIndex == PokemonMove.MoveIndex.Move2 ? newMove.Id : Move2.Move.Id,
            moveIndex == PokemonMove.MoveIndex.Move3 ? newMove.Id : Move3.Move.Id,
            moveIndex == PokemonMove.MoveIndex.Move4 ? newMove.Id : Move4.Move.Id);

        if (newMoveSet.ToArray().All(m => m == MoveDefinition.None.Id))
        {
            return;
        }

        pokemon.SetMoves(newMoveSet);
        pokemon.FixMoves();
    }

    public record Stats(int Attack, int Defense, int Health, int Speed)
    {
        public static Stats EvFrom(PKM pokemon) => new(pokemon.EV_ATK, pokemon.EV_DEF, pokemon.EV_HP, pokemon.EV_SPD);
        public static Stats IvFrom(PKM pokemon) => new(pokemon.IV_ATK, pokemon.IV_DEF, pokemon.IV_HP, pokemon.IV_SPD);

        public static Stats StatsFrom(PKM pokemon) =>
            new(pokemon.Stat_ATK, pokemon.Stat_DEF, pokemon.Stat_HPMax, pokemon.Stat_SPD);

        public override string ToString() => $"Atk: {Attack} / Def: {Defense} / SPD: {Speed} / HP: {Health}";
    }
}

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