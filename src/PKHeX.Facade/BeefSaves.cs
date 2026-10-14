using PKHeX.Core;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade;

internal static class BeefSaves
{
    private const uint FooterMagic = 0x42454546;
    private const int FooterSize = 0x200;
    private const uint PokedexMagic = 0x2F120F17;
    private const int PokedexBlock = 6;

    // PKHeX's blank Gen 6 and 7 saves have no block table in their footer, so they aren't recognized once exported.
    // The table gets each block's length and id as a loaded save has them, and the checksums are written when the save is.
    // A blank Gen 7 Pokédex also lacks the magic a loaded one starts with.
    public static SaveFile Sealed(SaveFile save)
    {
        if (save is not (SAV6 or SAV7)) return save;

        var blocks = ((SAV_BEEF)save).AllBlocks;
        var footer = save.Data[^FooterSize..];
        WriteUInt32LittleEndian(footer[0x10..], FooterMagic);
        foreach (var block in blocks)
        {
            var entry = footer[(0x14 + (int)block.ID * 8)..];
            WriteUInt32LittleEndian(entry, (uint)block.Length);
            WriteUInt16LittleEndian(entry[4..], (ushort)block.ID);
        }

        if (save is SAV7) WriteUInt32LittleEndian(save.Data[blocks[PokedexBlock].Offset..], PokedexMagic);
        return save;
    }
}
