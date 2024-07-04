using System.Web;
using Microsoft.AspNetCore.Components;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Extensions;

public static class NavigationManagerExtensions
{
    public static string CurrentRoute(this NavigationManager navigation) =>
        navigation.Uri.Replace(navigation.BaseUri, string.Empty);

    public static void NavigateToPokemon(this NavigationManager navigation, PokemonSource source, UniqueId uniqueId,
        bool replace = false) =>
        navigation.NavigateTo($"/pokemon/{source.RouteString()}/{uniqueId}", replace: replace);
    
    public static void NavigateToNewCloneOf(this NavigationManager navigation, UniqueId uniqueId) => 
        navigation.NavigateTo($"/pokemon/{uniqueId}/clone");
    
    public static void NavigateToPokemonBox(this NavigationManager navigation, bool replace = false) => 
        navigation.NavigateTo($"/pokemon-box", replace: replace);
    
    public static void NavigateToSearchEncounter(this NavigationManager navigation) =>
        navigation.NavigateTo($"/pokemon/search-encounter");

    public static void StoreOnQuery(this NavigationManager navigation, Dictionary<string, object?> parameters)
    {
        navigation.NavigateTo(navigation.GetUriWithQueryParameters(parameters));
    }
}