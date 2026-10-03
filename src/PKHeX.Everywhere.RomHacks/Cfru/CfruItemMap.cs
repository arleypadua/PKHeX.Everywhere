using PKHeX.Core;

namespace PKHeX.Everywhere.RomHacks.Cfru;

public sealed class CfruItemMap
{
    // Gen 3 Pokémon can't hold TMs, so modern held-item lists are used without their machines and records.
    private static readonly HashSet<ushort> Holdable =
    [
        ..ItemStorage7USUM.GetAllHeld(), ..ItemStorage8SWSH.GetAllHeld(), ..ItemStorage9SV.GetAllHeld(), ..ItemStorage9ZA.GetAllHeld(),
    ];

    // PKHeX's item ids stay far below this, so an index the hack's table has no modern item for gets this id plus the index.
    private const ushort UnknownBase = 0xF000;

    private readonly ushort[] _modernByIndex;
    private readonly Dictionary<ushort, ushort> _indexByModern;

    public CfruItemMap(ushort[] modernByIndex)
    {
        _modernByIndex = modernByIndex;
        _indexByModern = modernByIndex
            .Select((item, index) => (Item: item, Index: (ushort)index))
            .Where(entry => entry.Item != 0)
            .DistinctBy(entry => entry.Item)
            .ToDictionary(entry => entry.Item, entry => entry.Index);
        HeldItems = _indexByModern.Keys
            .Where(item => Holdable.Contains(item) && !CfruItemStorage.Machines.Contains(item) && !ItemStorage8SWSH.IsTechRecord(item))
            .Order()
            .ToArray();
        Pockets = new CfruItemStorage(_indexByModern.Keys);
    }

    public ushort[] HeldItems { get; }

    public IEnumerable<ushort> Items => _indexByModern.Keys;

    public CfruItemStorage Pockets { get; }

    // An index past the table reads as no item, so it stays in the save as it is.
    public ushort ToModern(ushort index) => index switch
    {
        0 => 0,
        _ when index >= _modernByIndex.Length => 0,
        _ when _modernByIndex[index] == 0 => (ushort)(UnknownBase + index),
        _ => _modernByIndex[index],
    };

    public ushort? ToIndex(ushort item) => item == 0 ? (ushort)0
        : UnknownIndex(item) is { } index ? index
        : _indexByModern.TryGetValue(item, out var mapped) ? mapped : null;

    public bool IsOutsideTable(ushort index) => index >= _modernByIndex.Length;

    public ushort? UnknownIndex(int item) =>
        item > UnknownBase && item - UnknownBase < _modernByIndex.Length && _modernByIndex[item - UnknownBase] == 0
            ? (ushort)(item - UnknownBase)
            : null;
}
