using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Extensions;

public static class GameExtensions
{
    public static Pokemon? FindPokemon(this Game game, UniqueId uniqueId) =>
        game.Trainer.Party.Pokemons.FirstOrDefault(p => p.UniqueId.Equals(uniqueId))
        ?? game.Trainer.PokemonBox.All.FirstOrDefault(p => p.UniqueId.Equals(uniqueId));
}