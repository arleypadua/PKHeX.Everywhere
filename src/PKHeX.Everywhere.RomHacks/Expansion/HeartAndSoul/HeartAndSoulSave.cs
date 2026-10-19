using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Cfru;
using PKHeX.Facade;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Everywhere.RomHacks.Expansion.HeartAndSoul;

/// <summary>
/// A Pokémon Heart &amp; Soul 2.0 save: two slots of 14 sectors, of which it reads and writes the newer.
/// It copies the Pokémon storage out of its sectors when loading, writes it back on export, and leaves every other byte as it was.
/// </summary>
public sealed class HeartAndSoulSave : SaveFile
{
    private const int SectorSize = 0x1000;
    internal const int SectorsPerSlot = 14;
    private const int SectorData = 3968;
    private const uint Signature = 0x08012025;
    private const int SaveBlock2 = 0;
    private const int SaveBlock1 = 1;
    private const int Storage = 5;
    private const int SlotsPerBox = 30;
    private const int Boxes = 14;
    private const int BoxesOffset = 0x4;

    // Each sector's data bytes past these, up to 4084, hold SaveBlock3, which the checksum leaves out.
    private static readonly int[] ChecksumLengths =
        [3892, SectorData, SectorData, SectorData, 3856, .. Enumerable.Repeat(SectorData, 8), 2400];

    private readonly int[] _sectors = new int[SectorsPerSlot];
    private readonly byte[] _storage = new byte[ChecksumLengths[Storage..].Sum()];

    public HeartAndSoulSave(byte[] data) : base(data)
    {
        if (!TryReadNewerSlot(data, _sectors)) throw new GameNotLoadedException();

        CopyStorage(toSave: false);

        Party = 0;
        Box = 0;
    }

    internal ushort SaveVersion => ReadUInt16LittleEndian(Sector(SaveBlock1));

    // A slot with a missing or repeated sector is skipped, so a save written once still has its one slot.
    internal static bool TryReadNewerSlot(ReadOnlySpan<byte> data, Span<int> sectors)
    {
        Span<int> candidate = stackalloc int[SectorsPerSlot];
        var newestCounter = -1L;
        for (var slot = 0; slot < 2; slot++)
        {
            if (!TryReadSlot(data, slot, candidate)) continue;

            long counter = ReadUInt32LittleEndian(data[(candidate[0] + 0xFFC)..]);
            if (counter <= newestCounter) continue;

            newestCounter = counter;
            candidate.CopyTo(sectors);
        }

        return newestCounter >= 0;
    }

    private static bool TryReadSlot(ReadOnlySpan<byte> data, int slot, Span<int> sectors)
    {
        var seen = 0;
        for (var index = 0; index < SectorsPerSlot; index++)
        {
            var offset = ((slot * SectorsPerSlot) + index) * SectorSize;
            var sector = data.Slice(offset, SectorSize);
            int id = ReadUInt16LittleEndian(sector[0xFF4..]);
            if (id >= SectorsPerSlot || (seen & (1 << id)) != 0) return false;
            if (ReadUInt32LittleEndian(sector[0xFF8..]) != Signature) return false;
            sectors[id] = offset;
            seen |= 1 << id;
        }

        return true;
    }

    internal static int SaveBlock1Offset(ReadOnlySpan<int> sectors, int offset) =>
        sectors[SaveBlock1 + (offset / SectorData)] + (offset % SectorData);

    private Span<byte> Sector(int id) => Data.Slice(_sectors[id], SectorSize);

    private void CopyStorage(bool toSave)
    {
        var position = 0;
        for (var id = Storage; id < SectorsPerSlot; id++)
        {
            var chunk = Sector(id)[..ChecksumLengths[id]];
            var storage = _storage.AsSpan(position, chunk.Length);
            if (toSave) storage.CopyTo(chunk);
            else chunk.CopyTo(storage);
            position += chunk.Length;
        }
    }

    protected override Memory<byte> GetFinalData()
    {
        CopyStorage(toSave: true);
        SetChecksums();
        return Data.ToArray();
    }

    private ushort Checksum(int id) => Checksums.CheckSum32(Sector(id)[..ChecksumLengths[id]]);

    protected override void SetChecksums()
    {
        for (var id = 0; id < SectorsPerSlot; id++)
            WriteUInt16LittleEndian(Sector(id)[0xFF6..], Checksum(id));
    }

    public override bool ChecksumsValid => Enumerable.Range(0, SectorsPerSlot)
        .All(id => ReadUInt16LittleEndian(Sector(id)[0xFF6..]) == Checksum(id));

    public override string ChecksumInfo => ChecksumsValid ? "Checksums are valid." : "Some sector checksums are invalid.";

    protected override string ShortSummary => $"{OT} ({Version})";
    public override string Extension => ".sav";
    protected override HeartAndSoulSave CloneInternal() => new(GetFinalData().ToArray());

    public override GameVersion Version { get => GameVersion.E; set { } }
    public override byte Generation => 9;
    public override EntityContext Context => EntityContext.Gen9;
    public override int Language { get => (int)LanguageID.English; set { } }

    public override IPersonalTable Personal => SpeciesMap.Personal;
    internal CfruSpeciesMap SpeciesMap => HeartAndSoulPokemon.Map;
    internal CfruItemMap ItemMap => HeartAndSoulPokemon.Items;

    public override Type PKMType => typeof(HeartAndSoulPokemon);
    public override HeartAndSoulPokemon BlankPKM => new();
    protected override HeartAndSoulPokemon GetPKM(Memory<byte> data) => new(data);
    protected override void DecryptPKM(Span<byte> data) => PokeCrypto.Decrypt3(data);

    public override int MaxStringLengthTrainer => 7;
    public override int MaxStringLengthNickname => 12;
    public override ushort MaxMoveID => BlankPKM.MaxMoveID;
    public override ushort MaxSpeciesID => BlankPKM.MaxSpeciesID;
    public override int MaxAbilityID => BlankPKM.MaxAbilityID;
    public override int MaxItemID => BlankPKM.MaxItemID;
    public override int MaxBallID => BlankPKM.MaxBallID;
    public override GameVersion MaxGameID => BlankPKM.MaxGameID;
    public override int MaxEV => EffortValues.Max255;
    public override ReadOnlySpan<ushort> HeldItems => ItemMap.HeldItems;

    public override string GetString(ReadOnlySpan<byte> data) => StringConverter3.GetString(data, false);
    public override int LoadString(ReadOnlySpan<byte> data, Span<char> text) => StringConverter3.LoadString(data, text, false);
    public override int SetString(Span<byte> destBuffer, ReadOnlySpan<char> value, int maxLength, StringConverterOption option) =>
        StringConverter3.SetString(destBuffer, value, maxLength, false, option);

    public override string OT
    {
        get => GetString(Sector(SaveBlock2)[..7]);
        set => SetString(Sector(SaveBlock2)[..7], value, 7, StringConverterOption.ClearFF);
    }

    public override byte Gender { get => Sector(SaveBlock2)[0x08]; set => Sector(SaveBlock2)[0x08] = value; }
    public override uint ID32 { get => ReadUInt32LittleEndian(Sector(SaveBlock2)[0x0A..]); set => WriteUInt32LittleEndian(Sector(SaveBlock2)[0x0A..], value); }
    public override ushort TID16 { get => ReadUInt16LittleEndian(Sector(SaveBlock2)[0x0A..]); set => WriteUInt16LittleEndian(Sector(SaveBlock2)[0x0A..], value); }
    public override ushort SID16 { get => ReadUInt16LittleEndian(Sector(SaveBlock2)[0x0C..]); set => WriteUInt16LittleEndian(Sector(SaveBlock2)[0x0C..], value); }

    internal uint EncryptionKey => ReadUInt32LittleEndian(Sector(SaveBlock2)[0xAC..]);
    public override uint Money { get => ReadUInt32LittleEndian(Sector(SaveBlock1)[0x494..]) ^ EncryptionKey; set => WriteUInt32LittleEndian(Sector(SaveBlock1)[0x494..], value ^ EncryptionKey); }

    // SaveBlock1 starts with the save version, so the party sits 4 bytes later than Emerald's.
    public override int PartyCount { get => Sector(SaveBlock1)[0x238]; protected set => Sector(SaveBlock1)[0x238] = (byte)value; }
    protected override Span<byte> PartyBuffer => Sector(SaveBlock1).Slice(0x23C, 6 * ExpansionPokemon.SizeParty);
    public override int GetPartyOffset(int slot) => slot * ExpansionPokemon.SizeParty;

    public override int BoxCount => Boxes;
    public override int BoxSlotCount => SlotsPerBox;
    protected override Span<byte> BoxBuffer => _storage;
    public override int GetBoxOffset(int box) => BoxesOffset + (box * SlotsPerBox * ExpansionPokemon.SizeStored);
    public override int CurrentBox { get => _storage[0]; set => _storage[0] = (byte)value; }
    public override bool IsPKMPresent(ReadOnlySpan<byte> data) => (data[0x13] & ExpansionPokemon.HasSpeciesFlag) != 0;

    public override int SIZE_STORED => ExpansionPokemon.SizeStored;
    public override int SIZE_PARTY => ExpansionPokemon.SizeParty;

    // Party stats computed for a blank would land in the empty slot, which the game leaves zeroed.
    // A species without data keeps the stats it has, since they can't be computed without its base stats.
    protected override void SetPartyValues(PKM pk, bool isParty)
    {
        if (SpeciesMap.Contains(pk.Species)) base.SetPartyValues(pk, isParty);
    }
}
