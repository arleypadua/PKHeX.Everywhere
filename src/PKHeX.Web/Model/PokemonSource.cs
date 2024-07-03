namespace PKHeX.Web.Model;

public enum PokemonSource
{
    Party,
    Box
}

public static class PokemonSourceExtensions
{
    public static string RouteString(this PokemonSource pokemonSource) => pokemonSource.ToString().ToLowerInvariant();
}