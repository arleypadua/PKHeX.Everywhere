using PKHeX.CLI.Base;
using PKHeX.Facade;
using Spectre.Console;

namespace PKHeX.CLI.Commands;

public static class EditInventoryItem
{
    public static Result Handle(Game game, string inventoryType, ushort itemId) => SafeHandle(() => {
        var itemDefinition = game.ItemRepository.GetItem(itemId);
        var inventory = game.Trainer.Inventories[inventoryType];
        var item = inventory.Items.FirstOrDefault(i => i.Id == itemId);

        var promptText = $"How many {itemDefinition.Name} would you like to set?";
        if (item != null)
        {
            promptText = $"{promptText} [grey italic](current: {item.Count})[/]";
        }
        
        var count = AnsiConsole.Ask(promptText, item?.Count ?? 0);
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
