namespace PKHeX.Everywhere.RomHacks.Cfru;

internal static class CfruMoves
{
    // PKHeX's move ids end below 1000, so an index with no national move travels as an id from here up.
    private const ushort UnknownIdBase = 0xF000;

    private static readonly ushort[] NationalByIndex = CfruMoveTable.NationalByIndex;

    private static readonly Dictionary<ushort, ushort> IndexByNational = NationalByIndex
        .Select((move, index) => (Move: move, Index: (ushort)index))
        .Where(entry => entry.Move != 0)
        .DistinctBy(entry => entry.Move)
        .ToDictionary(entry => entry.Move, entry => entry.Index);

    public static readonly IReadOnlySet<ushort> All = IndexByNational.Keys.ToHashSet();

    public static ushort ToNational(ushort index) =>
        index >= NationalByIndex.Length ? (ushort)0
        : IsUnknown(index) ? (ushort)(UnknownIdBase + index)
        : NationalByIndex[index];

    public static ushort? ToIndex(ushort move) =>
        move == 0 ? (ushort)0
        : IndexByNational.TryGetValue(move, out var index) ? index
        : UnknownIndex(move);

    public static string? NameOf(int move) => UnknownIndex(move) is { } index ? $"Unknown move #{index}" : null;

    private static bool IsUnknown(int index) => index > 0 && index < NationalByIndex.Length && NationalByIndex[index] == 0;

    private static ushort? UnknownIndex(int move) => IsUnknown(move - UnknownIdBase) ? (ushort)(move - UnknownIdBase) : null;
}
