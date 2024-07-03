using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Extensions;

public static class PokemonSourceExtensions
{
    public static string RouteString(this PokemonSource pokemonSource) => pokemonSource.ToString().ToLowerInvariant();
}