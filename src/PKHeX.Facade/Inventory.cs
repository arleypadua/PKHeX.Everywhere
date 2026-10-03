using System.Collections;
using System.Collections.Immutable;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Repositories;
using PKHeX.Core;

namespace PKHeX.Facade;

public class Inventory : IEnumerable<Inventory.Item>
{
    private readonly Game _game;
    private readonly PlayerBag _bag;
    private readonly InventoryPouch _pouch;

    public Inventory(string type, Game game) : this(type, game, game.SaveFile.Inventory) { }

    internal Inventory(string type, Game game, PlayerBag bag)
    {
        _game = game;
        _bag = bag;
        _pouch = _bag.Pouches.FirstOrDefault(i => i.Type.ToString() == type)
            ?? throw new InvalidOperationException($"Inventory of type {type} not found");

        Type = type;
    }

    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
    public IEnumerator<Item> GetEnumerator() => Items.GetEnumerator();


    public string Type { get; init; }
    public ImmutableList<Item> Items => GetItems();

    /// <summary>
    /// Returns the items the save stores with ids PKHeX doesn't know, such as a ROM hack's own items. They have id 0 and can't be set.
    /// </summary>
    public ImmutableList<Item> UnknownItems => _pouch is IUnmappedItems pouch
        ? pouch.UnmappedItems
            .Select(item => new Item(new InventoryItem { Count = item.Count }, _ => ItemDefinition.Unmapped(item.Id)))
            .ToImmutableList()
        : [];

    /// <summary>
    /// Returns the max count a single item can have
    /// </summary>
    public int MaxItemCountAllowed => _pouch.MaxCount;

    /// <summary>
    /// Returns the max count of a specific item, which can be lower than <see cref="MaxItemCountAllowed"/> (e.g. HMs)
    /// </summary>
    public int MaxCountOf(ushort itemId) => _bag.GetMaxCount(_pouch.Type, itemId);

    /// <summary>
    /// Returns a list of all items supported in this inventory
    /// </summary>
    public ImmutableList<ItemDefinition> AllSupportedItems => _pouch
        .GetAllItems()
        .ToArray()
        .Select(_game.ItemRepository.GetGameItem).ToImmutableList();

    /// <summary>
    /// Returns a list of all items that can be added to the current inventory
    /// </summary>
    public ImmutableList<ItemDefinition> CurrentSupportedItems => AllSupportedItems
        .Except(Items.Select(i => i.Definition))
        .ToImmutableList();
    
    public bool Supports(ItemDefinition item) =>
        AllSupportedItems.Any(i => i.Id == item.Id);

    /// <summary>
    /// Whether <see cref="TrySet"/> accepts the item: one the pouch supports, or an unknown item already in it.
    /// </summary>
    public bool CanSet(ushort itemId) =>
        AllSupportedItems.Any(i => i.Id == itemId) || Items.Any(i => i.Id == itemId && i.IsUnknown);

    public void Remove(ushort itemId)
    {
        if (itemId == ItemDefinition.None)
        {
            throw new InvalidOperationException("Cannot remove None item.");
        }

        _pouch.RemoveAll(i => i.Index == itemId);
        Commit();
    }

    // Published plug-in DLLs bind to this exact signature; changing its return type breaks them at runtime.
    public void Set(ushort itemId, uint count) => TrySet(itemId, count);

    /// <returns>false when the item isn't owned and the pouch has no free slot</returns>
    public bool TrySet(ushort itemId, uint count)
    {
        if (itemId == ItemDefinition.None)
        {
            throw new InvalidOperationException("Cannot set item to None.");
        }
        
        if (!CanSet(itemId))
        {
            throw new InvalidOperationException($"Item {itemId} is not supported in this inventory.");
        }

        var owned = _pouch.Items.Where(i => i.Index == itemId).ToArray();
        if (owned.Length > 0)
        {
            foreach (var duplicate in owned.Skip(1)) duplicate.Clear();
            owned[0].Count = _bag.Clamp(_pouch.Type, itemId, Convert.ToInt32(count));
        }
        else if (_pouch.GiveItem(_bag, itemId, Convert.ToInt32(count)) < 0) return false;

        Commit();
        return true;
    }

    private void Commit()
    {
        _bag.CopyTo(_game.SaveFile);
    }

    private ImmutableList<Item> GetItems()
    {
        return _pouch.Items.Select(i => new Item(i, _game.ItemRepository.GetGameItem)).ToImmutableList();
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

        public Item() : this(default!, default!) { }

        public ushort Id => Convert.ToUInt16(_item.Index);
        public string Name => _itemFetcher(Id).Name;
        public int Count => _item.Count;

        public bool IsNone => Id == ItemDefinition.None;
        public bool IsUnknown => Definition.IsUnknown;
        public ItemDefinition Definition => _itemFetcher(Id);
        
        public override string ToString() => $"{Name} x{Count}";
    }
}
