using Microsoft.AspNetCore.Components;
using PKHeX.Facade.Pokemons;
using PKHeX.Web.Model;

namespace PKHeX.Web.Extensions;

public static class NavigationManagerExtensions
{
    public static string CurrentRoute(this NavigationManager navigation) => navigation.Uri.Replace(navigation.BaseUri, string.Empty);

    public static void NavigateToPokemon(this NavigationManager navigation, PokemonSource source, UniqueId uniqueId, bool replace = false) =>
        navigation.NavigateTo($"/pokemon/{source.RouteString()}/{uniqueId}", replace: replace);

}