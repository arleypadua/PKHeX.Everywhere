using PKHeX.Core;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Pokemons;

public class Pokemon(PKM pokemon, Game game)
{
    // for some reflection
    public Pokemon() : this(default!, default!)
    {
    }

    public UniqueId UniqueId => UniqueId.From(this);
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
    public PokemonTypes Types => new(pokemon);
    public string Nickname => pokemon.Nickname;

    public bool NicknameSet =>
        !pokemon.Nickname.Equals(Species.Name(), StringComparison.InvariantCultureIgnoreCase);

    public int Level => pokemon.CurrentLevel;
    public PokemonNature Natures => new(pokemon);

    public PokemonForm Form => new(pokemon);

    public Stats EVs => Stats.EvFrom(pokemon);
    public Stats IVs => Stats.IvFrom(pokemon);
    public Stats BaseStats => Stats.BaseFrom(pokemon);
    public Stats? AVs => pokemon is IAwakened ? Stats.AvFrom(pokemon) : null; 
    public PokemonMove Move1 => new(pokemon, PokemonMove.MoveIndex.Move1);
    public PokemonMove Move2 => new(pokemon, PokemonMove.MoveIndex.Move2);
    public PokemonMove Move3 => new(pokemon, PokemonMove.MoveIndex.Move3);
    public PokemonMove Move4 => new(pokemon, PokemonMove.MoveIndex.Move4);
    public Gender Gender => Gender.FromByte(pokemon.Gender);
    public bool IsShiny => pokemon.IsShiny;
    public ItemDefinition HeldItem
    {
        get => game.ItemRepository.GetItem(pokemon.HeldItem);
        set => pokemon.HeldItem = value.Id;
    }

    public AbilityDefinition Ability => AbilityRepository.Instance.Get(pokemon.Ability);

    public int Friendship
    {
        get => pokemon.CurrentFriendship;
        set => pokemon.CurrentFriendship = (byte)Math.Clamp(value, 0, 255);
    }

    public PokemonFlags Flags => new(pokemon);
    public MetConditions MetConditions => new(pokemon);
    public Egg Egg => new(pokemon);

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
        pokemon.SetNickname(nickname);
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

    public Pokemon MakeCopy()
    {
        var underlyingPkm = Pkm.Clone();
        underlyingPkm.ClearNickname();
        
        var isShiny = underlyingPkm.IsShiny;
        
        // re-roll the pid
        underlyingPkm.PID = EntityPID.GetRandomPID(Random.Shared, underlyingPkm.Species, underlyingPkm.Gender,
            underlyingPkm.Version, underlyingPkm.Nature, underlyingPkm.Form, underlyingPkm.PID);

        if (isShiny)
        {
            // because re-rolling may void the shiny status, we are making it shiny again
            underlyingPkm.SetIsShiny(true);
        }

        return new Pokemon(underlyingPkm, Game);
    }
}