using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Cfru;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Everywhere.RomHacks.Expansion.Imperium;

/// <summary>
/// An Emerald Imperium 1.x save: one slot of 28 sectors, found by their ids wherever the slot has rotated to.
/// It copies the Pokémon storage out of its sectors when loading, writes it back on export, and leaves every other byte as it was.
/// </summary>
public sealed class ImperiumSave : SaveFile
{
    private const int SectorSize = 0x1000;
    private const int SectorCount = 28;
    private const int SaveBlock2 = 0;
    private const int SaveBlock1 = 1;
    private const int Storage = 17;
    private const int SlotsPerBox = 30;
    private const int Boxes = 14;
    private const int BoxesOffset = 0x4;

    // How many of each sector's 4084 data bytes the game uses, and its checksum covers.
    internal static readonly int[] ChunkLengths =
        [2572, 4084, 4084, 4084, 2152, .. new int[12], .. Enumerable.Repeat(4084, 8), 1472, 0, 0];

    private readonly int[] _sectors = new int[SectorCount];
    private readonly byte[] _storage = new byte[ChunkLengths[Storage..].Sum()];

    public ImperiumSave(byte[] data) : base(data)
    {
        for (var sector = 0; sector < SectorCount; sector++)
            _sectors[ReadUInt16LittleEndian(data.AsSpan((sector * SectorSize) + 0xFF4))] = sector * SectorSize;

        CopyStorage(toSave: false);

        Party = 0;
        Box = 0;
    }

    private Span<byte> Sector(int id) => Data.Slice(_sectors[id], SectorSize);

    private void CopyStorage(bool toSave)
    {
        var position = 0;
        for (var id = Storage; id < SectorCount; id++)
        {
            var chunk = Sector(id)[..ChunkLengths[id]];
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

    private ushort Checksum(int id) => Checksums.CheckSum32(Sector(id)[..ChunkLengths[id]]);

    protected override void SetChecksums()
    {
        for (var id = 0; id < SectorCount; id++)
            WriteUInt16LittleEndian(Sector(id)[0xFF6..], Checksum(id));
    }

    public override bool ChecksumsValid => Enumerable.Range(0, SectorCount)
        .All(id => ReadUInt16LittleEndian(Sector(id)[0xFF6..]) == Checksum(id));

    public override string ChecksumInfo => ChecksumsValid ? "Checksums are valid." : "Some sector checksums are invalid.";

    protected override string ShortSummary => $"{OT} ({Version})";
    public override string Extension => ".sav";
    protected override ImperiumSave CloneInternal() => new(GetFinalData().ToArray());

    public override GameVersion Version { get => GameVersion.E; set { } }
    public override byte Generation => 9;
    public override EntityContext Context => EntityContext.Gen9;
    public override int Language { get => (int)LanguageID.English; set { } }

    public override IPersonalTable Personal => SpeciesMap.Personal;
    internal CfruSpeciesMap SpeciesMap => ImperiumPokemon.SpeciesMap;
    internal CfruItemMap ItemMap => ImperiumPokemon.ItemMap;

    public override Type PKMType => typeof(ImperiumPokemon);
    public override ImperiumPokemon BlankPKM => new();
    protected override ImperiumPokemon GetPKM(Memory<byte> data) => new(data);
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

    public override int PartyCount { get => Sector(SaveBlock1)[0x234]; protected set => Sector(SaveBlock1)[0x234] = (byte)value; }
    protected override Span<byte> PartyBuffer => Sector(SaveBlock1).Slice(0x238, 6 * ImperiumPokemon.SizeParty);
    public override int GetPartyOffset(int slot) => slot * ImperiumPokemon.SizeParty;

    public override int BoxCount => Boxes;
    public override int BoxSlotCount => SlotsPerBox;
    protected override Span<byte> BoxBuffer => _storage;
    public override int GetBoxOffset(int box) => BoxesOffset + (box * SlotsPerBox * ImperiumPokemon.SizeStored);
    public override int CurrentBox { get => _storage[0]; set => _storage[0] = (byte)value; }
    public override bool IsPKMPresent(ReadOnlySpan<byte> data) => (data[0x13] & 2) != 0;

    public override int SIZE_STORED => ImperiumPokemon.SizeStored;
    public override int SIZE_PARTY => ImperiumPokemon.SizeParty;

    // Party stats computed for a blank would land in the empty slot, which the game leaves zeroed.
    // A species without data keeps the stats it has, since they can't be computed without its base stats.
    protected override void SetPartyValues(PKM pk, bool isParty)
    {
        if (SpeciesMap.Contains(pk.Species)) base.SetPartyValues(pk, isParty);
    }
}
