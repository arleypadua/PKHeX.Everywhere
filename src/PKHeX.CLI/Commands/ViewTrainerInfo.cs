using PKHeX.CLI.Base;
using PKHeX.Facade;
using Spectre.Console;

namespace PKHeX.CLI;

public static class ViewTrainerInfo
{
    public static Result Handle(Game game)
    {
        AnsiConsole.MarkupLine($"[bold darkgreen]Trainer Info[/]");
        Console.WriteLine();
        AnsiConsole.MarkupLine($"[bold darkgreen]TID/SID:[/] {game.Trainer.Id}");
        AnsiConsole.MarkupLine($"[bold darkgreen]Name:[/] {game.Trainer.Name}");
        AnsiConsole.MarkupLine($"[bold darkgreen]Gender:[/] {game.Trainer.Gender}");
        AnsiConsole.MarkupLine($"[bold darkgreen]Money:[/] [yellow]{game.Trainer.Money}[/]");
        
        Console.WriteLine();

        AnsiConsole.MarkupLine($"[bold darkgreen]Rival:[/] {game.Trainer.RivalName}");

        Console.WriteLine();

        return Result.Continue;
    }
}
