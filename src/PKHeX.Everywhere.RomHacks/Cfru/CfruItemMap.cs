using PKHeX.Core;

namespace PKHeX.Everywhere.RomHacks.Cfru;

public sealed class CfruItemMap
{
    // Gen 3 Pokémon can't hold TMs, so modern held-item lists are used without their machines and records.
    private static readonly HashSet<ushort> Holdable =
    [
        ..ItemStorage7USUM.GetAllHeld(), ..ItemStorage8SWSH.GetAllHeld(), ..ItemStorage9SV.GetAllHeld(), ..ItemStorage9ZA.GetAllHeld(),
    ];

    private static readonly HashSet<ushort> Machines = [..ItemStorage9SV.Machine];

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
            .Where(item => Holdable.Contains(item) && !Machines.Contains(item) && !ItemStorage8SWSH.IsTechRecord(item))
            .Order()
            .ToArray();
        Pockets = new CfruItemStorage(_indexByModern.Keys);
    }

    public ushort[] HeldItems { get; }

    public CfruItemStorage Pockets { get; }

    public ushort ToModern(ushort index) => index < _modernByIndex.Length ? _modernByIndex[index] : (ushort)0;

    public ushort? ToIndex(ushort item) => item == 0 ? (ushort)0 : _indexByModern.TryGetValue(item, out var index) ? index : null;
}
