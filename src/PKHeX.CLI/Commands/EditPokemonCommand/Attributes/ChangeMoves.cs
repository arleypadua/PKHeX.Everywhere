using PKHeX.CLI.Base;
using PKHeX.Facade;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Repositories;
using Spectre.Console;

namespace PKHeX.CLI.Commands.EditPokemonCommand.Attributes;

internal class ChangeMoves(Pokemon pokemon) : EditPokemonAttribute(pokemon)
{
    protected override string Label => "Moves";

    protected override string Value => Unassigned > 0
        ? $"{MovesDisplay} / {Unassigned} unassigned"
        : MovesDisplay;

    public override Result HandleSelection()
    {
        IEnumerable<EditPokemonAttribute> moveOptions =
        [
            new Move(Pokemon, PokemonMove.MoveIndex.Move1),
            new Move(Pokemon, PokemonMove.MoveIndex.Move2),
            new Move(Pokemon, PokemonMove.MoveIndex.Move3),
            new Move(Pokemon, PokemonMove.MoveIndex.Move4),
        ];

        RepeatUntilExit(() =>
        {
            var selection = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
                .Title(
                    $"{Environment.NewLine}[yellow]Moves of: [yellow]{Pokemon.NameDisplay()}[/][/]{Environment.NewLine}")
                .PageSize(5)
                .AddChoices(OptionOrBack.WithValues(
                    options: moveOptions,
                    display: (attribute) => attribute.Display))
                .WrapAround());

            return selection is OptionOrBack.Option<EditPokemonAttribute> attributeOption
                ? attributeOption.Value.HandleSelection()
                : Result.Exit;
        });

        return Result.Continue;
    }

    private int Unassigned => Pokemon.Moves.Values.Count(m => m.Move == MoveDefinition.None);

    private string MovesDisplay => string.Join(" / ",
        Pokemon.Moves.Values.Where(m => m.Move != MoveDefinition.None).Select(m => m.Move.Name));
}