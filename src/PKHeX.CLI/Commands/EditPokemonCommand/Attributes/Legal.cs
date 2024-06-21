using PKHeX.CLI.Base;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Extensions;
using Spectre.Console;

namespace PKHeX.CLI.Commands.EditPokemonCommand.Attributes;

internal class Legal(Pokemon pokemon) : EditPokemonAttribute(pokemon)
{
    private readonly LegalityAnalysis _legalityAnalysis = pokemon.Legality();

    protected override string Label => "Legal";

    protected override string Value => Pokemon.Legality().Valid
        ? string.Empty
        : "[red bold]Illegal[/]";

    public override bool Hidden => _legalityAnalysis.Valid;

    public override Result HandleSelection()
    {
        var displays = _legalityAnalysis.Results.Select(a => a.Judgement switch
        {
            Severity.Valid => $"* [green]{a.Identifier}: {a.Comment}[/]",
            Severity.Fishy => $"* [yellow]{a.Identifier}: {a.Comment}[/]",
            Severity.Invalid => $"* [red]{a.Identifier}: {a.Comment}[/]",
            _ => string.Empty,
        });
                
        var display = string.Join(Environment.NewLine, displays);
                
        AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
            .Title(display)
            .PageSize(10)
            .AddChoices(OptionOrBack.WithValues(
                options: Enumerable.Empty<OptionOrBack>()))
            .WrapAround());
                
        return Result.Continue;
    }
}