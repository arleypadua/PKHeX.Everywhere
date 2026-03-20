using PKHeX.CLI.Base;
using PKHeX.Core;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;
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
        var localization = LegalityLocalizationContext.Create(_legalityAnalysis);
        var displays = new List<string>(_legalityAnalysis.Results.Count);
        foreach (var result in _legalityAnalysis.Results)
        {
            var message = localization.Humanize(result);
            displays.Add(result.Judgement switch
            {
                Severity.Valid => $"* [green]{result.Identifier}: {message}[/]",
                Severity.Fishy => $"* [yellow]{result.Identifier}: {message}[/]",
                Severity.Invalid => $"* [red]{result.Identifier}: {message}[/]",
                _ => string.Empty,
            });
        }
                
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