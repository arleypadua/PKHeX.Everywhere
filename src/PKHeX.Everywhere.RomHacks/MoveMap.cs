namespace PKHeX.Everywhere.RomHacks;

internal sealed class MoveMap
{
    private readonly ushort[] _nationalByIndex;
    private readonly Dictionary<ushort, ushort> _indexByNational;

    public MoveMap(ushort[] nationalByIndex)
    {
        _nationalByIndex = nationalByIndex;
        _indexByNational = nationalByIndex
            .Select((move, index) => (Move: move, Index: (ushort)index))
            .Where(entry => entry.Move != 0)
            .DistinctBy(entry => entry.Move)
            .ToDictionary(entry => entry.Move, entry => entry.Index);
        All = _indexByNational.Keys.ToHashSet();
    }

    public IReadOnlySet<ushort> All { get; }

    public ushort ToNational(ushort index) =>
        index >= _nationalByIndex.Length ? (ushort)0
        : IsUnknown(index) ? (ushort)(RomHackIds.Base + index)
        : _nationalByIndex[index];

    public ushort? ToIndex(ushort move) =>
        move == 0 ? (ushort)0
        : _indexByNational.TryGetValue(move, out var index) ? index
        : UnknownIndex(move);

    public string? NameOf(int move) => UnknownIndex(move) is { } index ? $"Unknown move #{index}" : null;

    private bool IsUnknown(int index) => index > 0 && index < _nationalByIndex.Length && _nationalByIndex[index] == 0;

    private ushort? UnknownIndex(int move) => IsUnknown(move - RomHackIds.Base) ? (ushort)(move - RomHackIds.Base) : null;
}
