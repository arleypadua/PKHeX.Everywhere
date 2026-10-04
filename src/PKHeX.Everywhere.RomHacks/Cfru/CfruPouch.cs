using PKHeX.Core;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Everywhere.RomHacks.Cfru;

/// <summary>
/// A bag pocket stored as a hack item index and a quantity per slot, the quantity XORed with a key where the hack encrypts it.
/// Its slots can be split across regions of the file.
/// </summary>
public sealed class CfruPouch(InventoryType type, IItemStorage info, int maxCount, CfruItemMap map, ushort countKey, params (int Offset, int Slots)[] regions)
    : InventoryPouch(type, info, maxCount, 0, regions.Sum(region => region.Slots))
{
    public const int SlotSize = 4;

    private InventoryItem[] _items = [];
    private HashSet<int> _outsideTable = [];

    public override InventoryItem[] Items => _items;

    private IEnumerable<int> SlotOffsets => regions
        .SelectMany(region => Enumerable.Range(0, region.Slots).Select(slot => region.Offset + (slot * SlotSize)));

    public override void GetPouch(ReadOnlySpan<byte> data)
    {
        List<InventoryItem> items = [];
        HashSet<int> outsideTable = [];
        foreach (var (slot, offset) in SlotOffsets.Index())
        {
            var index = ReadUInt16LittleEndian(data[offset..]);
            int count = (ushort)(ReadUInt16LittleEndian(data[(offset + 2)..]) ^ countKey);
            if (map.IsOutsideTable(index)) outsideTable.Add(slot);
            else items.Add(new InventoryItem { Index = map.ToModern(index), Count = count });
        }

        _items = [..items];
        _outsideTable = outsideTable;
    }

    // Items fill the other slots, so the ones holding an index outside the hack's table keep their place and raw values.
    public override void SetPouch(Span<byte> data)
    {
        var next = 0;
        foreach (var (slot, offset) in SlotOffsets.Index())
        {
            if (_outsideTable.Contains(slot)) continue;
            var item = _items[next++];
            var index = map.ToIndex((ushort)item.Index)
                ?? throw new InvalidOperationException($"Item {item.Index} has no index in this hack.");
            WriteUInt16LittleEndian(data[offset..], index);
            WriteUInt16LittleEndian(data[(offset + 2)..], (ushort)(item.Count ^ countKey));
        }
    }

    public override InventoryItem GetEmpty(int itemID = 0, int count = 0) => new() { Index = itemID, Count = count };
}
