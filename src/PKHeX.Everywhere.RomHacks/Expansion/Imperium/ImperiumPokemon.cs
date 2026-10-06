using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Cfru;
using PKHeX.Facade.Abstractions;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Everywhere.RomHacks.Expansion.Imperium;

/// <summary>
/// A Pokémon from an Emerald Imperium save, held decrypted in the Gen 3 party layout with pokeemerald-expansion's wider fields.
/// It reports the Gen 9 context, so species, moves and forms resolve through PKHeX's national data.
/// Every write keeps the bits around the field it changes, including the two flags only Imperium has.
/// </summary>
public sealed class ImperiumPokemon : PKM, ISpeciesIndex
{
    public const int SizeParty = 100;
    public const int SizeStored = 80;

    internal static readonly CfruSpeciesMap SpeciesMap = new(ImperiumSpeciesTable.NationalByIndex);
    internal static readonly CfruItemMap ItemMap = new(ImperiumItemTable.ModernByIndex);

    // PKHeX has no Park Ball.
    internal static readonly Ball[] Balls =
    [
        Core.Ball.Strange, Core.Ball.Poke, Core.Ball.Great, Core.Ball.Ultra, Core.Ball.Master, Core.Ball.Premier,
        Core.Ball.Heal, Core.Ball.Net, Core.Ball.Nest, Core.Ball.Dive, Core.Ball.Dusk, Core.Ball.Timer, Core.Ball.Quick,
        Core.Ball.Repeat, Core.Ball.Luxury, Core.Ball.Level, Core.Ball.Lure, Core.Ball.Moon, Core.Ball.Friend,
        Core.Ball.Love, Core.Ball.Fast, Core.Ball.Heavy, Core.Ball.Dream, Core.Ball.Safari, Core.Ball.Sport,
        Core.Ball.None, Core.Ball.Beast, Core.Ball.Cherish,
    ];

    private const byte NoMail = 0xFF;
    private const int ShinyOdds = 36;

    public ImperiumPokemon() : base(SizeParty) => Data[0x55] = NoMail;

    public ImperiumPokemon(Memory<byte> data) : base(data.Length >= SizeParty ? data : Widen(data)) { }

    private static Memory<byte> Widen(Memory<byte> data)
    {
        var party = new byte[SizeParty];
        data.Span.CopyTo(party);
        party[0x55] = NoMail;
        return party;
    }

    public override ImperiumPokemon Clone() => new(Data.ToArray());

    public override int SIZE_PARTY => SizeParty;
    public override int SIZE_STORED => SizeStored;
    public override EntityContext Context => EntityContext.Gen9;
    public override PersonalInfo PersonalInfo => Hack?.Data?.Personal ?? SpeciesMap.Personal[National.Species, Form];

    public override bool Valid { get => true; set { } }

    private ushort Checksum { get => ReadUInt16LittleEndian(Data[0x1C..]); set => WriteUInt16LittleEndian(Data[0x1C..], value); }
    private ushort CalculateChecksum() => Checksums.Add16(Data[0x20..SizeStored]);
    public override bool ChecksumValid => Checksum == CalculateChecksum();
    public override void RefreshChecksum() => Checksum = CalculateChecksum();

    protected override void EncryptStored(Span<byte> stored)
    {
        WriteUInt16LittleEndian(stored[0x1C..], Checksums.Add16(stored[0x20..SizeStored]));
        PokeCrypto.Encrypt3(stored);
    }

    protected override void EncryptParty(Span<byte> party) { }

    // The game reads a nickname's 11th and 12th characters from bitfields in the encrypted data, and two zeroes there as no more characters.
    public override Span<byte> NicknameTrash => Data.Slice(0x08, 10);
    public override Span<byte> OriginalTrainerTrash => Data.Slice(0x14, 7);
    public override int TrashCharCountTrainer => 7;
    public override int TrashCharCountNickname => 10;
    public override int MaxStringLengthTrainer => 7;
    public override int MaxStringLengthNickname => 12;

    private uint ExpWord { get => ReadUInt32LittleEndian(Data[0x24..]); set => WriteUInt32LittleEndian(Data[0x24..], value); }
    private ushort BallWord { get => ReadUInt16LittleEndian(Data[0x2A..]); set => WriteUInt16LittleEndian(Data[0x2A..], value); }
    private byte Nickname11 { get => (byte)(ExpWord >> 21); set => ExpWord = (ExpWord & ~(0xFFu << 21)) | ((uint)value << 21); }
    private byte Nickname12 { get => (byte)(BallWord >> 6); set => BallWord = (ushort)((BallWord & ~(0xFF << 6)) | (value << 6)); }

    private byte[] NicknameBytes(bool forDisplay)
    {
        var name = new byte[12];
        NicknameTrash.CopyTo(name);
        var vanilla = Nickname11 == 0 && Nickname12 == 0;
        (name[10], name[11]) = forDisplay && vanilla ? (StringConverter3.TerminatorByte, StringConverter3.TerminatorByte) : (Nickname11, Nickname12);
        return name;
    }

    public override string Nickname
    {
        get => StringConverter3.GetString(NicknameBytes(forDisplay: true), Language);
        set
        {
            var name = NicknameBytes(forDisplay: false);
            StringConverter3.SetString(name, value, 12, Language, StringConverterOption.None);
            name.AsSpan(0, 10).CopyTo(NicknameTrash);
            (Nickname11, Nickname12) = (name[10], name[11]);
        }
    }

    public override string OriginalTrainerName
    {
        get => StringConverter3.GetString(OriginalTrainerTrash, Language);
        set => StringConverter3.SetString(OriginalTrainerTrash, value, 7, Language, StringConverterOption.None);
    }

    public override ushort MaxMoveID => (ushort)(Move.MAX_COUNT - 1);
    public override ushort MaxSpeciesID => (ushort)(Core.Species.MAX_COUNT - 1);
    public override int MaxItemID => ushort.MaxValue;
    public override int MaxAbilityID => (int)Core.Ability.MAX_COUNT - 1;
    public override int MaxBallID => (int)Core.Ball.Strange;
    public override GameVersion MaxGameID => GameVersion.CXD;
    public override int MaxIV => 31;
    public override int MaxEV => EffortValues.Max255;

    // The hidden nature is stored as an offset from the PID's nature, so a new PID keeps the nature by moving the offset.
    public override uint PID
    {
        get => ReadUInt32LittleEndian(Data);
        set
        {
            var nature = Nature;
            WriteUInt32LittleEndian(Data, value);
            Nature = nature;
        }
    }

    public override uint EncryptionConstant { get => PID; set { } }
    public override uint ID32 { get => ReadUInt32LittleEndian(Data[0x04..]); set => WriteUInt32LittleEndian(Data[0x04..], value); }
    public override ushort TID16 { get => ReadUInt16LittleEndian(Data[0x04..]); set => WriteUInt16LittleEndian(Data[0x04..], value); }
    public override ushort SID16 { get => ReadUInt16LittleEndian(Data[0x06..]); set => WriteUInt16LittleEndian(Data[0x06..], value); }

    public override int Language { get => Data[0x12] & 0x7; set => Data[0x12] = (byte)((Data[0x12] & ~0x7) | (value & 0x7)); }

    private int HiddenNatureModifier { get => Data[0x12] >> 3; set => Data[0x12] = (byte)((Data[0x12] & 0x7) | (value << 3)); }

    public override Nature Nature
    {
        get => (Nature)((PID % 25) ^ (uint)HiddenNatureModifier);
        set => HiddenNatureModifier = (int)((PID % 25) ^ (uint)value);
    }

    internal const byte HasSpeciesFlag = 1 << 1;
    private const byte IsEggFlag = 1 << 2;
    private byte Flags { get => Data[0x13]; set => Data[0x13] = value; }

    private ushort SpeciesWord { get => ReadUInt16LittleEndian(Data[0x20..]); set => WriteUInt16LittleEndian(Data[0x20..], value); }

    public ushort SpeciesIndex
    {
        get => (ushort)(SpeciesWord & 0x7FF);
        set
        {
            SpeciesWord = (ushort)((SpeciesWord & ~0x7FF) | (value & 0x7FF));
            Flags = (byte)(value == 0 ? Flags & ~HasSpeciesFlag : Flags | HasSpeciesFlag);
        }
    }

    private CfruSpecies National => SpeciesMap.ToNational(SpeciesIndex);

    private HackSpecies? Hack => SpeciesMap.ToHack(SpeciesIndex);

    // A species with no national entry has no forms, and writing one would wipe its index.
    private bool HasNational => National.Species != 0;

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
            if (value != Form && HasNational) WriteSpeciesIndex(SpeciesMap.ToIndex(National with { Form = value }) ?? SpeciesMap.ToIndex(new CfruSpecies(Species, value)));
        }
    }

    private ushort ItemWord { get => ReadUInt16LittleEndian(Data[0x22..]); set => WriteUInt16LittleEndian(Data[0x22..], value); }

    public ushort HeldItemIndex { get => (ushort)(ItemWord & 0x3FF); set => ItemWord = (ushort)((ItemWord & ~0x3FF) | (value & 0x3FF)); }

    // Like the species, writing back the item already shown keeps an index outside the item table.
    public override int HeldItem
    {
        get => ItemMap.ToModern(HeldItemIndex);
        set
        {
            if (value != HeldItem && (uint)value <= ushort.MaxValue && ItemMap.ToIndex((ushort)value) is { } index) HeldItemIndex = index;
        }
    }

    public override uint EXP { get => ExpWord & 0x1FFFFF; set => ExpWord = (ExpWord & ~0x1FFFFFu) | (value & 0x1FFFFF); }

    private byte PPUps { get => Data[0x28]; set => Data[0x28] = value; }
    public override int Move1_PPUps { get => (PPUps >> 0) & 3; set => PPUps = (byte)((PPUps & ~(3 << 0)) | ((value & 3) << 0)); }
    public override int Move2_PPUps { get => (PPUps >> 2) & 3; set => PPUps = (byte)((PPUps & ~(3 << 2)) | ((value & 3) << 2)); }
    public override int Move3_PPUps { get => (PPUps >> 4) & 3; set => PPUps = (byte)((PPUps & ~(3 << 4)) | ((value & 3) << 4)); }
    public override int Move4_PPUps { get => (PPUps >> 6) & 3; set => PPUps = (byte)((PPUps & ~(3 << 6)) | ((value & 3) << 6)); }

    public override byte OriginalTrainerFriendship { get => Data[0x29]; set => Data[0x29] = value; }
    public override byte CurrentFriendship { get => OriginalTrainerFriendship; set => OriginalTrainerFriendship = value; }

    public byte BallIndex { get => (byte)(BallWord & 0x3F); set => BallWord = (ushort)((BallWord & ~0x3F) | (value & 0x3F)); }

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

    private Span<byte> MoveWord(int slot) => Data[(0x2C + (slot * 2))..];

    public ushort GetMoveIndex(int slot) => (ushort)(ReadUInt16LittleEndian(MoveWord(slot)) & 0x7FF);

    public void SetMoveIndex(int slot, ushort index) =>
        WriteUInt16LittleEndian(MoveWord(slot), (ushort)((ReadUInt16LittleEndian(MoveWord(slot)) & ~0x7FF) | (index & 0x7FF)));

    private ushort ReadMove(int slot) => ImperiumMoveTable.Map.ToNational(GetMoveIndex(slot));

    // Like the held item, writing back the move already shown keeps an index outside the move table.
    private void WriteMove(int slot, ushort move)
    {
        if (move != ReadMove(slot) && ImperiumMoveTable.Map.ToIndex(move) is { } index) SetMoveIndex(slot, index);
    }

    public override int Move1_PP { get => ReadPP(0); set => WritePP(0, value); }
    public override int Move2_PP { get => ReadPP(1); set => WritePP(1, value); }
    public override int Move3_PP { get => ReadPP(2); set => WritePP(2, value); }
    public override int Move4_PP { get => ReadPP(3); set => WritePP(3, value); }

    private int ReadPP(int slot) => Data[0x34 + slot] & 0x7F;
    private void WritePP(int slot, int pp) => Data[0x34 + slot] = (byte)((Data[0x34 + slot] & 0x80) | (pp & 0x7F));

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
        get => ((IV32 >> 30) & 1) == 1;
        set
        {
            IV32 = (IV32 & ~(1u << 30)) | (value ? 1u << 30 : 0);
            Flags = (byte)(value ? Flags | IsEggFlag : Flags & ~IsEggFlag);
        }
    }

    private uint Ribbons { get => ReadUInt32LittleEndian(Data[0x4C..]); set => WriteUInt32LittleEndian(Data[0x4C..], value); }

    private int AbilitySlot { get => (int)(Ribbons >> 29) & 3; set => Ribbons = (Ribbons & ~(3u << 29)) | ((uint)(value & 3) << 29); }

    public override int AbilityNumber { get => 1 << Math.Min(AbilitySlot, 2); set => AbilitySlot = value switch { 2 => 1, 4 => 2, _ => 0 }; }
    public override int Ability { get => PersonalInfo.GetAbilityAtIndex(Math.Min(AbilitySlot, 2)); set { } }

    public override bool FatefulEncounter { get => Ribbons >> 31 == 1; set => Ribbons = (Ribbons & 0x7FFFFFFF) | (value ? 1u << 31 : 0); }

    // The game rebuilds a withdrawn Pokémon's HP and status from the box header, so party edits keep the header in step, as the game does.
    public override int Status_Condition
    {
        get => ReadInt32LittleEndian(Data[0x50..]);
        set
        {
            WriteInt32LittleEndian(Data[0x50..], value);
            if (value == 0) Data[0x1B] &= 0x0F;
        }
    }

    public override byte Stat_Level { get => Data[0x54]; set => Data[0x54] = value; }
    public override int Stat_HPCurrent { get => ReadUInt16LittleEndian(Data[0x56..]); set { WriteUInt16LittleEndian(Data[0x56..], (ushort)value); KeepHpLostInStep(); } }
    public override int Stat_HPMax { get => ReadUInt16LittleEndian(Data[0x58..]); set { WriteUInt16LittleEndian(Data[0x58..], (ushort)value); KeepHpLostInStep(); } }
    public override int Stat_ATK { get => ReadUInt16LittleEndian(Data[0x5A..]); set => WriteUInt16LittleEndian(Data[0x5A..], (ushort)value); }
    public override int Stat_DEF { get => ReadUInt16LittleEndian(Data[0x5C..]); set => WriteUInt16LittleEndian(Data[0x5C..], (ushort)value); }
    public override int Stat_SPE { get => ReadUInt16LittleEndian(Data[0x5E..]); set => WriteUInt16LittleEndian(Data[0x5E..], (ushort)value); }
    public override int Stat_SPA { get => ReadUInt16LittleEndian(Data[0x60..]); set => WriteUInt16LittleEndian(Data[0x60..], (ushort)value); }
    public override int Stat_SPD { get => ReadUInt16LittleEndian(Data[0x62..]); set => WriteUInt16LittleEndian(Data[0x62..], (ushort)value); }

    private ushort HealthWord { get => ReadUInt16LittleEndian(Data[0x1E..]); set => WriteUInt16LittleEndian(Data[0x1E..], value); }

    private void KeepHpLostInStep() =>
        HealthWord = (ushort)((HealthWord & ~0x3FFF) | Math.Clamp(Stat_HPMax - Stat_HPCurrent, 0, 0x3FFF));

    private bool ShinyModifier => ((HealthWord >> 14) & 1) == 1;

    public override uint PSV => ((PID >> 16) ^ (PID & 0xFFFF)) >> 3;
    public override uint TSV => (uint)(TID16 ^ SID16) >> 3;
    public override bool IsShiny => (ShinyXor < ShinyOdds) ^ ShinyModifier;

    public override byte Gender { get => EntityGender.GetFromPIDAndRatio(PID, PersonalInfo.Gender); set { } }
    public override bool IsNicknamed { get => Hack is { } hack ? Nickname != hack.DefaultNickname : SpeciesName.IsNicknamed(National.Species, Nickname, Language); set { } }
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
