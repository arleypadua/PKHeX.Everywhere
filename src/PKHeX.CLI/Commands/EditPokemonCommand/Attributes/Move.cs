using PKHeX.CLI.Base;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;
using Spectre.Console;

namespace PKHeX.CLI.Commands.EditPokemonCommand.Attributes;

internal class Move(Pokemon pokemon, PokemonMove.MoveIndex moveIndex) : EditPokemonAttribute(pokemon)
{
    protected override string Label => $"Move #{(int)moveIndex + 1}";
    protected override string Value => $"{PokemonMove.Move.Name,-20}PP: {PokemonMove.PP,-10}";

    public override string Display => IsLegal()
        ? base.Display
        : $"[red]{Label}: {Value}[/]{GetIllegalDisplay()}";

    public override Result HandleSelection()
    {
        var alreadyAssignedMoves = Pokemon.Moves
            .Where(m => m.Value.Move != MoveDefinition.None)
            .Select(m => m.Value.Move.Id)
            .ToList();

        var possibleMoves = MoveRepository.Instance.PossibleMovesFor(Pokemon)
            .Where(m => !alreadyAssignedMoves.Contains(m.Id))
            .Append(MoveDefinition.None);

        var selectedOption = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
            .Title($"[red]Changing[/] {Display}")
            .PageSize(10)
            .AddChoices(OptionOrBack.WithValues(
                options: possibleMoves,
                display: move => move.Name))
            .EnableSearch()
            .WrapAround());

        if (selectedOption is OptionOrBack.Option<MoveDefinition> moveDefinition)
        {
            Pokemon.ChangeMove(moveIndex, moveDefinition.Value);
        }

        return Result.Continue;
    }

    private bool IsLegal() => MoveRepository.Instance
        .PossibleMovesFor(Pokemon)
        .Append(MoveDefinition.None)
        .Any(m => m == Pokemon.Moves[moveIndex].Move);

    private string GetIllegalDisplay() => IsLegal()
        ? string.Empty
        : " - [yellow]Illegal move[/]";
        
    private PokemonMove PokemonMove => Pokemon.Moves[moveIndex];
}