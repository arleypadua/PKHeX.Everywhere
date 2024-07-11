namespace PKHeX.Web.Services.Plugins;

public class PlugInService(
    PlugInRegistry registry,
    PlugInLocalStorage localStorage)
{
    public async Task InstallFrom(string url)
    {
        var result = await registry.RegisterFrom(url);
        localStorage.Persist(result);
    }

    public void Uninstall(LoadedPlugIn plugIn)
    {
        registry.Deregister(plugIn);
        localStorage.Remove(plugIn);
    }
}