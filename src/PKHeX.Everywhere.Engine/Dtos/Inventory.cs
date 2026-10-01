using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Dtos;

public record ItemHandle(string Pouch, int ItemId) : IHandle
{
    public string Topic() => Topics.Inventory;
}

public record OwnedItem(int Id, string Name, int Count, int MaxCount);

public record AddableItem(int Id, string Name, int MaxCount);

public record Pouch(string Name, OwnedItem[] Items, AddableItem[] Addable);

public static class InventoryMapping
{
    public static Pouch ToPouch(this Inventory inventory) => new(
        inventory.Type,
        inventory.AllExceptNone()
            .Select(item => new OwnedItem(item.Id, item.Name, item.Count, inventory.MaxCountOf(item.Id)))
            .ToArray(),
        inventory.CurrentSupportedItems
            .Select(item => new AddableItem(item.Id, item.Name, inventory.MaxCountOf(item.Id)))
            .ToArray());
}
