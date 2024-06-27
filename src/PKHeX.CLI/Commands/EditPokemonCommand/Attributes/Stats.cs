using PKHeX.CLI.Base;
using PKHeX.Facade.Pokemons;
using Spectre.Console;

namespace PKHeX.CLI.Commands.EditPokemonCommand.Attributes;

class Stats(Pokemon pokemon) : EditPokemonAttribute(pokemon)
{
    protected override string Label => "Stats";
    protected override string Value => $"[yellow]Base:[/] {Pokemon.BaseStats.Total,-3} " +
                                       $"[yellow]IVs:[/] {Pokemon.IVs.Total,-3} " +
                                       $"[yellow]EVs:[/] {Pokemon.EVs.Total,-3} ";

    public override Result HandleSelection()
    {
        RepeatUntilExit(() =>
        {
            IEnumerable<EditPokemonAttribute> options =
            [
                new BaseStats(Pokemon),
                new IV(Pokemon),
                new EV(Pokemon),
            ];
            
            var selectedOption = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
                .Title(Display)
                .PageSize(10)
                .AddChoices(OptionOrBack.WithValues(
                    options: options,
                    display: option => option.Display))
                .WrapAround());

            return selectedOption is OptionOrBack.Option<EditPokemonAttribute> attributeOption
                ? attributeOption.Value.HandleSelection()
                : Result.Exit;
        });
        
        return Result.Continue;
    }
}