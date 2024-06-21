using PKHeX.CLI.Base;
using PKHeX.CLI.Commands.EditPokemonCommand;
using PKHeX.CLI.Extensions;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Extensions;
using Spectre.Console;

namespace PKHeX.CLI.Commands;

public static class ShowPokemonBox
{
    public static Result Handle(Game game)
    {
        RepeatUntilExit(() =>
        {
            var options = game.Trainer.PokemonBox.BySpecies
                .Select(p => p.Value)
                .Select(PokemonBoxChoice.From);

            var selection = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
                .Title("Which Pokémon/group would you like to view?")
                .PageSize(10)
                .EnableSearch()
                .AddChoices(OptionOrBack.WithValues(
                    options: options,
                    display: pokemon => pokemon.Display()))
                .WrapAround());

            return selection switch
            {
                OptionOrBack.Back => Result.Exit,
                OptionOrBack.Option<PokemonBoxChoice> choice => choice.Value switch
                {
                    PokemonBoxChoice.Single pokemonChoice => EditPokemon.Handle(game, pokemonChoice.Pokemon),
                    PokemonBoxChoice.Group groupChoice => HandleGroupChoice(game, groupChoice),
                    _ => Result.Exit
                },
                _ => Result.Exit
            };
        });
        
        game.Trainer.PokemonBox.Commit();

        return Result.Continue;
    }

    private static Result HandleGroupChoice(Game game, PokemonBoxChoice.Group choice)
    {
        ShowPokemons.Handle(game, choice.Pokemons);
        return Result.Continue;
    }

    abstract class PokemonBoxChoice
    {
        public abstract Species Species { get; }
        public abstract string Display();

        public class Group(IList<Pokemon> pokemons) : PokemonBoxChoice
        {
            public IList<Pokemon> Pokemons => pokemons;
            public override Species Species { get; } = pokemons[0].Species;
            public override string Display() => $"(#{(int)Species:000}) {Species.Name()} ({Pokemons.Count} pokemons)";
        }

        public class Single(Pokemon pokemon) : PokemonBoxChoice
        {
            public Pokemon Pokemon { get; } = pokemon;
            public override Species Species { get; } = pokemon.Species;
            public override string Display() => $"(#{(int)Species:000}) {Pokemon.GetPokemonDisplay(includeLegalityFlag: true)}";
        }

        public static PokemonBoxChoice From(IList<Pokemon> pokemons) => pokemons switch
        {
            { Count: 0 } => throw new InvalidOperationException("At least one pokemon is required per group"),
            { Count: 1 } pokemon => new Single(pokemons[0]),
            _ => new Group(pokemons)
        };
    }
}