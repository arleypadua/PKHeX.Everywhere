using PKHeX.Core;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade;

// PKHeX reads Stadium teams but never writes them, and keeps their layout private, so the sizes here mirror its constants.
// Each team block is a header, six slots and a footer: a magic number, then the 16-bit sum of every byte before it.
internal sealed record StadiumTeamLayout(
    int Count,
    Func<SAV_STADIUM, int, int> Offset,
    int TeamsPerCup,
    string[]? Cups,
    int HeaderSize,
    int Size,
    int? CountAt,
    int NameAt,
    int NameLength,
    uint Magic)
{
    public const uint PokeMagic = 0x454B4F50; // "POKE"
    public const uint Stadium2Magic = 0x30763350; // "P3v0"

    private static readonly string[] Stadium1Cups = ["Anything Goes", "Poké Cup", "Petit Cup", "Pika Cup", "Prime Cup", "Gym Leader Castle", "Vs. Mewtwo"];
    private static readonly string[] Stadium1JapaneseCups = ["Anything Goes", "Nintendo Cup '97", "Nintendo Cup '98", "Nintendo Cup '99", "Petit Cup", "Pika Cup", "Prime Cup", "Gym Leader Castle", "Vs. Mewtwo"];
    private static readonly string[] Stadium2Cups = ["Anything Goes", "Little Cup", "Poké Cup", "Prime Cup", "Gym Leader Castle", "Vs. Rival"];

    // The English Stadium stores two cups it never uses after the first, which PKHeX's team numbers still count.
    private static readonly StadiumTeamLayout Stadium1 = new(70, (save, team) => ((SAV1Stadium)save).GetTeamOffset(team < 10 ? team : team + 20),
        10, Stadium1Cups, 0x10, 0x160, 0x0F, 0x01, 7, PokeMagic);

    private static readonly StadiumTeamLayout Stadium1Japanese = new(108, (save, team) => ((SAV1Stadium)save).GetTeamOffset(team),
        12, Stadium1JapaneseCups, 0x0C, 0x120, 0x0B, 0x02, 5, PokeMagic);

    // Where Pocket Monsters Stadium counts a team's members isn't known, so its count is left alone.
    private static readonly StadiumTeamLayout PocketMonstersStadium = new(16, (_, team) => SAV1StadiumJ.GetTeamOffset(team),
        16, null, 0x14, 0x128, null, 0x02, 5, PokeMagic);

    private static readonly StadiumTeamLayout Stadium2 = new(60, (_, team) => SAV2Stadium.GetTeamOffset(team),
        10, Stadium2Cups, 0x10, 0x180, 0x01, 0x04, 7, Stadium2Magic);

    public static StadiumTeamLayout Of(SAV_STADIUM save) => save switch
    {
        SAV1Stadium { Japanese: true } => Stadium1Japanese,
        SAV1Stadium => Stadium1,
        SAV1StadiumJ => PocketMonstersStadium,
        SAV2Stadium => Stadium2,
        _ => throw new NotSupportedException($"{save.GetType().Name} has no known team layout."),
    };

    public string? CupOf(int team) => Cups?[team / TeamsPerCup];
}

internal static class StadiumSaves
{
    public static readonly IReadOnlySet<GameVersion> Versions = new HashSet<GameVersion> { GameVersion.Stadium, GameVersion.StadiumJ, GameVersion.Stadium2 };

    // PKHeX's blank Stadium saves have no footer magic, so they aren't recognized once exported. Each box gets the footer a loaded
    // save has, and the games' checksums are written when the save is.
    public static SaveFile? Blank(GameVersion version)
    {
        switch (version)
        {
            case GameVersion.Stadium:
                // Loading conditions every box without a valid header, writing its header and footer.
                return new SAV1Stadium(new byte[SaveUtil.SIZE_G1STAD], japanese: false);
            case GameVersion.StadiumJ:
            {
                // PKHeX's blank constructor writes an empty slot to every box, which fails like any Pocket Monsters Stadium slot write.
                var save = new SAV1StadiumJ(new byte[SaveUtil.SIZE_G1STADJ]);
                for (var box = 0; box < save.BoxCount; box++)
                    Seal(save.Data, save.GetBoxOffset(box) - 0x14, 0x560, StadiumTeamLayout.PokeMagic);
                return save;
            }
            case GameVersion.Stadium2:
            {
                var save = new SAV2Stadium(japanese: false);
                for (var box = 0; box < save.BoxCount; box++)
                    Seal(save.Data, save.GetBoxOffset(box) - 0x20, 0x4D8, StadiumTeamLayout.Stadium2Magic);
                return save;
            }
            default:
                return null;
        }
    }

    private static void Seal(Span<byte> data, int boxStart, int boxSize, uint magic) =>
        WriteUInt32LittleEndian(data[(boxStart + boxSize - 6)..], magic);
}
