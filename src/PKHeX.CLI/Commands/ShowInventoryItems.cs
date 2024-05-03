using PKHeX.CLI.Base;
using PKHeX.CLI.Commands;
using PKHeX.Facade;
using Spectre.Console;

namespace PKHeX.CLI;

public class ShowInventoryItems
{
    public static Result Handle(Game game, string inventoryType)
    {
        RepeatUntilExit(() =>
        {
            var inventory = game.Trainer.Inventories[inventoryType];
            var inventoryItems = inventory
                .AllExceptNone()
                .OrderBy(i => i.Name)
                .AsEnumerable();

            var noneItem = inventory.FirstOrDefault(i => i.IsNone);
            if (noneItem != null)
            {
                inventoryItems = inventoryItems.Append(noneItem);
            }

            var selection = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
                .AddChoices(OptionOrBack.WithValues(
                    options: inventoryItems, 
                    display: (item) => item.IsNone
                        ? "[bold lightgreen]+ Add new item[/]"
                        : $"(#{item.Id:000}) {item.Name} x [yellow]{item.Count}[/]"))
                .Title($"[bold darkgreen]Bag of {inventoryType}[/] [yellow]({inventoryItems.Count()}/{inventory.Count()})[/]")
                .PageSize(10)
                .EnableSearch()
                .WrapAround(true));
 
            return selection switch
            {
                OptionOrBack.Back => Result.Exit,
                OptionOrBack.Option<Inventory.Item> item and { Value: { IsNone: false } } => 
                    EditInventoryItem.Handle(game, inventoryType, item.Value.Id),
                OptionOrBack.Option<Inventory.Item> item and { Value: { IsNone: true } } => 
                    AddInventoryItem.Handle(game, inventoryType),
                _ => Result.Exit
            };
        });

        return Result.Continue;
    }
}
