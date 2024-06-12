using PKHeX.CLI.Base;
using PKHeX.CLI.Extensions;
using PKHeX.Facade;
using Spectre.Console;

namespace PKHeX.CLI.Commands;

public static class ShowPokemonParty
{
    public static Result Handle(Game game)
    {
        RepeatUntilExit(() =>
        {
            var party = game.Trainer.Party.Pokemons;

            var selection = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
                .Title("Which Pokémon would you like to view?")
                .PageSize(10)
                .AddChoices(OptionOrBack.WithValues(
                    options: party,
                    display: (pokemon) => pokemon.GetPokemonDisplay()))
                .WrapAround(true));

            return selection switch
            {
                OptionOrBack.Back => Result.Exit,
                OptionOrBack.Option<Pokemon> option => Result.Exit, // TODO: Implement the Pokémon view
                _ => Result.Exit
            };
        });

        return Result.Continue;
    }

    
}
