using System.ComponentModel;
using PKHeX.CLI.Base;
using PKHeX.CLI.Facade;
using Spectre.Console;

namespace PKHeX.CLI.Commands;

public static class ViewInventory
{
    public static Result Handle(Game game)
    {
        RepeatUntilExit(() =>
        {
            var types = game.Trainer.Inventories.InventoryTypes.OrderBy(t => t);
            var selection = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
                .Title("Which inventory would you like to view?")
                .PageSize(10)
                .AddChoices(OptionOrBack.WithValues(types))
                .WrapAround(true));

            return selection switch
            {
                OptionOrBack.Back => Result.Exit,
                OptionOrBack.Option<string> option => ShowInventoryItems.Handle(game, option.Value),
                _ => Result.Exit
            };
        });


        return Result.Continue;
    }
}
