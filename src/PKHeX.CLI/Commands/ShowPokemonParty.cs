using PKHeX.CLI.Base;
using PKHeX.CLI.Extensions;
using PKHeX.Facade;
using Spectre.Console;

namespace PKHeX.CLI.Commands;

public static class ShowPokemonParty
{
    public static Result Handle(Game game)
    {
        RepeatUntilExit(() => ShowPokemons.Handle(game, game.Trainer.Party.Pokemons));
        
        game.Trainer.Party.Commit();

        return Result.Continue;
    }
}
