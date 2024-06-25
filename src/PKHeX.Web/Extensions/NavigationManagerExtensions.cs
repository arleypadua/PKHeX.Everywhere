using Microsoft.AspNetCore.Components;

namespace PKHeX.Web.Extensions;

public static class NavigationManagerExtensions
{
    public static string? CurrentRoute(this NavigationManager navigation) => navigation.Uri.Replace(navigation.BaseUri, string.Empty);
}