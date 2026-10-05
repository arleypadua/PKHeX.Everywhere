using PKHeX.Core;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Everywhere.RomHacks.Cfru;

/// <summary>
/// A save from a hack built on the Complete FireRed Upgrade. It copies the box stream into one buffer when loading,
/// writes it back on export, and leaves every other byte as it was.
/// </summary>
public abstract class CfruSave : SaveFile
{
    private const int SectorSize = 0x1000;
    private const int SectorCount = 14;
    private const int SlotSize = SectorCount * SectorSize;
    private const int SlotsPerBox = 30;
    private const int Boxes = 25;

    public static readonly int[] FileSizes = [0x20000, 0x20010];

    // Sectors 30 and 31 sit outside both save slots, have no footer and are addressed physically.
    private static readonly (int Block, int Start, int End)[] BoxStream =
    [
        (5, 0x4, 0xFF0), (6, 0, 0xFF0), (7, 0, 0xFF0), (8, 0, 0xFF0), (9, 0, 0xFF0), (10, 0, 0xFF0), (11, 0, 0xFF0),
        (12, 0, 0xFF0), (13, 0, 0x1A8),
        (30, 0xB0C, 0xFF0), (31, 0, 0xF80),
        (2, 0xF18, 0xFF0), (3, 0, 0xCC0),
        (0, 0xB0, 0x77C),
    ];

    private readonly int[] _blocks = new int[SectorCount];
    private readonly byte[] _boxes = new byte[Boxes * SlotsPerBox * CfruPokemon.SizeBoxed];

    protected CfruSave(byte[] data, IReadOnlyCollection<uint> signatures) : base(data)
    {
        var slot = FindActiveSlot(data, signatures)
            ?? throw new ArgumentException("The data has no valid CFRU save slot.", nameof(data));
        for (var block = 0; block < SectorCount; block++)
            _blocks[block] = FindBlock(data, slot, block);

        CopyBoxStream(toSave: false);

        Party = 0;
        Box = 0;
    }

    public static int? FindActiveSlot(ReadOnlySpan<byte> data, IReadOnlyCollection<uint> signatures)
    {
        if (!FileSizes.Contains(data.Length)) return null;

        int? active = null;
        uint activeIndex = 0;
        foreach (var slot in (int[])[0, SlotSize])
        {
            if (!IsValidSlot(data[slot..(slot + SlotSize)], signatures)) continue;

            var index = ReadUInt32LittleEndian(data[(slot + 0xFFC)..]);
            if (active is not null && !IsNewer(index, activeIndex)) continue;
            active = slot;
            activeIndex = index;
        }

        return active;
    }

    public static int FindBlock(ReadOnlySpan<byte> data, int slot, int block)
    {
        for (var sector = 0; sector < SectorCount; sector++)
        {
            var offset = slot + (sector * SectorSize);
            if (ReadUInt16LittleEndian(data[(offset + 0xFF4)..]) == block) return offset;
        }

        throw new ArgumentException($"The save slot has no block {block}.", nameof(data));
    }

    // The save counter wraps from uint.MaxValue to 0.
    private static bool IsNewer(uint index, uint than) => index == 0 && than == uint.MaxValue
        || (index > than && !(index == uint.MaxValue && than == 0));

    private static bool IsValidSlot(ReadOnlySpan<byte> slot, IReadOnlyCollection<uint> signatures)
    {
        var seen = 0;
        for (var sector = 0; sector < SectorCount; sector++)
        {
            var footer = slot[((sector * SectorSize) + 0xFF4)..];
            int id = ReadUInt16LittleEndian(footer);
            if (id >= SectorCount || !signatures.Contains(ReadUInt32LittleEndian(footer[4..]))) return false;
            seen |= 1 << id;
        }

        return seen == (1 << SectorCount) - 1;
    }

    private int BlockOffset(int block) => block < SectorCount ? _blocks[block] : block * SectorSize;

    private Span<byte> Block(int block) => Data.Slice(BlockOffset(block), SectorSize);

    private void CopyBoxStream(bool toSave)
    {
        var position = 0;
        foreach (var (block, start, end) in BoxStream)
        {
            var region = Data.Slice(BlockOffset(block) + start, end - start);
            var boxes = _boxes.AsSpan(position, region.Length);
            if (toSave) boxes.CopyTo(region);
            else region.CopyTo(boxes);
            position += region.Length;
        }
    }

    protected override Memory<byte> GetFinalData()
    {
        CopyBoxStream(toSave: true);
        SetChecksums();
        return Data.ToArray();
    }

    private static int ChecksumLength(int block) => block switch
    {
        0 => 0xF24,
        4 => 0xD98,
        13 => 0x450,
        _ => 0xFF0,
    };

    private ushort Checksum(int block) => Checksums.CheckSum32(Block(block)[..ChecksumLength(block)]);

    protected override void SetChecksums()
    {
        for (var block = 0; block < SectorCount; block++)
            WriteUInt16LittleEndian(Block(block)[0xFF6..], Checksum(block));
    }

    public override bool ChecksumsValid => Enumerable.Range(0, SectorCount)
        .All(block => ReadUInt16LittleEndian(Block(block)[0xFF6..]) == Checksum(block));

    public override string ChecksumInfo => ChecksumsValid ? "Checksums are valid." : "Some block checksums are invalid.";

    protected override string ShortSummary => $"{OT} ({Version})";
    public override string Extension => ".sav";

    public override GameVersion Version { get => GameVersion.FR; set { } }
    public override byte Generation => 9;
    public override EntityContext Context => EntityContext.Gen9;
    public override int Language { get => (int)LanguageID.English; set { } }

    public override IPersonalTable Personal => SpeciesMap.Personal;
    internal abstract CfruSpeciesMap SpeciesMap { get; }
    internal abstract CfruItemMap ItemMap { get; }

    public override int MaxStringLengthTrainer => 7;
    public override int MaxStringLengthNickname => 10;
    public override ushort MaxMoveID => BlankPKM.MaxMoveID;
    public override ushort MaxSpeciesID => BlankPKM.MaxSpeciesID;
    public override int MaxAbilityID => BlankPKM.MaxAbilityID;
    public override int MaxItemID => BlankPKM.MaxItemID;
    public override int MaxBallID => BlankPKM.MaxBallID;
    public override GameVersion MaxGameID => BlankPKM.MaxGameID;
    public override int MaxEV => EffortValues.Max255;
    public override int MaxMoney => 999999;
    public override ReadOnlySpan<ushort> HeldItems => ItemMap.HeldItems;
    public override CfruBag Inventory => new(Data, BlockOffset(13), BlockOffset(30), ItemMap);

    public override string GetString(ReadOnlySpan<byte> data) => StringConverter3.GetString(data, false);
    public override int LoadString(ReadOnlySpan<byte> data, Span<char> text) => StringConverter3.LoadString(data, text, false);
    public override int SetString(Span<byte> destBuffer, ReadOnlySpan<char> value, int maxLength, StringConverterOption option) =>
        StringConverter3.SetString(destBuffer, value, maxLength, false, option);

    public override string OT
    {
        get => GetString(Block(0)[..7]);
        set => SetString(Block(0)[..7], value, 7, StringConverterOption.ClearFF);
    }

    public override byte Gender { get => Block(0)[0x08]; set => Block(0)[0x08] = value; }
    public override uint ID32 { get => ReadUInt32LittleEndian(Block(0)[0x0A..]); set => WriteUInt32LittleEndian(Block(0)[0x0A..], value); }
    public override ushort TID16 { get => ReadUInt16LittleEndian(Block(0)[0x0A..]); set => WriteUInt16LittleEndian(Block(0)[0x0A..], value); }
    public override ushort SID16 { get => ReadUInt16LittleEndian(Block(0)[0x0C..]); set => WriteUInt16LittleEndian(Block(0)[0x0C..], value); }

    public override int PlayedHours { get => ReadUInt16LittleEndian(Block(0)[0x0E..]); set => WriteUInt16LittleEndian(Block(0)[0x0E..], (ushort)value); }
    public override int PlayedMinutes { get => Block(0)[0x10]; set => Block(0)[0x10] = (byte)value; }
    public override int PlayedSeconds { get => Block(0)[0x11]; set => Block(0)[0x11] = (byte)value; }

    public override uint Money { get => ReadUInt32LittleEndian(Block(1)[0x290..]); set => WriteUInt32LittleEndian(Block(1)[0x290..], value); }

    public override int PartyCount { get => Block(1)[0x34]; protected set => Block(1)[0x34] = (byte)value; }
    protected override Span<byte> PartyBuffer => Block(1).Slice(0x38, 6 * CfruPokemon.SizeParty);
    public override int GetPartyOffset(int slot) => slot * CfruPokemon.SizeParty;

    public override int BoxCount => Boxes;
    public override int BoxSlotCount => SlotsPerBox;
    public override int SIZE_BOXSLOT => CfruPokemon.SizeBoxed;
    protected override Span<byte> BoxBuffer => _boxes;
    public override int GetBoxOffset(int box) => box * SlotsPerBox * CfruPokemon.SizeBoxed;
    public override int CurrentBox { get => Block(5)[0]; set => Block(5)[0] = (byte)value; }
    public override bool IsPKMPresent(ReadOnlySpan<byte> data) => ReadUInt16LittleEndian(data[0x1C..]) != 0;

    public override int SIZE_STORED => CfruPokemon.SizeStored;
    public override int SIZE_PARTY => CfruPokemon.SizeParty;
    protected override void DecryptPKM(Span<byte> data) { }

    // Party stats computed for a blank would land in the empty slot, which the game leaves zeroed.
    // An unknown species keeps the stats it has, since they can't be computed without its base stats.
    protected override void SetPartyValues(PKM pk, bool isParty)
    {
        if (SpeciesMap.Contains(pk.Species)) base.SetPartyValues(pk, isParty);
    }

    // PKHeX counts the party by species, which reads as 0 for an index outside the species table.
    public override void SetPartySlotAtIndex(PKM pk, int index, EntityImportSettings settings = default)
    {
        base.SetPartySlotAtIndex(pk, index, settings);
        if (((CfruPokemon)pk).SpeciesIndex != 0 && PartyCount <= index) PartyCount = index + 1;
    }

    protected override PKM GetBoxSlot(int offset)
    {
        var party = new byte[CfruPokemon.SizeParty];
        CfruPokemon.Expand(BoxBuffer.Slice(offset, CfruPokemon.SizeBoxed), party);
        var pokemon = GetPKM(party);
        pokemon.HealPP();
        return pokemon;
    }

    protected override void WriteSlotBox(PKM pk, Span<byte> data) => ((CfruPokemon)pk).WriteBoxed(data);
}
