using System.Web;
using Microsoft.AspNetCore.Components;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Extensions;

public static class NavigationManagerExtensions
{
    public static string CurrentRoute(this NavigationManager navigation) =>
        navigation.Uri.Replace(navigation.BaseUri, string.Empty);
    
    public static void NavigateToHomePage(this NavigationManager navigation) =>
        navigation.NavigateTo("/");
    
    public static void NavigateToItems(this NavigationManager navigation) =>
        navigation.NavigateTo("/items");

    public static void NavigateToPokemonBox(this NavigationManager navigation, bool replace = false) => 
        navigation.NavigateTo($"/pokemon-box", replace: replace);
    
    public static void NavigateToSearchEncounter(this NavigationManager navigation, bool replace = false) =>
        navigation.NavigateTo($"/pokemon/search-encounter", replace);
    
    public static void NavigateToPlugInErrors(this NavigationManager navigation) =>
        navigation.NavigateTo($"/plugins/errors");
    
    public static void NavigateToPlugIns(this NavigationManager navigation) =>
        navigation.NavigateTo($"/plugins");
    
    public static void NavigateToPlugIn(this NavigationManager navigation, string plugInId) =>
        navigation.NavigateTo($"/plugins/{plugInId}");
    
    public static void NavigateToPlugInPage(this NavigationManager navigation, string plugInId, string path,
        string layout, bool replace = false) =>
        navigation.NavigateTo($"/plugins/{plugInId}/{path}/{layout.ToLowerInvariant()}", replace: replace);
    
    public static void NavigateToAnalyticsResults(this NavigationManager navigation) =>
        navigation.NavigateTo($"/analytics");
    
    public static void NavigateToSave(this NavigationManager navigation) =>
        navigation.NavigateTo($"/save");
    
    public static void NavigateToSettings(this NavigationManager navigation) =>
        navigation.NavigateTo($"/settings");

    public static void NavigateToReleaseNotes(this NavigationManager navigation, DateOnly? since = null) =>
        navigation.NavigateTo($"/release-notes?since={since?.ToString("yyyy-MM-dd")}");
    
    public static void StoreOnQuery(this NavigationManager navigation, Dictionary<string, object?> parameters)
    {
        navigation.NavigateTo(navigation.GetUriWithQueryParameters(parameters));
    }
}