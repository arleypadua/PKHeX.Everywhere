using PKHeX.Everywhere.Engine.PlugIns;

namespace PKHeX.Web.Services.Plugins;

public class PlugInService(
    PlugInRegistry registry,
    PlugInLocalStorage localStorage,
    PlugInSourceService sourceService,
    AnalyticsService analyticsService)
{
    public async Task<InstalledPlugIn> InstallFrom(string sourceId, string fileUrl, StoredPlugIn? stored = null)
    {
        var result = await registry.RegisterFrom(sourceId, fileUrl, stored);
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

        var version = sourcePlugIn.NewestCompatibleVersion;
        if (version is null) return false;

        var downloadUrl = source.GetDownloadUrl(sourcePlugIn, version);
        var updatedPlugIn = await InstallFrom(source.SourceUrl, downloadUrl);

        analyticsService.TrackUpdated(updatedPlugIn);
        return true;
    }

    public async Task Uninstall(string plugInId)
    {
        registry.Deregister(plugInId);
        await localStorage.Remove(plugInId);
    }
}
