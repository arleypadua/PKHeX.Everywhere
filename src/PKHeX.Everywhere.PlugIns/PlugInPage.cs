namespace PKHeX.Everywhere.PlugIns;

/// <param name="Module">The name of the embedded resource holding the page's JS module</param>
public record PlugInPage(string Path, string Module, PageLayout Layout = PageLayout.Standard, string? Title = null);

/// <summary>
/// How the host frames a plug-in page: <c>standard</c> with the host's usual page chrome, such as a header with the title, or <c>empty</c> with the page module alone, filling the content area.
/// </summary>
public enum PageLayout
{
    Standard,
    Empty
}
