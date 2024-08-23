using System.Web;
using Microsoft.AspNetCore.Components;
using PKHeX.Facade.Pokemons;
using PKHeX.Web.Plugins;
using PKHeX.Web.Services.Plugins;

namespace PKHeX.Web.Extensions;

public static class NavigationManagerExtensions
{
    public static string CurrentRoute(this NavigationManager navigation) =>
        navigation.Uri.Replace(navigation.BaseUri, string.Empty);
    
    public static void NavigateToHomePage(this NavigationManager navigation) =>
        navigation.NavigateTo("/");
    
    public static void NavigateToItems(this NavigationManager navigation) =>
        navigation.NavigateTo("/items");

    public static void NavigateToPokemon(this NavigationManager navigation, PokemonSource source, UniqueId uniqueId,
        bool replace = false) =>
        navigation.NavigateTo($"/pokemon/{source.RouteString()}/{uniqueId}", replace: replace);
    
    public static void NavigateToNewCloneOf(this NavigationManager navigation, UniqueId uniqueId) => 
        navigation.NavigateTo($"/pokemon/{uniqueId}/clone");
    
    public static void NavigateToPokemonBox(this NavigationManager navigation, bool replace = false) => 
        navigation.NavigateTo($"/pokemon-box", replace: replace);
    
    public static void NavigateToSearchEncounter(this NavigationManager navigation, bool replace = false) =>
        navigation.NavigateTo($"/pokemon/search-encounter", replace);
    
    public static void NavigateToSelectedEncounter(this NavigationManager navigation) =>
        navigation.NavigateTo($"/pokemon/selected-encounter");
    
    public static void NavigateToLoadedPokemon(this NavigationManager navigation) =>
        navigation.NavigateTo($"/pokemon/loaded-file");
    
    public static void NavigateToPlugInErrors(this NavigationManager navigation) =>
        navigation.NavigateTo($"/plugins/errors");
    
    public static void NavigateToPlugIns(this NavigationManager navigation) =>
        navigation.NavigateTo($"/plugins");
    
    public static void NavigateToPlugIn(this NavigationManager navigation, LoadedPlugIn plugIn) =>
        navigation.NavigateTo($"/plugins/{plugIn.Id}");
    
    public static void NavigateToPlugInPage(this NavigationManager navigation, string plugInId, string path,
        Outcome.PlugInPage.PageLayout layout) =>
        navigation.NavigateTo($"/plugins/{plugInId}/{path}/{layout.ToString().ToLowerInvariant()}");
    
    public static void NavigateToAnalyticsResults(this NavigationManager navigation) =>
        navigation.NavigateTo($"/analytics");
    
    public static void NavigateToSave(this NavigationManager navigation) =>
        navigation.NavigateTo($"/save");
    
    public static void NavigateToSettings(this NavigationManager navigation) =>
        navigation.NavigateTo($"/settings");
    
    public static void StoreOnQuery(this NavigationManager navigation, Dictionary<string, object?> parameters)
    {
        navigation.NavigateTo(navigation.GetUriWithQueryParameters(parameters));
    }
}