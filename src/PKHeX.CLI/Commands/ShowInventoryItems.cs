using PKHeX.CLI.Base;
using PKHeX.Facade;
using Spectre.Console;

namespace PKHeX.CLI.Commands;

public static class ShowInventoryItems
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

            var allItems = inventoryItems.ToList();

            var selection = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
                .AddChoices(OptionOrBack.WithValues(
                    options: allItems, 
                    display: (item) => item.IsNone
                        ? "[bold lightgreen]+ Add new item[/]"
                        : $"(#{item.Id:000}) {item.Name} x [yellow]{item.Count}[/]"))
                .Title($"[bold darkgreen]Bag of {inventoryType}[/] [yellow]({allItems.Count()}/{inventory.Count()})[/]")
                .PageSize(10)
                .EnableSearch()
                .WrapAround());
 
            return selection switch
            {
                OptionOrBack.Back => Result.Exit,
                OptionOrBack.Option<Inventory.Item> { Value: { IsNone: false } } item => 
                    EditInventoryItem.Handle(game, inventoryType, item.Value.Id),
                OptionOrBack.Option<Inventory.Item> { Value: { IsNone: true } } => 
                    AddInventoryItem.Handle(game, inventoryType),
                _ => Result.Exit
            };
        });

        return Result.Continue;
    }
}
