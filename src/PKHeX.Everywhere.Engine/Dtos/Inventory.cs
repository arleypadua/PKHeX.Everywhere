using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// Points <c>inventory.setItem</c> at an item in one pouch.
/// </summary>
/// <param name="Pouch">The pouch's name, as <c>Pouch.name</c> gives it.</param>
/// <param name="ItemId">The item's id in the save's own item list, as <c>OwnedItem.id</c> and <c>AddableItem.id</c> give it.</param>
public record ItemHandle(string Pouch, int ItemId) : IHandle
{
    public string Topic() => Topics.Inventory;
}

/// <summary>
/// An item the trainer holds in a pouch.
/// </summary>
/// <param name="Id">The item's id in the save's own item list. From Generation 4 on, and in ROM hack saves, these are PKHeX.Core item ids; other Generation 1 to 3 saves use their game's own numbering. An unknown item gets an id of its own, which only means something to this save.</param>
/// <param name="Index">The item as the save stores it, before any conversion: the ROM hack's own index in hack saves, and the same value as <c>id</c> elsewhere. Use it to index the game's own tables, such as its item icons.</param>
/// <param name="Count">How many the trainer holds.</param>
/// <param name="MaxCount">The highest count <c>inventory.setItem</c> accepts for this item in this pouch.</param>
/// <param name="IsUnknown">True when the save stores an item PKHeX has no id for, such as a ROM hack's own item. <c>name</c> is then <c>Unknown item #n</c>. <c>inventory.setItem</c> can change its count or remove it in this pouch, and no pouch offers it in <c>addable</c>.</param>
public record OwnedItem(int Id, int Index, string Name, int Count, int MaxCount, bool IsUnknown);

/// <summary>
/// An item the pouch can hold that the trainer doesn't have yet.
/// </summary>
/// <param name="Id">The item's id in the save's own item list. From Generation 4 on, and in ROM hack saves, these are PKHeX.Core item ids; other Generation 1 to 3 saves use their game's own numbering.</param>
/// <param name="Index">The item as the save stores it, before any conversion: the ROM hack's own index in hack saves, and the same value as <c>id</c> elsewhere. Use it to index the game's own tables, such as its item icons.</param>
/// <param name="MaxCount">The highest count <c>inventory.setItem</c> accepts for this item in this pouch.</param>
public record AddableItem(int Id, int Index, string Name, int MaxCount);

/// <summary>
/// One bag pouch in the save, returned by <c>inventory.get</c>.
/// </summary>
/// <param name="Name">The PKHeX <c>InventoryType</c> name, such as <c>Items</c> or <c>KeyItems</c>.</param>
/// <param name="Items">The items the trainer holds in this pouch.</param>
/// <param name="Addable">The items this pouch can hold that the trainer doesn't have yet.</param>
public record Pouch(string Name, OwnedItem[] Items, AddableItem[] Addable);

public static class InventoryMapping
{
    public static Pouch ToPouch(this Inventory inventory) => new(
        inventory.Type,
        inventory.AllExceptNone()
            .Select(item => new OwnedItem(item.Id, item.Index, item.Name, item.Count, inventory.MaxCountOf(item.Id), item.IsUnknown))
            .ToArray(),
        inventory.CurrentSupportedItems
            .Select(item => new AddableItem(item.Id, item.Index, item.Name, inventory.MaxCountOf(item.Id)))
            .ToArray());
}
