using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade;

public class Pokemon(PKM pokemon, Game game)
{
    // for some reflection
    public Pokemon() : this(default!, default!) { }
    
    public PKM Pkm => pokemon;
    public Game Game => game;

    public ItemDefinition Ball
    {
        get => game.ItemRepository.GetItem(pokemon.Ball);
        set => pokemon.Ball = Convert.ToByte(value.Id);
    }

    public EntityId Id => new(Pkm.TID16, Pkm.SID16);
    public uint PID => Pkm.PID;
    public Species Species => (Species)pokemon.Species;
    public string Nickname => pokemon.Nickname;
    public int Level => pokemon.CurrentLevel;
    public PokemonNature Natures => new(pokemon);
    public Stats EVs => Stats.EvFrom(pokemon);
    public Stats IVs => Stats.IvFrom(pokemon);
    public Stats BaseStats => Stats.BaseFrom(pokemon);
    public PokemonMove Move1 => new(pokemon, PokemonMove.MoveIndex.Move1);
    public PokemonMove Move2 => new(pokemon, PokemonMove.MoveIndex.Move2);
    public PokemonMove Move3 => new(pokemon, PokemonMove.MoveIndex.Move3);
    public PokemonMove Move4 => new(pokemon, PokemonMove.MoveIndex.Move4);
    public Gender Gender => Gender.FromByte(pokemon.Gender);
    public bool IsShiny => pokemon.IsShiny;
    public ItemDefinition HeldItem => game.ItemRepository.GetItem(pokemon.HeldItem);
    public AbilityDefinition Ability => AbilityRepository.Instance.Get(pokemon.Ability);
    public int Friendship => pokemon.CurrentFriendship;
    public PokemonFlags Flags => new(pokemon);
    public PokemonMetConditions MetConditions => new(pokemon);

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

    public record Stats(int Attack, int Defense, int SpecialAttack, int SpecialDefense, int Health, int Speed)
    {
        public int Total => Attack + Defense + SpecialAttack + SpecialDefense + Health + Speed;
        
        public static Stats EvFrom(PKM pokemon) => new(pokemon.EV_ATK, pokemon.EV_DEF, pokemon.EV_SPA, pokemon.EV_SPD,
            pokemon.EV_HP, pokemon.EV_SPD);

        public static Stats IvFrom(PKM pokemon) => new(pokemon.IV_ATK, pokemon.IV_DEF, pokemon.IV_SPA, pokemon.IV_SPD,
            pokemon.IV_HP, pokemon.IV_SPD);

        public static Stats BaseFrom(PKM pokemon) =>
            new(pokemon.Stat_ATK, pokemon.Stat_DEF, pokemon.Stat_SPA, pokemon.Stat_SPD, pokemon.Stat_HPMax,
                pokemon.Stat_SPD);
    }

    public record PokemonNature(PKM pokemon)
    {
        public Nature Nature => pokemon.Nature;
        public Nature StatNature => pokemon.StatNature;
    }

    public class PokemonFlags(PKM pokemon)
    {
        public bool IsEgg => pokemon.IsEgg;
        public bool IsInfected => pokemon.IsPokerusInfected;
        public bool IsCured => pokemon.IsPokerusCured;
    }

    public class PokemonMetConditions(PKM pokemon)
    {
        public GameVersionDefinition Version => GameVersionRepository.Instance.Get(pokemon.Version);
        public Location Location => new(pokemon.MetLocation, pokemon.GetLocationString(false));
        public Location EggLocation => new(pokemon.EggLocation, pokemon.GetLocationString(true));
    }
}

public record Location(ushort Id, string Name);

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