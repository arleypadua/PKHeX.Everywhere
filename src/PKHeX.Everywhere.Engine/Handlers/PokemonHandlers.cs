using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class PokemonHandlers
{
    [Query("pokemon.get", Topics.Party, Topics.Box)]
    public static PokemonSummary Get(Game game, PokemonHandle at) => game.Find(at).Pokemon.ToSummary(at);

    [Command("pokemon.setLevel")]
    public static void SetLevel(Game game, PokemonHandle at, int level)
    {
        if (level is < 1 or > 100)
            throw new EngineException(ErrorCodes.OutOfRange, $"Level must be between 1 and 100, got {level}.");

        var slot = game.Find(at);
        slot.Pokemon.ChangeLevel(level);
        slot.Commit();
    }
}
