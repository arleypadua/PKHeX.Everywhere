using PKHeX.Core;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade;

internal static class GameCubeSaves
{
    // PKHeX saves these games under versions it counts as origins rather than saved versions, and names none of them.
    public static readonly IReadOnlyDictionary<GameVersion, string> Names = new Dictionary<GameVersion, string>
    {
        [GameVersion.COLO] = "Colosseum",
        [GameVersion.XD] = "XD",
        [GameVersion.RSBOX] = "Box Ruby & Sapphire",
    };

    public static SaveFile? Blank(GameVersion version) => version switch
    {
        GameVersion.COLO => Colosseum(),
        GameVersion.XD => XD(),
        GameVersion.RSBOX => BlankSaveFile.Get(SaveFileType.RSBox, version),
        _ => null,
    };

    private const uint SlotMagic = 0x101;

    // PKHeX's blank Colosseum constructor never sizes its data and throws, so the blank reads an empty save and is set up as that constructor would.
    // Each slot gets the magic PKHeX looks for when it loads a save, or the exported blank wouldn't load back.
    private static SAV3Colosseum Colosseum()
    {
        var data = new byte[SaveUtil.SIZE_G3COLO];
        for (var slot = 0; slot < ColoCrypto.SLOT_COUNT; slot++)
            WriteUInt32LittleEndian(ColoCrypto.GetSlot(data, slot).Span, SlotMagic);

        var save = new SAV3Colosseum(data, decrypt: false)
        {
            CurrentRegion = GCRegion.NTSC_U,
            OriginalRegion = GCRegion.NTSC_U,
            Language = (int)LanguageID.English,
        };
        save.ClearBoxes();
        return save;
    }

    // As laid out by PKHeX's blank XD constructor, which keeps them in memory only and so makes a save that can't be exported.
    private static ReadOnlySpan<ushort> XDBlockLengths => [0x88, 0x978, 0xBC50, 0x4A, 0xCC, 0x1774, 0x364, 0x2400, 0x6B4, 0xB, 0x400, 0x800, 0x1000, 0x2000, 0x2298, 0xC800];
    private static ReadOnlySpan<int> XDBlockOffsets => [0x0, 0x90, 0xA10, 0xC660, 0xC6B0, 0xC780, 0xDF00, 0xE270, 0x10670, 0x10D30, 0x10D40, 0x11140, 0x11940, 0x12940, 0x14940, 0x16BE0];

    private static SAV3XD XD()
    {
        var data = new byte[XDCrypto.SAVE_SIZE];
        for (var slot = 0; slot < 2; slot++)
            WriteUInt32LittleEndian(XDCrypto.GetSlot(data, slot).Span, SlotMagic);

        var first = XDCrypto.GetSlot(data, 0).Span;
        for (var block = 0; block < XDBlockLengths.Length; block++)
        {
            WriteUInt16BigEndian(first[(0x20 + 2 * block)..], XDBlockLengths[block]);
            WriteUInt16BigEndian(first[(0x40 + 4 * block)..], (ushort)XDBlockOffsets[block]);
            WriteUInt16BigEndian(first[(0x42 + 4 * block)..], (ushort)(XDBlockOffsets[block] >> 16));
        }
        XDCrypto.EncryptSlot(first);

        var save = new SAV3XD(data)
        {
            CurrentRegion = GCRegion.NTSC_U,
            OriginalRegion = GCRegion.NTSC_U,
            Language = (int)LanguageID.English,
        };
        save.ClearBoxes();
        return save;
    }
}
