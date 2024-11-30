using PKHeX.CLI.Base;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;
using Spectre.Console;

namespace PKHeX.CLI.Commands.EditPokemonCommand.Attributes;

internal class CapturedWith(Pokemon pokemon) : EditPokemonAttribute.SimpleAttribute(pokemon, "Captured with", () => pokemon.Ball.Name)
{
    public override Result HandleSelection()
    {
        var balls = GetLegalBalls(Pokemon.Pkm)
            .Select(b => ItemRepository.GetItem((ushort)b));

        var ball = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
            .Title(Display)
            .PageSize(10)
            .AddChoices(OptionOrBack.WithValues(
                options: balls,
                display: ball => ball.Name))
            .WrapAround());

        if (ball is OptionOrBack.Option<ItemDefinition> selectedBall)
        {
            Pokemon.Ball = selectedBall.Value;
        }

        return Result.Continue;
    }
    
    private static IEnumerable<Ball> GetLegalBalls(PKM pk)
    {
        var clone = pk.Clone();
        foreach (var b in Enum.GetValues<Ball>())
        {
            var ball = (byte)b;
            clone.Ball = ball;
            if (clone.Ball != ball)
                continue; // Some setters guard against out of bounds values.
            if (new LegalityAnalysis(clone).Valid)
                yield return b;
        }
    }
}