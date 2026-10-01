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
}
