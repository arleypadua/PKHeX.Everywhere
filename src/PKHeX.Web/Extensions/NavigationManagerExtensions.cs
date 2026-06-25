using System.Web;
using Microsoft.AspNetCore.Components;
using PKHeX.Facade.Pokemons;
using PKHeX.Web.Plugins;
using PKHeX.Web.Services.Plugins;

namespace PKHeX.Web.Extensions;

public static class NavigationManagerExtensions
{
    private static string WithPathBase(this NavigationManager navigation, string uri)
    {
        var pathBase = new Uri(navigation.BaseUri).AbsolutePath.TrimEnd('/');
        return pathBase + uri;
    }

    public static string CurrentRoute(this NavigationManager navigation) =>
        navigation.Uri.Replace(navigation.BaseUri, string.Empty);
    
    public static void NavigateToHomePage(this NavigationManager navigation) =>
        navigation.NavigateTo(navigation.WithPathBase("/"));

    public static void NavigateToLoad(this NavigationManager navigation) =>
        navigation.NavigateTo(navigation.WithPathBase("/load"));
    
    public static void NavigateToItems(this NavigationManager navigation) =>
        navigation.NavigateTo(navigation.WithPathBase("/items"));

    public static void NavigateToPokemon(this NavigationManager navigation, PokemonSource source, UniqueId uniqueId,
        bool replace = false) =>
        navigation.NavigateTo(navigation.WithPathBase($"/pokemon/{source.RouteString()}/{uniqueId}"), replace: replace);
    
    public static void NavigateToNewCloneOf(this NavigationManager navigation, UniqueId uniqueId) => 
        navigation.NavigateTo(navigation.WithPathBase($"/pokemon/{uniqueId}/clone"));
    
    public static void NavigateToPokemonBox(this NavigationManager navigation, bool replace = false) => 
        navigation.NavigateTo(navigation.WithPathBase($"/pokemon-box"), replace: replace);
    
    public static void NavigateToSearchEncounter(this NavigationManager navigation, bool replace = false) =>
        navigation.NavigateTo(navigation.WithPathBase($"/pokemon/search-encounter"), replace);
    
    public static void NavigateToSelectedEncounter(this NavigationManager navigation) =>
        navigation.NavigateTo(navigation.WithPathBase($"/pokemon/selected-encounter"));
    
    public static void NavigateToLoadedPokemon(this NavigationManager navigation) =>
        navigation.NavigateTo(navigation.WithPathBase($"/pokemon/loaded-file"));
    
    public static void NavigateToPlugInErrors(this NavigationManager navigation) =>
        navigation.NavigateTo(navigation.WithPathBase($"/plugins/errors"));
    
    public static void NavigateToPlugIns(this NavigationManager navigation) =>
        navigation.NavigateTo(navigation.WithPathBase($"/plugins"));
    
    public static void NavigateToPlugIn(this NavigationManager navigation, LoadedPlugIn plugIn) =>
        navigation.NavigateTo(navigation.WithPathBase($"/plugins/{plugIn.Id}"));
    
    public static void NavigateToPlugInPage(this NavigationManager navigation, string plugInId, string path,
        Outcome.PlugInPage.PageLayout layout) =>
        navigation.NavigateTo(navigation.WithPathBase($"/plugins/{plugInId}/{path}/{layout.ToString().ToLowerInvariant()}"));
    
    public static void NavigateToAnalyticsResults(this NavigationManager navigation) =>
        navigation.NavigateTo(navigation.WithPathBase($"/analytics"));
    
    public static void NavigateToSave(this NavigationManager navigation) =>
        navigation.NavigateTo(navigation.WithPathBase($"/save"));
    
    public static void NavigateToSettings(this NavigationManager navigation) =>
        navigation.NavigateTo(navigation.WithPathBase($"/settings"));

    public static void NavigateToSharedPokemon(this NavigationManager navigation, Guid id) =>
        navigation.NavigateTo(navigation.WithPathBase($"/s/{id}"));
    
    public static void NavigateToCloudPokemonList(this NavigationManager navigation) =>
        navigation.NavigateTo(navigation.WithPathBase($"/cloud/pokemon"));
    
    public static void NavigateToCloudPokemon(this NavigationManager navigation, Guid id) =>
        navigation.NavigateTo(navigation.WithPathBase($"/cloud/pokemon/{id}"));
    
    public static void NavigateToReleaseNotes(this NavigationManager navigation, DateOnly? since = null) =>
        navigation.NavigateTo(navigation.WithPathBase($"/release-notes?since={since?.ToString("yyyy-MM-dd")}"));
    
    public static void StoreOnQuery(this NavigationManager navigation, Dictionary<string, object?> parameters)
    {
        navigation.NavigateTo(navigation.GetUriWithQueryParameters(parameters));
    }
}