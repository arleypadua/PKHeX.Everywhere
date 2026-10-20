using PKHeX.Core;

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
        GameVersion.XD => BlankSaveFile.Get(SaveFileType.XD, version),
        GameVersion.RSBOX => BlankSaveFile.Get(SaveFileType.RSBox, version),
        _ => null,
    };

    // PKHeX's blank Colosseum constructor never sizes its data and throws, so the blank reads an empty save and is set up as that constructor would.
    private static SAV3Colosseum Colosseum()
    {
        var save = new SAV3Colosseum(new byte[SaveUtil.SIZE_G3COLO], decrypt: false)
        {
            CurrentRegion = GCRegion.NTSC_U,
            OriginalRegion = GCRegion.NTSC_U,
            Language = (int)LanguageID.English,
        };
        save.ClearBoxes();
        return save;
    }
}
