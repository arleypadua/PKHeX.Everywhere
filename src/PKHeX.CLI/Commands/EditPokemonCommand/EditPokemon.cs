using PKHeX.CLI.Base;
using PKHeX.CLI.Commands.EditPokemonCommand.Attributes;
using PKHeX.Facade;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;
using Spectre.Console;
using MetConditions = PKHeX.CLI.Commands.EditPokemonCommand.Attributes.MetConditions;
using Stats = PKHeX.CLI.Commands.EditPokemonCommand.Attributes.Stats;

namespace PKHeX.CLI.Commands.EditPokemonCommand;

public static class EditPokemon
{
    public static Result Handle(Game game, Pokemon pokemon)
    {
        RepeatUntilExit(() =>
        {
            IEnumerable<EditPokemonAttribute> attributes =
            [
                new Legal(pokemon),
                new EditPokemonAttribute.ReadOnlyAttribute(pokemon, "PID", () => pokemon.PID.ToString("X8")),
                new EditPokemonAttribute.IsShiny(pokemon),
                new EditPokemonAttribute.Name(pokemon),
                new EditPokemonAttribute.Level(pokemon),
                new EditPokemonAttribute.Nature(pokemon),
                new EditPokemonAttribute.ReadOnlyAttribute(pokemon, "Held item", pokemon.HeldItem.Name),
                new EditPokemonAttribute.ReadOnlyAttribute(pokemon, "Ability", pokemon.Ability.Name),
                new EditPokemonAttribute.ReadOnlyAttribute(pokemon, "Friendship", pokemon.Friendship.ToString()),
                new EditPokemonAttribute.Flags(pokemon),

                new MetConditions(pokemon),
                new ChangeMoves(pokemon),
                new Stats(pokemon),
            ];
        
            attributes = attributes.Where(a => !a.Hidden);
            
            var selection = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
                .Title(
                    $"{Environment.NewLine}[yellow]Editing Pokemon: [yellow]{pokemon.NameDisplay()}[/][/]{Environment.NewLine}")
                .PageSize(13)
                .AddChoices(OptionOrBack.WithValues(
                    options: attributes,
                    display: (attribute) => attribute.Display))
                .WrapAround());

            return selection is OptionOrBack.Option<EditPokemonAttribute> attributeOption
                ? attributeOption.Value.HandleSelection()
                : Result.Exit;
        });

        return Result.Continue;
    }
}