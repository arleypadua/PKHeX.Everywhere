namespace PKHeX.Everywhere.RomHacks.Cfru;

internal static class CfruMoves
{
    private static readonly Dictionary<ushort, ushort> IndexByNational = CfruMoveTable.NationalByIndex
        .Select((move, index) => (Move: move, Index: (ushort)index))
        .Where(entry => entry.Move != 0)
        .DistinctBy(entry => entry.Move)
        .ToDictionary(entry => entry.Move, entry => entry.Index);

    public static readonly IReadOnlySet<ushort> All = IndexByNational.Keys.ToHashSet();

    public static ushort ToNational(ushort index) =>
        index < CfruMoveTable.NationalByIndex.Length ? CfruMoveTable.NationalByIndex[index] : (ushort)0;

    public static ushort? ToIndex(ushort move) =>
        move == 0 ? (ushort)0 : IndexByNational.TryGetValue(move, out var index) ? index : null;
}
