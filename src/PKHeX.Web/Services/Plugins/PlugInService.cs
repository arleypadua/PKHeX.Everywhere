namespace PKHeX.Web.Services.Plugins;

public class PlugInService(
    PlugInRegistry registry,
    PlugInLocalStorage localStorage,
    PlugInSourceService sourceService)
{
    public async Task InstallFrom(string sourceId, string fileUrl)
    {
        var result = await registry.RegisterFrom(sourceId, fileUrl);
        localStorage.Persist(result);
    }

    public async Task Update(LoadedPlugIn plugIn)
    {
        var source = await sourceService.FetchFrom(plugIn);
        if (source is null) return;

        var sourcePlugIn = source.PlugIns.FirstOrDefault(p => p.Id == plugIn.Id);
        if (sourcePlugIn is null) return;

        var downloadUrl = source.GetLatestDownloadUrl(sourcePlugIn);
        await InstallFrom(source.SourceUrl, downloadUrl);
    }

    public void Uninstall(LoadedPlugIn plugIn)
    {
        registry.Deregister(plugIn);
        localStorage.Remove(plugIn);
    }
}