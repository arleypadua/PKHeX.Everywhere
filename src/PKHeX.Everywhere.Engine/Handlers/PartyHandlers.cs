using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class PartyHandlers
{
    [Query("party.get")]
    public static PokemonSummary[] Get(Game game) => game.Trainer.Party.Pokemons
        .Select((pokemon, slot) => pokemon.ToSummary(PokemonHandle.Party(slot)))
        .ToArray();
}
