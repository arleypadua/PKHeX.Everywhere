﻿using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PKHeX.CLI;
using PKHeX.CLI.Base;
using PKHeX.CLI.Commands;
using PKHeX.CLI.Facade;
using PKHeX.Core;
using Spectre.Console;
using Spectre.Console.Cli;

public static class Program
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PkCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PkCommand.Settings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.ExplainCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.VersionCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.XmlDocCommand", "Spectre.Console.Cli")]
    public static void Main(string[] args)
    {
        var app = new CommandApp<PkCommand>();
        app.Run(args);
    }

}

public sealed partial class PkCommand : Command<PkCommand.Settings>
{
    public override int Execute(CommandContext context, Settings settings)
    {
        var saveFile = LoadSaveFile(settings.SaveFilePath);
        if (saveFile is null) return 1;

        PrintHeader();

        return Run(saveFile, settings);
    }

    private void PrintHeader()
    {
        AnsiConsole.Write(
            new FigletText("PKHeX CLI")
                .LeftJustified()
                .Color(Color.Red));
    }

    private SaveFile? LoadSaveFile(string path)
    {
        var saveFileObject = FileUtil.GetSupportedFile(path);
        if (saveFileObject is null)
        {
            AnsiConsole.MarkupLine($"[bold red]Failed to load save file at {path}[/]");
            return null;
        }

        if (saveFileObject is not SaveFile saveFile)
        {
            AnsiConsole.MarkupLine($"[bold red]Unsupported save file ({saveFileObject.GetType().Name}) at {path}[/]");
            return null;
        }

        AnsiConsole.MarkupLine($"[bold green]Loaded save file at {path}[/]");

        return saveFile;
    }

    private int Run(SaveFile saveFile, Settings settings)
    {
        var game = new Game(saveFile);

        RepeatUntilExit(() =>
        {
            var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
                            .Title($"Hello [yellow bold]{game.Trainer.Name.ToUpperInvariant()}[/] What would you like to do?")
                            .PageSize(10)
                            .AddChoices(["View Trainer Info", "View/Edit Inventory", "Exit"])
                            .WrapAround(true));

            return selection switch
            {
                "View Trainer Info" => ViewTrainerInfo.Handle(game),
                "View/Edit Inventory" => ViewInventory.Handle(game),
                "Exit" => Exit.Handle(game, settings),
                _ => Result.Continue
            };
        });

        return 0;
    }

    public sealed class Settings : CommandSettings
    {
        public Settings() { }

        [Description("The path to the save file. Default to ./data/savedata.bin")]
        [CommandArgument(0, "[savefile]")]
        public string SaveFilePath { get; set; } = "./data/savedata.bin";
    }
}