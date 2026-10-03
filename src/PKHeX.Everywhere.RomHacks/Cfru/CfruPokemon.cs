using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Everywhere.RomHacks.Cfru;

/// <summary>
/// A Pokémon from a CFRU save, held in the vanilla Gen 3 party layout with hack indices for species, moves and items.
/// It reports the Gen 9 context, so species, moves and forms resolve through PKHeX's national data.
/// </summary>
public abstract class CfruPokemon : PKM, IUnmappedValues, IGigantamax, IFormArgument
{
    public const int SizeParty = 100;
    public const int SizeStored = 80;
    public const int SizeBoxed = 58;

    internal static readonly Ball[] Balls =
    [
        Core.Ball.Master, Core.Ball.Ultra, Core.Ball.Great, Core.Ball.Poke, Core.Ball.Safari, Core.Ball.Net,
        Core.Ball.Dive, Core.Ball.Nest, Core.Ball.Repeat, Core.Ball.Timer, Core.Ball.Luxury, Core.Ball.Premier,
        Core.Ball.Dusk, Core.Ball.Heal, Core.Ball.Quick, Core.Ball.Cherish, Core.Ball.None, Core.Ball.Fast,
        Core.Ball.Level, Core.Ball.Lure, Core.Ball.Heavy, Core.Ball.Love, Core.Ball.Friend, Core.Ball.Moon,
        Core.Ball.Sport, Core.Ball.Beast, Core.Ball.Dream,
    ];

    // The game marks a party slot without mail, empty or not, with 0xFF.
    private const byte NoMail = 0xFF;

    protected CfruPokemon() : base(SizeParty) => Data[0x55] = NoMail;

    protected CfruPokemon(Memory<byte> data) : base(data.Length >= SizeParty ? data : Widen(data)) { }

    private static Memory<byte> Widen(Memory<byte> data)
    {
        var party = new byte[SizeParty];
        data.Span.CopyTo(party);
        return party;
    }

    protected abstract CfruSpeciesMap SpeciesMap { get; }
    protected abstract CfruItemMap ItemMap { get; }

    public static void Expand(ReadOnlySpan<byte> boxed, Span<byte> party)
    {
        party[..SizeParty].Clear();
        boxed[..0x1C].CopyTo(party);
        boxed.Slice(0x1C, 0x0B).CopyTo(party[0x20..]);
        var moves = ReadUInt64LittleEndian(boxed[0x27..]) & 0xFF_FFFF_FFFF;
        for (var i = 0; i < 4; i++)
            WriteUInt16LittleEndian(party[(0x2C + (i * 2))..], (ushort)((moves >> (i * 10)) & 0x3FF));
        boxed.Slice(0x2C, 6).CopyTo(party[0x38..]);
        boxed.Slice(0x32, 8).CopyTo(party[0x44..]);
        party[0x55] = NoMail;
    }

    public void WriteBoxed(Span<byte> boxed)
    {
        Data[..0x1C].CopyTo(boxed);
        Data.Slice(0x20, 0x0B).CopyTo(boxed[0x1C..]);
        ulong moves = 0;
        for (var i = 0; i < 4; i++)
            moves |= (ulong)(ReadUInt16LittleEndian(Data[(0x2C + (i * 2))..]) & 0x3FF) << (i * 10);
        for (var i = 0; i < 5; i++)
            boxed[0x27 + i] = (byte)(moves >> (i * 8));
        Data.Slice(0x38, 6).CopyTo(boxed[0x2C..]);
        Data.Slice(0x44, 8).CopyTo(boxed[0x32..]);
    }

    public override int SIZE_PARTY => SizeParty;
    public override int SIZE_STORED => SizeStored;
    public override EntityContext Context => EntityContext.Gen9;
    public override PersonalInfo PersonalInfo => SpeciesMap.Personal[National.Species, Form];

    public override bool Valid { get => true; set { } }
    public override bool ChecksumValid => true;

    // The game swaps the species at the start of a battle when this checksum isn't 0.
    public override void RefreshChecksum() => WriteUInt16LittleEndian(Data[0x1C..], 0);

    protected override void EncryptStored(Span<byte> stored) { }
    protected override void EncryptParty(Span<byte> party) { }

    public override Span<byte> NicknameTrash => Data.Slice(0x08, 10);
    public override Span<byte> OriginalTrainerTrash => Data.Slice(0x14, 7);
    public override int TrashCharCountTrainer => 7;
    public override int TrashCharCountNickname => 10;
    public override int MaxStringLengthTrainer => 7;
    public override int MaxStringLengthNickname => 10;

    public override ushort MaxMoveID => (ushort)(Move.MAX_COUNT - 1);
    public override ushort MaxSpeciesID => (ushort)(Core.Species.MAX_COUNT - 1);
    public override int MaxItemID => ushort.MaxValue;
    public override int MaxAbilityID => (int)Core.Ability.MAX_COUNT - 1;
    public override int MaxBallID => (int)Core.Ball.Beast;
    public override GameVersion MaxGameID => GameVersion.CXD;
    public override int MaxIV => 31;
    public override int MaxEV => EffortValues.Max255;

    public override uint PID { get => ReadUInt32LittleEndian(Data); set => WriteUInt32LittleEndian(Data, value); }
    public override uint EncryptionConstant { get => PID; set { } }
    public override uint ID32 { get => ReadUInt32LittleEndian(Data[0x04..]); set => WriteUInt32LittleEndian(Data[0x04..], value); }
    public override ushort TID16 { get => ReadUInt16LittleEndian(Data[0x04..]); set => WriteUInt16LittleEndian(Data[0x04..], value); }
    public override ushort SID16 { get => ReadUInt16LittleEndian(Data[0x06..]); set => WriteUInt16LittleEndian(Data[0x06..], value); }

    public override string Nickname
    {
        get => StringConverter3.GetString(NicknameTrash, Language);
        set => StringConverter3.SetString(NicknameTrash, value, 10, Language, StringConverterOption.None);
    }

    public override int Language { get => Data[0x12]; set => Data[0x12] = (byte)value; }

    public override string OriginalTrainerName
    {
        get => StringConverter3.GetString(OriginalTrainerTrash, Language);
        set => StringConverter3.SetString(OriginalTrainerTrash, value, 7, Language, StringConverterOption.None);
    }

    public ushort SpeciesIndex { get => ReadUInt16LittleEndian(Data[0x20..]); set => WriteUInt16LittleEndian(Data[0x20..], value); }

    private CfruSpecies National => SpeciesMap.ToNational(SpeciesIndex);

    private void WriteSpeciesIndex(ushort? index)
    {
        if (index is { } value) SpeciesIndex = value;
    }

    // Writing back the species already shown keeps the raw index, so unmapped and duplicate indices survive an edit elsewhere.
    public override ushort Species
    {
        get => SpeciesMap.ToSpeciesId(SpeciesIndex);
        set
        {
            if (value != Species) WriteSpeciesIndex(SpeciesMap.ToIndex(National with { Species = value, Form = 0 }) ?? SpeciesMap.ToIndex(value));
        }
    }

    public override byte Form
    {
        get => National.Form;
        set
        {
            if (value != Form) WriteSpeciesIndex(SpeciesMap.ToIndex(National with { Form = value }) ?? SpeciesMap.ToIndex(new CfruSpecies(Species, value)));
        }
    }

    public bool CanGigantamax
    {
        get => National.IsGigantamax;
        set
        {
            if (value != CanGigantamax) WriteSpeciesIndex(SpeciesMap.ToIndex(National with { IsGigantamax = value }));
        }
    }

    public uint FormArgument
    {
        get => National.FormArgument;
        set
        {
            if (value != FormArgument) WriteSpeciesIndex(SpeciesMap.ToIndex(National with { FormArgument = value }));
        }
    }

    // The index only holds an Alcremie sweet, so the timed form arguments of later games are always 0.
    public byte FormArgumentRemain { get => 0; set { } }
    public byte FormArgumentElapsed { get => 0; set { } }
    public byte FormArgumentMaximum { get => 0; set { } }

    public ushort? UnmappedSpecies => SpeciesIndex != 0 && National.Species == 0 ? SpeciesIndex : null;

    public ushort HeldItemIndex { get => ReadUInt16LittleEndian(Data[0x22..]); set => WriteUInt16LittleEndian(Data[0x22..], value); }

    // Like the species, writing back the item already shown keeps an index outside the hack's table.
    public override int HeldItem
    {
        get => ItemMap.ToModern(HeldItemIndex);
        set
        {
            if (value != HeldItem && (uint)value <= ushort.MaxValue && ItemMap.ToIndex((ushort)value) is { } index) HeldItemIndex = index;
        }
    }

    public ushort? UnmappedHeldItem => null;

    public override uint EXP { get => ReadUInt32LittleEndian(Data[0x24..]); set => WriteUInt32LittleEndian(Data[0x24..], value); }

    private byte PPUps { get => Data[0x28]; set => Data[0x28] = value; }
    public override int Move1_PPUps { get => (PPUps >> 0) & 3; set => PPUps = (byte)((PPUps & ~(3 << 0)) | ((value & 3) << 0)); }
    public override int Move2_PPUps { get => (PPUps >> 2) & 3; set => PPUps = (byte)((PPUps & ~(3 << 2)) | ((value & 3) << 2)); }
    public override int Move3_PPUps { get => (PPUps >> 4) & 3; set => PPUps = (byte)((PPUps & ~(3 << 4)) | ((value & 3) << 4)); }
    public override int Move4_PPUps { get => (PPUps >> 6) & 3; set => PPUps = (byte)((PPUps & ~(3 << 6)) | ((value & 3) << 6)); }

    public override byte OriginalTrainerFriendship { get => Data[0x29]; set => Data[0x29] = value; }
    public override byte CurrentFriendship { get => OriginalTrainerFriendship; set => OriginalTrainerFriendship = value; }

    public byte BallIndex { get => Data[0x2A]; set => Data[0x2A] = value; }

    public override byte Ball
    {
        get => BallIndex < Balls.Length ? (byte)Balls[BallIndex] : (byte)0;
        set
        {
            var index = Array.IndexOf(Balls, (Ball)value);
            if (value != Ball && index >= 0) BallIndex = (byte)index;
        }
    }

    public override ushort Move1 { get => ReadMove(0); set => WriteMove(0, value); }
    public override ushort Move2 { get => ReadMove(1); set => WriteMove(1, value); }
    public override ushort Move3 { get => ReadMove(2); set => WriteMove(2, value); }
    public override ushort Move4 { get => ReadMove(3); set => WriteMove(3, value); }

    public ushort GetMoveIndex(int slot) => ReadUInt16LittleEndian(Data[(0x2C + (slot * 2))..]);
    public void SetMoveIndex(int slot, ushort index) => WriteUInt16LittleEndian(Data[(0x2C + (slot * 2))..], index);

    private ushort ReadMove(int slot) => CfruMoves.ToNational(GetMoveIndex(slot));

    // Like the held item, writing back the move already shown keeps an index outside the move table.
    private void WriteMove(int slot, ushort move)
    {
        if (move != ReadMove(slot) && CfruMoves.ToIndex(move) is { } index) SetMoveIndex(slot, index);
    }

    public override int Move1_PP { get => Data[0x34]; set => Data[0x34] = (byte)value; }
    public override int Move2_PP { get => Data[0x35]; set => Data[0x35] = (byte)value; }
    public override int Move3_PP { get => Data[0x36]; set => Data[0x36] = (byte)value; }
    public override int Move4_PP { get => Data[0x37]; set => Data[0x37] = (byte)value; }

    public override int EV_HP { get => Data[0x38]; set => Data[0x38] = (byte)value; }
    public override int EV_ATK { get => Data[0x39]; set => Data[0x39] = (byte)value; }
    public override int EV_DEF { get => Data[0x3A]; set => Data[0x3A] = (byte)value; }
    public override int EV_SPE { get => Data[0x3B]; set => Data[0x3B] = (byte)value; }
    public override int EV_SPA { get => Data[0x3C]; set => Data[0x3C] = (byte)value; }
    public override int EV_SPD { get => Data[0x3D]; set => Data[0x3D] = (byte)value; }

    private byte PokerusState { get => Data[0x44]; set => Data[0x44] = value; }
    public override int PokerusDays { get => PokerusState & 0xF; set => PokerusState = (byte)((PokerusState & ~0xF) | (value & 0xF)); }
    public override int PokerusStrain { get => PokerusState >> 4; set => PokerusState = (byte)((PokerusState & 0xF) | (value << 4)); }

    public override ushort MetLocation { get => Data[0x45]; set => Data[0x45] = (byte)value; }

    private ushort Origins { get => ReadUInt16LittleEndian(Data[0x46..]); set => WriteUInt16LittleEndian(Data[0x46..], value); }
    public override byte MetLevel { get => (byte)(Origins & 0x7F); set => Origins = (ushort)((Origins & ~0x7F) | (value & 0x7F)); }
    public override GameVersion Version { get => (GameVersion)((Origins >> 7) & 0xF); set => Origins = (ushort)((Origins & ~0x780) | (((byte)value & 0xF) << 7)); }
    public override byte OriginalTrainerGender { get => (byte)((Origins >> 15) & 1); set => Origins = (ushort)((Origins & ~(1 << 15)) | ((value & 1) << 15)); }

    private uint IV32 { get => ReadUInt32LittleEndian(Data[0x48..]); set => WriteUInt32LittleEndian(Data[0x48..], value); }
    public override int IV_HP { get => ReadIV(0); set => WriteIV(0, value); }
    public override int IV_ATK { get => ReadIV(5); set => WriteIV(5, value); }
    public override int IV_DEF { get => ReadIV(10); set => WriteIV(10, value); }
    public override int IV_SPE { get => ReadIV(15); set => WriteIV(15, value); }
    public override int IV_SPA { get => ReadIV(20); set => WriteIV(20, value); }
    public override int IV_SPD { get => ReadIV(25); set => WriteIV(25, value); }

    private int ReadIV(int shift) => (int)(IV32 >> shift) & 0x1F;
    private void WriteIV(int shift, int value) => IV32 = (IV32 & ~(0x1Fu << shift)) | ((uint)Math.Clamp(value, 0, 31) << shift);

    public override bool IsEgg
    {
        get => ((IV32 >> 30) & 1) == 1 || National.IsEgg;
        set
        {
            IV32 = (IV32 & ~(1u << 30)) | (value ? 1u << 30 : 0);
            Data[0x13] = (byte)((Data[0x13] & ~4) | (value ? 4 : 0));
            if (!value && National.IsEgg) WriteSpeciesIndex(SpeciesMap.ToIndex(National with { IsEgg = false }));
        }
    }

    public bool HasHiddenAbility { get => IV32 >> 31 == 1; set => IV32 = (IV32 & 0x7FFFFFFF) | (value ? 1u << 31 : 0); }

    public override int AbilityNumber { get => HasHiddenAbility ? 4 : 1 << (int)(PID & 1); set => HasHiddenAbility = value == 4; }
    public override int Ability { get => PersonalInfo.GetAbilityAtIndex(HasHiddenAbility ? 2 : (int)(PID & 1)); set { } }

    private uint Ribbons { get => ReadUInt32LittleEndian(Data[0x4C..]); set => WriteUInt32LittleEndian(Data[0x4C..], value); }
    public override bool FatefulEncounter { get => Ribbons >> 31 == 1; set => Ribbons = (Ribbons & 0x7FFFFFFF) | (value ? 1u << 31 : 0); }

    public override int Status_Condition { get => ReadInt32LittleEndian(Data[0x50..]); set => WriteInt32LittleEndian(Data[0x50..], value); }
    public override byte Stat_Level { get => Data[0x54]; set => Data[0x54] = value; }
    public override int Stat_HPCurrent { get => ReadUInt16LittleEndian(Data[0x56..]); set => WriteUInt16LittleEndian(Data[0x56..], (ushort)value); }
    public override int Stat_HPMax { get => ReadUInt16LittleEndian(Data[0x58..]); set => WriteUInt16LittleEndian(Data[0x58..], (ushort)value); }
    public override int Stat_ATK { get => ReadUInt16LittleEndian(Data[0x5A..]); set => WriteUInt16LittleEndian(Data[0x5A..], (ushort)value); }
    public override int Stat_DEF { get => ReadUInt16LittleEndian(Data[0x5C..]); set => WriteUInt16LittleEndian(Data[0x5C..], (ushort)value); }
    public override int Stat_SPE { get => ReadUInt16LittleEndian(Data[0x5E..]); set => WriteUInt16LittleEndian(Data[0x5E..], (ushort)value); }
    public override int Stat_SPA { get => ReadUInt16LittleEndian(Data[0x60..]); set => WriteUInt16LittleEndian(Data[0x60..], (ushort)value); }
    public override int Stat_SPD { get => ReadUInt16LittleEndian(Data[0x62..]); set => WriteUInt16LittleEndian(Data[0x62..], (ushort)value); }

    public override byte Gender { get => EntityGender.GetFromPIDAndRatio(PID, PersonalInfo.Gender); set { } }
    public override Nature Nature { get => (Nature)(PID % 25); set { } }
    public override bool IsNicknamed { get => SpeciesName.IsNicknamed(National.Species, Nickname, Language); set { } }
    public override int Characteristic => -1;
    public override byte CurrentHandler { get => 0; set { } }
    public override ushort EggLocation { get => 0; set { } }

    public override string GetString(ReadOnlySpan<byte> data) => StringConverter3.GetString(data, Language);
    public override int LoadString(ReadOnlySpan<byte> data, Span<char> text) => StringConverter3.LoadString(data, text, Language);
    public override int SetString(Span<byte> data, ReadOnlySpan<char> text, int length, StringConverterOption option) =>
        StringConverter3.SetString(data, text, length, Language, option);
    public override int GetStringTerminatorIndex(ReadOnlySpan<byte> data) => TrashBytes8.GetTerminatorIndex(data);
    public override int GetStringLength(ReadOnlySpan<byte> data) => TrashBytes8.GetStringLength(data);
    public override int GetBytesPerChar() => 1;
}
