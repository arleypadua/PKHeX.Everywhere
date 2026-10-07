using System.Collections.Frozen;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Abstractions;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Everywhere.RomHacks.Legacy;

// Emerald Legacy keeps Emerald's sector signature and ids, and PKHeX checksums a fixed 0xF80 per sector
// rather than the length the game uses, so a Legacy save is byte-plausible as Emerald and can only ever
// be a possible match. It is told apart by where its SaveBlock1 puts things: the Pokédex keeps three
// copies of the seen flags that the game writes together, so they agree at Legacy's offsets and
// disagree at Emerald's. Pouch counts at Legacy's offsets corroborate that, for a save whose dex
// regions are still empty.
public sealed class EmeraldLegacyFormat : ISaveFormat
{
    private const int SectorSize = 0x1000;
    private const int SectorsPerSlot = 14;
    private const int SectorUsed = 0xF80;
    private const uint Signature = 0x08012025;
    private const int DexFlagBytes = 0x34;

    private static readonly int[] FileSizes = [0x20000, 0x20010];

    public string Id => "emerald-legacy";
    public string Name => "Pokémon Emerald Legacy";
    public GameVersion BaseGame => GameVersion.E;
    // Legacy keeps Emerald's event system, only renumbered, so the flag and work editor works against the
    // offsets EmeraldLegacyBlockLarge supplies. Everything else PKHeX would infer from the base game is still off.
    public IReadOnlySet<Capability> Capabilities { get; } = new[] { Capability.Events }.ToFrozenSet();

    public EventLabels? EventLabels { get; } =
        new(EmeraldLegacyEventLabels.Flags, EmeraldLegacyEventLabels.Work);

    public SaveFormatMatch Detect(ReadOnlySpan<byte> data)
    {
        if (!FileSizes.Contains(data.Length)) return SaveFormatMatch.No;
        if (!TryReadBlocks(data, out var small, out var large)) return SaveFormatMatch.No;

        // Emerald keeps seen1 at 0x988 and seen2 at 0x3B24; Legacy's bag moves the first to 0xAF0 and
        // its longer trainer flag range moves the second to 0x3B34.
        if (!SeenCopiesAgree(large, 0xAF0, 0x3B34)) return SaveFormatMatch.No;
        if (SeenCopiesAgree(large, 0x988, 0x3B24)) return SaveFormatMatch.No;

        var key = (ushort)ReadUInt32LittleEndian(small[0xAC..]);
        if (!PouchIsPlausible(large, 0x740, 30, 1, key, out var keyItems)) return SaveFormatMatch.No;
        if (!PouchIsPlausible(large, 0x8F8, 46, 999, key, out _)) return SaveFormatMatch.No;
        if (!PouchIsPlausible(large, 0x7F8, 64, 99, key, out _)) return SaveFormatMatch.No;

        // A save with nothing in its key items has told us nothing the empty dex regions didn't.
        return keyItems == 0 ? SaveFormatMatch.No : SaveFormatMatch.Possible;
    }

    public SaveFile Load(byte[] data) => new EmeraldLegacySave(data);

    private static bool SeenCopiesAgree(ReadOnlySpan<byte> large, int first, int second) =>
        large.Slice(first, DexFlagBytes).SequenceEqual(large.Slice(second, DexFlagBytes));

    // Counts are stored XORed with the save's security key, item ids are not.
    private static bool PouchIsPlausible(ReadOnlySpan<byte> large, int offset, int slots, int max, ushort key, out int used)
    {
        used = 0;
        for (var slot = 0; slot < slots; slot++)
        {
            var entry = large[(offset + (slot * 4))..];
            if (ReadUInt16LittleEndian(entry) == 0) continue;
            var count = ReadUInt16LittleEndian(entry[2..]) ^ key;
            if (count is 0 || count > max) return false;
            used++;
        }

        return true;
    }

    // The active slot's sectors carry their own ids, so they are gathered by id rather than by position.
    private static bool TryReadBlocks(ReadOnlySpan<byte> data, out byte[] small, out byte[] large)
    {
        small = [];
        large = [];
        var best = -1;
        Span<int> bestOffsets = stackalloc int[SectorsPerSlot];
        Span<int> offsets = stackalloc int[SectorsPerSlot];

        for (var slot = 0; slot < 2; slot++)
        {
            var seen = 0;
            for (var index = 0; index < SectorsPerSlot; index++)
            {
                var offset = (slot * SectorsPerSlot * SectorSize) + (index * SectorSize);
                if (offset + SectorSize > data.Length) return false;

                var sector = data.Slice(offset, SectorSize);
                int id = ReadUInt16LittleEndian(sector[0xFF4..]);
                if (id >= SectorsPerSlot || (seen & (1 << id)) != 0) { seen = 0; break; }
                if (ReadUInt32LittleEndian(sector[0xFF8..]) != Signature) { seen = 0; break; }

                offsets[id] = offset;
                seen |= 1 << id;
            }

            if (seen != (1 << SectorsPerSlot) - 1) continue;

            var counter = (int)ReadUInt32LittleEndian(data.Slice(offsets[0], SectorSize)[0xFFC..]);
            if (counter <= best) continue;

            best = counter;
            offsets.CopyTo(bestOffsets);
        }

        if (best < 0) return false;

        small = data.Slice(bestOffsets[0], SectorUsed).ToArray();
        var block = new byte[4 * SectorUsed];
        for (var id = 1; id <= 4; id++)
            data.Slice(bestOffsets[id], SectorUsed).CopyTo(block.AsSpan((id - 1) * SectorUsed));
        large = block;

        return true;
    }
}
