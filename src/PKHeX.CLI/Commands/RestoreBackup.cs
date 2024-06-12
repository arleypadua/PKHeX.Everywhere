using PKHeX.CLI.Base;
using PKHeX.Facade;
using Spectre.Console;

namespace PKHeX.CLI.Commands;

public static class RestoreBackup
{
    public static Result Handle(Game game, PkCommand.Settings settings)
    {
        var backupFiles = BackupFile.GetBackupFilesFor(settings.ResolveSaveFilePath());

        if (backupFiles.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No backup files found.[/]");
            return Result.Continue;
        }

        var selectedOption = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
            .AddChoices(OptionOrBack.WithValues(
                options: backupFiles, 
                display: (item) => $"[yellow]{item.Date:dd/MM/yyyy hh:mm:ss}[/] [grey53 italic]{item.FilePath}[/]"))
            .Title($"[bold darkgreen]Backups available[/]")
            .PageSize(10)
            .EnableSearch()
            .WrapAround());
        
        return selectedOption switch
        {
            OptionOrBack.Option<BackupFile> backup => HandleBackup(settings, backup.Value),
            OptionOrBack.Back => Result.Continue,
            _ => Result.Continue,
        };
    }

    private static Result HandleBackup(PkCommand.Settings settings, BackupFile backupFile)
    {
        Save.Backup(settings);
        
        File.Copy(backupFile.FilePath, settings.ResolveSaveFilePath(), overwrite: true);
        AnsiConsole.MarkupLine($"Backup restored: {backupFile.FilePath}");
        
        return Result.Exit;
    }
}