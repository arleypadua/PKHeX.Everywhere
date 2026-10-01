using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Extensions;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class PartyHandlers
{
    [Query("party.get", Topics.Party)]
    public static PokemonSummary[] Get(Game game) => game.Trainer.Party.Pokemons
        .Select((pokemon, slot) => pokemon.ToSummary(PokemonHandle.Party(slot)))
        .ToArray();

    [Query("party.showdown", Topics.Party)]
    public static string Showdown(Game game) => game.Trainer.Party.Pokemons.Showdown();
}
