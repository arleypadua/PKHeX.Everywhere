using PKHeX.CLI.Base;
using PKHeX.Facade;
using Spectre.Console;

namespace PKHeX.CLI.Commands;

public static class Exit
{
    public static Result Handle(Game game, PkCommand.Settings settings)
    {
        var answer = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("There may be changes to your save, what would you like to do?")
            .AddChoices(Choices.LeaveWithoutSaving, Choices.OverwriteCurrent, Choices.SaveAsNew, Choices.Cancel));

        var result = answer switch
        {
            Choices.LeaveWithoutSaving => Result.Exit,
            Choices.OverwriteCurrent => Save.SaveExisting(game, settings),
            Choices.SaveAsNew => Save.SaveAsNew(game, settings),
            Choices.Cancel => Result.Continue,
            _ => Result.Continue
        };

        if (result == Result.Exit)
        {
            settings.PersistedSettings.Save();
        }

        return result;
    }

    private static class Choices
    {
        public const string LeaveWithoutSaving = "Leave without saving";
        public const string OverwriteCurrent = "Overwrite current [italic lightgreen](will always save a backup)[/]";
        public const string SaveAsNew = "Save as new";
        public const string Cancel = "Cancel";
    }
}
