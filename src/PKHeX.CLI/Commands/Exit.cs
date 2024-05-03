using System.Text.RegularExpressions;
using PKHeX.CLI.Base;
using PKHeX.Facade;
using Spectre.Console;

namespace PKHeX.CLI;

public static class Exit
{
    public static Result Handle(Game game, PkCommand.Settings settings)
    {
        var answer = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("There may be changes to your save, what would you like to do?")
            .AddChoices(["Leave without saving", "Overwrite current", "Save as new", "Cancel"]));

        return answer switch
        {
            "Leave without saving" => Result.Exit,
            "Overwrite current" => SaveExisting(game, settings),
            "Save as new" => SaveAsNew(game, settings),
            "Cancel" => Result.Continue,
            _ => Result.Continue
        };
    }

    private static Result SaveExisting(Game game, PkCommand.Settings settings)
    {
        File.WriteAllBytes(settings.SaveFilePath, game.ToByteArray());
        return Result.Exit;
    }

    private static Result SaveAsNew(Game game, PkCommand.Settings settings)
    {
        var extension = Path.GetExtension(settings.SaveFilePath) ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(settings.SaveFilePath) ?? "savedata";
        fileName = Regex.Replace(fileName, @"_\d+", string.Empty);
        
        var workingPath = Path.GetDirectoryName(settings.SaveFilePath) ?? "./";
        var epochNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        var suggestedName = $"{fileName}_{epochNow}{extension}";
        var suggestedFilePath = Path.Combine(workingPath, suggestedName);
        

        var savePath = AnsiConsole.Ask("Enter the path to save the new file", suggestedFilePath)
            ?? suggestedFilePath;

        File.WriteAllBytes(savePath, game.ToByteArray());
        return Result.Exit;
    }
}
