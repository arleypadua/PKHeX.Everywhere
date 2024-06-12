using System.Text.RegularExpressions;
using PKHeX.CLI.Base;
using PKHeX.Facade;
using Spectre.Console;

namespace PKHeX.CLI.Commands;

public static class Save
{
    public static Result SaveExisting(Game game, PkCommand.Settings settings)
    {
        Backup(settings);
        File.WriteAllBytes(settings.ResolveSaveFilePath(), game.ToByteArray());
        AnsiConsole.MarkupLine($"[bold green]Successfully overwrite file at ${settings.SaveFilePath}[/]");
        return Result.Exit;
    }

    public static Result SaveAsNew(Game game, PkCommand.Settings settings)
    {
        var extension = Path.GetExtension(settings.SaveFilePath) ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(settings.SaveFilePath) ?? "savedata";
        fileName = Regex.Replace(fileName, @"_\d+", string.Empty);
        
        var workingPath = Path.GetDirectoryName(settings.SaveFilePath) ?? "./";
        var epochNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        var suggestedName = $"{fileName}_{epochNow}{extension}";
        var suggestedFilePath = Path.Combine(workingPath, suggestedName);
        

        var savePath = AnsiConsole.Ask("Enter the path to save the new file", suggestedFilePath);

        File.WriteAllBytes(String.IsNullOrWhiteSpace(savePath) ? suggestedFilePath : savePath, game.ToByteArray());
        return Result.Exit;
    }

    public static void Backup(PkCommand.Settings settings)
    {
        if (BackupFile.BackupExists(settings.ResolveSaveFilePath(), out var existing))
        {
            AnsiConsole.MarkupLine($"[yellow]Backup already exists on path: {existing}[/]");
            return;
        }

        var backupName = $"{settings.SaveFilePath}.{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.backup"; 
        File.Copy(settings.ResolveSaveFilePath(), backupName);
        AnsiConsole.MarkupLine($"[bold green]Created backup at ${backupName}[/]");
    }
}