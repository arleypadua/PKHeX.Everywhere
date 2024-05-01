using PkHex.CLI.Base;
using PkHex.CLI.Facade;
using Spectre.Console;

namespace PkHex.CLI;

public class ShowInventoryItems
{
    public static Result Handle(Game game, string inventoryType)
    {
        RepeatUntilExit(() =>
        {
            var inventory = game.Trainer.Inventories[inventoryType];
            var inventoryItems = inventory
                .AllExceptNone()
                .OrderBy(i => i.Name);

            var selection = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
                .AddChoices(OptionOrBack.WithValues(
                    options: inventoryItems, 
                    display: (item) => $"[[#{item.Id:000}]] {item.Name} x [yellow]{item.Count}[/]"))
                .Title($"[bold darkgreen]Bag of {inventoryType}[/] [yellow]({inventoryItems.Count()}/{inventory.Count()})[/]")
                .PageSize(10)
                .WrapAround(true));
 
            return selection switch
            {
                OptionOrBack.Back => Result.Exit,
                OptionOrBack.Option<Inventory.Item> item => EditInventoryItem.Handle(game, inventoryType, item.Value.Id),
                _ => Result.Exit
            };
        });

        return Result.Continue;
    }
}
