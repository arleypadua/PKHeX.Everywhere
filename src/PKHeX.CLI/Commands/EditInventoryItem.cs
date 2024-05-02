using PKHeX.CLI.Base;
using PKHeX.CLI.Facade;
using Spectre.Console;

namespace PKHeX.CLI;

public class EditInventoryItem
{
    public static Result Handle(Game game, string inventoryType, ushort itemId) => SafeHandle(() => {
        var inventory = game.Trainer.Inventories[inventoryType];
        var item = inventory.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
        {
            throw new InvalidOperationException($"Item with ID {itemId} not found in {inventoryType} inventory.");
        }

        var count = AnsiConsole.Ask($"How many {item.Name} would you like to set? [grey italic](setting to 0 will remove the item)[/]", item.Count);
        if (count == 0)
        {
            inventory.Remove(itemId);
        }
        else
        {
            inventory.Set(itemId, Convert.ToUInt16(count));
        }

        return Result.Continue;
    });
}
