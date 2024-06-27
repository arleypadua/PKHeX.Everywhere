using PKHeX.CLI.Base;
using PKHeX.Facade.Pokemons;
using Spectre.Console;

namespace PKHeX.CLI.Commands.EditPokemonCommand.Attributes;

internal class MetConditions(Pokemon pokemon) : EditPokemonAttribute(pokemon)
{
    protected override string Label => "Met conditions";
    protected override string Value => $"{Pokemon.MetConditions.Version.Name} / {Pokemon.MetConditions.Location.Name} / {Pokemon.Ball.Name}";

    public override Result HandleSelection()
    {
        RepeatUntilExit(() =>
        {
            IEnumerable<EditPokemonAttribute> options =
            [
                new ReadOnlyAttribute(Pokemon, "Origin game", Pokemon.MetConditions.Version.Name),
                new CapturedWith(Pokemon),
                new ReadOnlyAttribute(Pokemon, "Location", Pokemon.MetConditions.Location.Name),
                new ReadOnlyAttribute(Pokemon, "Date", Pokemon.Pkm.MetDate?.ToString() ?? "N/A"),
                new ReadOnlyAttribute(Pokemon, "Egg Location", Pokemon.MetConditions.Location.Name),
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