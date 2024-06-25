using PKHeX.CLI.Base;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;
using Spectre.Console;

namespace PKHeX.CLI.Commands
{
    public static class AddInventoryItem
    {
        public static Result Handle(Game game, string inventoryType) => SafeHandle(() =>
        {
            var inventory = game.Trainer.Inventories[inventoryType];
            var supportedItems = inventory
                .CurrentSupportedItems
                .OrderBy(i => i.Name);

            var selection = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
                    .AddChoices(OptionOrBack.WithValues(
                        options: supportedItems,
                        display: (item) => $"(#{item.Id:000}) {item.Name}"))
                    .Title($"[bold darkgreen]Add item to Bag ({inventoryType})[/]")
                    .PageSize(10)
                    .EnableSearch()
                    .WrapAround());

            return selection switch
            {
                OptionOrBack.Back => Result.Exit,
                OptionOrBack.Option<ItemDefinition> item => EditInventoryItem.Handle(game, inventoryType, item.Value.Id),
                _ => Result.Exit
            };
        });
    }
}