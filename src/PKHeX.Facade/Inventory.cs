using System.Collections;
using System.Collections.Immutable;
using PKHeX.Facade.Repositories;
using PKHeX.Core;

namespace PKHeX.Facade;

public class Inventory : IEnumerable<Inventory.Item>
{
    private readonly SaveFile _saveFile;
    private readonly ItemRepository _itemRepository;
    private readonly InventoryPouch _pouch;

    public Inventory(string type, SaveFile saveFile, ItemRepository itemRepository)
    {
        _saveFile = saveFile;
        _itemRepository = itemRepository;
        _pouch = saveFile.Inventory.FirstOrDefault(i => i.Type.ToString() == type)
            ?? throw new InvalidOperationException($"Inventory of type {type} not found");

        Type = type;
    }

    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
    public IEnumerator<Item> GetEnumerator() => Items.GetEnumerator();


    public string Type { get; init; }
    public ImmutableList<Item> Items => GetItems();
    public ImmutableList<ItemDefinition> SupportedItems => _pouch
        .GetAllItems()
        .ToArray()
        .Select(_itemRepository.GetItem).ToImmutableList();

    public void Remove(ushort itemId)
    {
        if (itemId == ItemDefinition.None)
        {
            throw new InvalidOperationException("Cannot remove None item.");
        }
        
        _pouch.RemoveAll(i => i.Index == itemId);
        Commit();
    }

    public void Set(ushort itemId, uint count)
    {
        if (itemId == ItemDefinition.None)
        {
            throw new InvalidOperationException("Cannot set item to None.");
        }
        
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than or equal to zero.");
        }

        if (SupportedItems.All(i => i.Id != itemId))
        {
            throw new InvalidOperationException($"Item {itemId} is not supported in this inventory.");
        }

        _pouch.RemoveAll(i => i.Index == itemId);
        _pouch.GiveItem(_saveFile, itemId, Convert.ToInt32(count));

        Commit();
    }

    private void Commit()
    {
        _saveFile.Inventory = _saveFile.Inventory
            .Select(i => i.Type.ToString() == Type ? _pouch : i)
            .ToImmutableList();
    }

    private ImmutableList<Item> GetItems()
    {
        return _pouch.Items.Select(i => new Item(i, _itemRepository.GetItem)).ToImmutableList();
    }

    public sealed class Item
    {
        private readonly Func<ushort, ItemDefinition> _itemFetcher;
        private readonly InventoryItem _item;

        public Item(InventoryItem item, Func<ushort, ItemDefinition> itemFetcher)
        {
            _itemFetcher = itemFetcher;
            _item = item;
        }

        public ushort Id => Convert.ToUInt16(_item.Index);
        public string Name => _itemFetcher(Id).Name;
        public int Count => _item.Count;

        public bool IsNone => Id == ItemDefinition.None;
        public ItemDefinition Definition => _itemFetcher(Id);
        
        public override string ToString() => $"{Name} x{Count}";
    }
}
