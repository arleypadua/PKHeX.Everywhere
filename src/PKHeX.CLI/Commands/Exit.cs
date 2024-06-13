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
            Choices.LeaveWithoutSaving => ExitWithoutSaving(),
            Choices.OverwriteCurrent => Save.SaveExisting(game, settings),
            Choices.SaveAsNew => Save.SaveAsNew(game, settings),
            Choices.Cancel => Result.Continue,
            _ => Result.Continue
        };

        if (result != Result.Exit) return result;
        
        settings.PersistedSettings.LastSaveFilePath = settings.ResolveSaveFilePath();
        settings.PersistedSettings.Save();
            
        PrintExecuteHelpBanner(settings);

        return result;
    }

    private static Result ExitWithoutSaving()
    {
        AnsiConsole.MarkupLine("[yellow]Exited without saving[/]");
        return Result.Exit;
    }

    private static void PrintExecuteHelpBanner(PkCommand.Settings settings)
    {
        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine("[grey50 italic]To run PKHeX.CLI again:[/]");
        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine($"[grey50 italic]\t* execute [olive]pkhex-cli[/] to open the last used save file ({settings.ResolveSaveFilePath()})[/]");
        AnsiConsole.MarkupLine("[grey50 italic]\t* execute [olive]pkhex-cli /path/to/save-file[/] to open another save file[/]");
        AnsiConsole.MarkupLine(string.Empty);
    }

    private static class Choices
    {
        public const string LeaveWithoutSaving = "Leave without saving";
        public const string OverwriteCurrent = "Overwrite current [italic lightgreen](will always save a backup)[/]";
        public const string SaveAsNew = "Save as new";
        public const string Cancel = "Cancel";
    }
}
