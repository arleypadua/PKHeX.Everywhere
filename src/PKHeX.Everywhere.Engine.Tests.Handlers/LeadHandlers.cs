using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Tests.Handlers;

public record Lead(PokemonHandle At, string Species, int Level);

public static class LeadHandlers
{
    public static void AddTo(Session session) => session.AddHandlers(HandlerRegistry.TryInvoke);

    [Query("lead.get", Topics.Party)]
    public static Lead Get(Game game)
    {
        var pokemon = game.Trainer.Party.Pokemons[0];
        return new Lead(PokemonHandle.Party(0), pokemon.Species.Name, pokemon.Level);
    }

    [Command("lead.setLevel")]
    public static void SetLevel(Game game, PokemonHandle at, int level)
    {
        if (level is < 1 or > 100)
            throw new EngineException(ErrorCodes.OutOfRange, $"Level must be between 1 and 100, got {level}.");

        game.Trainer.Party.Pokemons[at.Slot].ChangeLevel(level);
        game.Trainer.Party.Commit();
    }

    [Command("lead.refresh", Topics.All)]
    public static void Refresh(Game game)
    {
    }
}
