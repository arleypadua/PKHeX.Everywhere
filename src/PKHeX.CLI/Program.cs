using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using PKHeX.CLI.Base;
using PKHeX.CLI.Commands;
using PKHeX.Core;
using PKHeX.Facade;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PKHeX.CLI;

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

public sealed class PkCommand : Command<PkCommand.Settings>
{
    public override int Execute(CommandContext context, Settings settings)
    {
        PrintHeader();
        
        return Run(settings.ResolveSaveFilePath(), settings);
    }

    private void PrintHeader()
    {
        AnsiConsole.Write(
            new FigletText("PKHeX CLI")
                .LeftJustified()
                .Color(Color.Red));
    }

    private int Run(string path, Settings settings)
    {
        var game = Game.LoadFrom(path);
        
        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine($"Successfully loaded the save state at: [blue]{path}[/]");
        AnsiConsole.MarkupLine(string.Empty);

        RepeatUntilExit(() =>
        {
            var selection = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title($"Hello [yellow bold]{game.Trainer.Name.ToUpperInvariant()}[/]! What would you like to do?")
                .PageSize(10)
                .AddChoices(
                    Choices.ViewTrainerInfo,
                    Choices.ViewPokemonParty,
                    Choices.ViewInventory,
                    Choices.RestoreBackup,
                    Choices.Exit)
                .WrapAround());

            return selection switch
            {
                Choices.ViewTrainerInfo => ViewTrainerInfo.Handle(game),
                Choices.ViewPokemonParty => ShowPokemonParty.Handle(game),
                Choices.ViewInventory => ViewInventory.Handle(game),
                Choices.Exit => Exit.Handle(game, settings),
                Choices.RestoreBackup => RestoreBackup.Handle(game, settings),
                _ => Result.Continue
            };
        });
        
        return 0;
    }
    
    private static class Choices
    {
        public const string ViewTrainerInfo = "View Trainer Info";
        public const string ViewPokemonParty = "View/Edit Pokémon Party";
        public const string ViewInventory = "View/Edit Inventory";
        public const string RestoreBackup = "Restore Backup";
        public const string Exit = "Exit";
    }

    public sealed class Settings : CommandSettings
    {
        [Description("The path to the save file. Default to ./data/savedata.bin")]
        [CommandArgument(0, "[savefile]")]
        public string? SaveFilePath { get; set; }

        public PersistedSettings PersistedSettings { get; } = PersistedSettings.Load();
        
        public string ResolveSaveFilePath() => SaveFilePath ?? PersistedSettings.LastSaveFilePath ?? LocalDebugSaveFile;
        
        private const string LocalDebugSaveFile = "./data/savedata.bin";
    }
}