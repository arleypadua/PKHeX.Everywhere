using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Pokemons;

public partial class Pokemon(PKM pokemon, Game game)
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
        get => ItemRepository.GetBall((Ball)pokemon.Ball) ?? ItemDefinition.Unknown(pokemon.Ball);
        set => pokemon.Ball = Convert.ToByte(value.Id);
    }

    public EntityId Id => new(Pkm.DisplayTID, Pkm.DisplaySID);
    public Owner Owner => new(pokemon);
    
    public uint PID => Pkm.PID;
    
    public GameVersionDefinition Version => GameVersionRepository.Instance.Get(Pkm.Version);

    public bool IsEmpty => IsBlank(pokemon);

    public bool IsUnknown => !IsBlank(pokemon) && !Game.SpeciesRepository.Knows(pokemon.Species);

    public bool IsEditable => !IsUnknown;

    internal static bool IsBlank(PKM pkm) => pkm.Species == 0;

    /// <exception cref="UnknownSpeciesException">The species has no PKHeX id, so the Pokémon can't be edited or copied.</exception>
    public void RequireEditable()
    {
        if (!IsEditable) throw new UnknownSpeciesException($"{Species.Name} can't be edited, as its species is unknown.");
    }

    public SpeciesDefinition Species
    {
        get => Game.SpeciesRepository.Get((Species)Pkm.Species);
        set
        {
            if (Pkm.Species == value.ShortId) return;

            var nicknamed = NicknameSet;
            Pkm.Species = value.ShortId;

            if (Pkm is ICombatPower combatPower) combatPower.ResetCP();
            if (!nicknamed) Pkm.ResetNickname();
        }
    }

    public PokemonTypes Types => new(pokemon);
    public string Nickname => pokemon.Nickname;

    public bool NicknameSet =>
        !pokemon.Nickname.Equals(Species.Name, StringComparison.InvariantCultureIgnoreCase);

    public int Level => pokemon.CurrentLevel;

    public uint Experience
    {
        get => pokemon.EXP;
        set => pokemon.EXP = value;
    }

    public PokemonNature Natures => new(pokemon);

    public PokemonForm Form => new(pokemon);

    public Stats EVs => Stats.EvFrom(pokemon);
    public Stats IVs => Stats.IvFrom(pokemon);
    public Stats BaseStats => Stats.BaseFrom(pokemon);
    public Stats? AVs => pokemon is IAwakened ? Stats.AvFrom(pokemon) : null;
    public HiddenPowerDefinition? HiddenPower =>
        pokemon.Context is EntityContext.Gen1 or EntityContext.Gen7b || pokemon.Context.Generation >= 8
            ? null
            : new(GameInfo.Strings.types[pokemon.HPType + 1], pokemon.Format <= 5 ? pokemon.HPPower : null);
    public PokemonMove Move1 => new(pokemon, PokemonMove.MoveIndex.Move1, game.GameData);
    public PokemonMove Move2 => new(pokemon, PokemonMove.MoveIndex.Move2, game.GameData);
    public PokemonMove Move3 => new(pokemon, PokemonMove.MoveIndex.Move3, game.GameData);
    public PokemonMove Move4 => new(pokemon, PokemonMove.MoveIndex.Move4, game.GameData);
    public Gender Gender
    {
        get => Gender.FromByte(pokemon.Gender);
        set => pokemon.SetGender(value.ToByte());
    }

    public bool IsShiny => pokemon.IsShiny;

    public bool SupportsAlpha => Pkm is IAlpha;

    public bool IsAlpha
    {
        get => Pkm is IAlpha { IsAlpha: true };
        set
        {
            if (Pkm is not IAlpha alpha)
                throw new InvalidOperationException($"{Pkm.GetType().Name} does not support the alpha flag.");
            alpha.IsAlpha = value;
        }
    }

    public ItemDefinition HeldItem
    {
        get => Game.ItemRepository.GetGameItem(Convert.ToUInt16(pokemon.HeldItem));
        set => pokemon.HeldItem = value.Id;
    }

    public AbilityDefinition Ability
    {
        get => AbilityRepository.Instance.Get(pokemon.Ability);
        set => pokemon.Ability = value.Id;
    }

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
        RequireEditable();
        var clamped = Math.Clamp(level, 1, 100);
        pokemon.CurrentLevel = Convert.ToByte(clamped);
    }

    public void ChangeNickname(string nickname)
    {
        if (nickname.Length == 0) pokemon.ResetNickname();
        else pokemon.SetNickname(nickname);
    }

    public void SetShiny(bool shiny)
    {
        pokemon.SetShinyKeepingGender(shiny);
    }

    public void ChangeMove(PokemonMove.MoveIndex moveIndex, MoveDefinition newMove) =>
        ChangeMoves(Moves.Select(m => m.Key == moveIndex ? newMove.Id : m.Value.Move.Id).ToArray());

    private void ChangeMoves(ushort[] moves)
    {
        if (moves.All(move => move == MoveDefinition.None.Id)) return;

        pokemon.SetMoves(moves);
        pokemon.FixMoves();
    }

    public Pokemon MakeCopy()
    {
        RequireEditable();
        var underlyingPkm = Pkm.Clone();
        underlyingPkm.ResetNickname();

        var isShiny = underlyingPkm.IsShiny;

        // re-roll the pid
        underlyingPkm.PID = underlyingPkm.RandomPid(Random.Shared);

        if (isShiny)
        {
            // because re-rolling may void the shiny status, we are making it shiny again
            underlyingPkm.SetShinyKeepingGender(true);
        }

        return new Pokemon(underlyingPkm, Game);
    }

    public Pokemon Clone() => new(Pkm.Clone(), Game);

    public void ApplyChangesFrom(Pokemon template, bool keepPid = true)
    {
        var pid = PID;
        template.Pkm.Data.CopyTo(Pkm.Data);

        if (keepPid)
        {
            Pkm.PID = pid;
        }
    }

    public File ToFile(bool encrypted = false)
    {
        var bytes = new byte[Pkm.SIZE_PARTY];
        if (encrypted)
        {
            Pkm.WriteEncryptedDataParty(bytes);
        }
        else
        {
            Pkm.WriteDecryptedDataParty(bytes);
        }

        return new File
        {
            Name = Pkm.FileName,
            Bytes = bytes
        };
    }

    public Pokemon? ConvertTo(Game target, out EntityConverterResult result)
    {
        var targetType = target.SaveFile.PKMType;
        if (Pkm.GetType() == targetType)
        {
            result = EntityConverterResult.None;
            return new Pokemon(Pkm, target);
        }

        var converted = EntityConverter.ConvertToType(Pkm, targetType, out result);
        return converted is null ? null : new Pokemon(converted, target);
    }

    public static Pokemon LoadFrom(
        byte[] bytes, 
        Game? game = null)
    {
        var format =
            EntityFileExtension.GetContextFromExtension(string.Empty, game?.Generation ?? EntityContext.Gen6);
        var pkm = EntityFormat.GetFromBytes(bytes, prefer: format)
                  ?? throw new InvalidOperationException("The file did not load into a valid pokemon file.");

        var version = GameVersionRepository.Instance.Get(pkm.Version);

        return new Pokemon(pkm, game ?? Game.EmptyOf(version));
    }

    public class File
    {
        public required string Name { get; init; }
        public required byte[] Bytes { get; init; }
    }
}

public class UnknownSpeciesException(string message) : Exception(message);

public record HiddenPowerDefinition(string Type, int? Power);