using System.Collections.Frozen;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Abstractions;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Everywhere.RomHacks.Expansion.HeartAndSoul;

// Heart & Soul keeps Emerald's two slots of 14 sectors, so it's told apart by the magic its SaveBlock1 carries.
public sealed class HeartAndSoulFormat : ISaveFormat
{
    private const int SectorSize = 0x1000;
    private const int SectorsPerSlot = 14;
    private const int SectorData = 3968;
    private const uint Signature = 0x08012025;
    private const int MagicOffset = 0x10E0;
    private const uint Magic = 0xE8F828BC;

    public string Id => "heart-and-soul";
    public string Name => "Pokémon Heart & Soul";
    public GameVersion BaseGame => GameVersion.E;
    public IReadOnlySet<Capability> Capabilities => FrozenSet<Capability>.Empty;

    public SaveFormatMatch Detect(ReadOnlySpan<byte> data)
    {
        if (!Gen3SaveSize.IsSupported(data.Length)) return SaveFormatMatch.No;

        Span<int> sectors = stackalloc int[SectorsPerSlot];
        if (!TryReadNewerSlot(data, sectors)) return SaveFormatMatch.No;

        var magic = data[(sectors[1 + (MagicOffset / SectorData)] + (MagicOffset % SectorData))..];
        return ReadUInt32LittleEndian(magic) == Magic ? SaveFormatMatch.Certain : SaveFormatMatch.No;
    }

    // Refused until its save is built.
    public SaveFile Load(byte[] data) => throw new GameNotLoadedException();

    // A slot with a missing or repeated sector is skipped, so a save written once still has its one slot.
    private static bool TryReadNewerSlot(ReadOnlySpan<byte> data, Span<int> sectors)
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
}
