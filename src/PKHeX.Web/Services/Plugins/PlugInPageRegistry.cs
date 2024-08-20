using PKHeX.Web.Plugins;

namespace PKHeX.Web.Services.Plugins;

public class PlugInPageRegistry
{
    private static readonly Dictionary<Identifier, Outcome.PlugInPage> PagesByPlugIn = new();

    public Outcome.PlugInPage? GetPageFor(string plugIn, string path)
        => PagesByPlugIn.GetValueOrDefault(new(plugIn, path));
    
    public void Register(string plugIn, Outcome.PlugInPage page)
        => PagesByPlugIn[new Identifier(plugIn, page.Path)] = page;

    public record Identifier(string PlugIn, string Path);
}