using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Everywhere.RomHacks.Cfru;

/// <summary>
/// A bag pocket stored as a hack item index and a quantity per slot, with no XOR. Its slots can be split across regions of the file.
/// </summary>
public sealed class CfruPouch(InventoryType type, IItemStorage info, int maxCount, CfruItemMap map, params (int Offset, int Slots)[] regions)
    : InventoryPouch(type, info, maxCount, 0, regions.Sum(region => region.Slots)), IUnmappedItems
{
    private const int SlotSize = 4;

    private InventoryItem[] _items = [];
    private (int Slot, ushort Id, int Count)[] _unmapped = [];

    public override InventoryItem[] Items => _items;

    public IReadOnlyList<(ushort Id, int Count)> UnmappedItems => _unmapped.Select(item => (item.Id, item.Count)).ToArray();

    private IEnumerable<int> SlotOffsets => regions
        .SelectMany(region => Enumerable.Range(0, region.Slots).Select(slot => region.Offset + (slot * SlotSize)));

    public override void GetPouch(ReadOnlySpan<byte> data)
    {
        List<InventoryItem> items = [];
        List<(int, ushort, int)> unmapped = [];
        foreach (var (slot, offset) in SlotOffsets.Index())
        {
            var index = ReadUInt16LittleEndian(data[offset..]);
            int count = ReadUInt16LittleEndian(data[(offset + 2)..]);
            var item = map.ToModern(index);
            if (index != 0 && item == 0) unmapped.Add((slot, index, count));
            else items.Add(new InventoryItem { Index = item, Count = count });
        }

        _items = [..items];
        _unmapped = [..unmapped];
    }

    // Items fill the slots the unmapped ones don't hold, so those keep their place and raw values.
    public override void SetPouch(Span<byte> data)
    {
        var unmapped = _unmapped.Select(item => item.Slot).ToHashSet();
        var next = 0;
        foreach (var (slot, offset) in SlotOffsets.Index())
        {
            if (unmapped.Contains(slot)) continue;
            var item = _items[next++];
            var index = map.ToIndex((ushort)item.Index)
                ?? throw new InvalidOperationException($"Item {item.Index} has no index in this hack.");
            WriteUInt16LittleEndian(data[offset..], index);
            WriteUInt16LittleEndian(data[(offset + 2)..], (ushort)item.Count);
        }
    }

    public override InventoryItem GetEmpty(int itemID = 0, int count = 0) => new() { Index = itemID, Count = count };
}
