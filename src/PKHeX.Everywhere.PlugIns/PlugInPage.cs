namespace PKHeX.Everywhere.PlugIns;

/// <summary>
/// A page the plug-in ships as a JS module
/// </summary>
/// <param name="Path">The URL path the page responds to</param>
/// <param name="Module">The name of the embedded resource holding the page's JS module</param>
/// <param name="Layout">The layout the page renders in</param>
/// <param name="Title">The title shown above a page with the standard layout</param>
public record PlugInPage(string Path, string Module, PageLayout Layout = PageLayout.Standard, string? Title = null);

public enum PageLayout
{
    Standard,
    Empty
}
