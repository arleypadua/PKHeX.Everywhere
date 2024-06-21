using PKHeX.CLI.Base;
using PKHeX.CLI.Commands.EditPokemonCommand;
using PKHeX.CLI.Extensions;
using PKHeX.Facade;
using Spectre.Console;

namespace PKHeX.CLI.Commands;

public static class ShowPokemons
{
    public static Result Handle(Game game, IEnumerable<Pokemon> pokemons) => RepeatUntilExit(() =>
    {
        var selection = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
            .Title("Which Pokémon would you like to view?")
            .PageSize(10)
            .AddChoices(OptionOrBack.WithValues(
                options: pokemons,
                display: pokemon => pokemon.GetPokemonDisplay(includeLegalityFlag: true)))
            .WrapAround());

        return selection switch
        {
            OptionOrBack.Back => Result.Exit,
            OptionOrBack.Option<Pokemon> option => EditPokemon.Handle(game, option.Value),
            _ => Result.Exit
        };
    });
}