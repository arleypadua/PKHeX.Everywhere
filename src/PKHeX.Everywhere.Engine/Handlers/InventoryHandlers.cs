using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class InventoryHandlers
{
    [Query("inventory.get", Topics.Inventory)]
    public static Pouch[] Get(Game game) => game.Trainer.Inventories.InventoryItems.Values
        .OrderBy(inventory => inventory.Type, StringComparer.Ordinal)
        .Select(inventory => inventory.ToPouch())
        .ToArray();

    [Command("inventory.setItem")]
    public static void SetItem(Game game, ItemHandle at, int count)
    {
        if (!game.Trainer.Inventories.InventoryItems.TryGetValue(at.Pouch, out var inventory))
            throw new EngineException(ErrorCodes.NotFound, $"The save has no {at.Pouch} pouch.");

        var owned = inventory.AllExceptNone().Any(item => item.Id == at.ItemId);
        if (count == 0 && owned)
        {
            inventory.Remove((ushort)at.ItemId);
            return;
        }

        if (inventory.AllSupportedItems.All(item => item.Id != at.ItemId))
            throw new EngineException(ErrorCodes.BadArguments, $"The {at.Pouch} pouch doesn't support item {at.ItemId}.");

        var itemId = (ushort)at.ItemId;
        var maxCount = inventory.MaxCountOf(itemId);
        if (count < 0 || count > maxCount)
            throw new EngineException(ErrorCodes.OutOfRange, $"Count must be between 0 and {maxCount}, got {count}.");

        if (count == 0)
            return;

        if (!inventory.Set(itemId, (uint)count))
            throw new EngineException(ErrorCodes.PouchFull, $"The {at.Pouch} pouch has no free slot.");
    }
}
