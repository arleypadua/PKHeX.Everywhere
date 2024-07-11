namespace PKHeX.Web.Services.Plugins;

public class PlugInLocalStorageLoader(
    PlugInLocalStorage plugInStorage,
    PlugInRegistry registry)
{
    public void InitializePlugIns()
    {
        var installed = plugInStorage.RestoreAll();
        foreach (var plugIn in installed)
        {
            registry.Register(plugIn);
        }
    }
}