namespace PKHeX.Web.Services.Plugins;

public class PlugInService(
    PlugInRegistry registry,
    PlugInLocalStorage localStorage,
    PlugInSourceService sourceService,
    AnalyticsService analyticsService)
{
    public async Task<InstalledPlugIn> InstallFrom(string sourceId, string fileUrl)
    {
        var result = await registry.RegisterFrom(sourceId, fileUrl);
        await localStorage.Persist(result);

        analyticsService.TrackInstalled(result);
        return result;
    }

    public async Task<bool> Update(InstalledPlugIn plugIn)
    {
        var source = await sourceService.FetchFrom(plugIn);
        if (source is null) return false;

        var sourcePlugIn = source.PlugIns.FirstOrDefault(p => p.Id == plugIn.Id);
        if (sourcePlugIn is null) return false;

        var downloadUrl = source.GetLatestDownloadUrl(sourcePlugIn);
        var updatedPlugIn = await InstallFrom(source.SourceUrl, downloadUrl);

        analyticsService.TrackUpdated(updatedPlugIn);
        return true;
    }

    public async Task Uninstall(InstalledPlugIn plugIn)
    {
        registry.Deregister(plugIn);
        await localStorage.Remove(plugIn);
    }
}