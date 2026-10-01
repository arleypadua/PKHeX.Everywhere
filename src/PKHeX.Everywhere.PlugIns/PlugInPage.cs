namespace PKHeX.Everywhere.PlugIns;

/// <param name="Module">The name of the embedded resource holding the page's JS module</param>
public record PlugInPage(string Path, string Module, PageLayout Layout = PageLayout.Standard, string? Title = null);

public enum PageLayout
{
    Standard,
    Empty
}
