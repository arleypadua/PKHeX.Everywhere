using AntDesign;
using PKHeX.Everywhere.Engine.PlugIns;

namespace PKHeX.Web.Services.Plugins;

public class PlugInLocalStorageLoader(
    PlugInLocalStorage plugInStorage,
    PlugInRegistry registry,
    PlugInSourceService sourceService,
    PlugInSourceLocalStorage plugInSourceLocalStorage,
    PlugInLocalStorage plugInLocalStorage,
    PlugInHost host,
    HttpClient httpClient,
    AnalyticsService analytics,
    INotificationService notification,
    ILogger<PlugInLocalStorageLoader> logger)
{
    public async Task InitializePlugIns()
    {
        var restored = await plugInStorage.RestoreAll();
        foreach (var plugIn in restored.Installed) registry.Register(plugIn);
        foreach (var plugIn in restored.Incompatible) registry.MarkNeedsReinstall(plugIn);

        _ = CheckNewVersions();
    }

    private async Task CheckNewVersions()
    {
        await UpdatePlugInSources();
        await UpdateIncompatiblePlugIns();
        await CheckPlugInVersions();
    }

    private async Task UpdatePlugInSources()
    {
        try
        {
            // for a while, we need to keep it so all local versions are migrated
            plugInSourceLocalStorage.MigrateOldDefaultSource();
            
            var sourceTasks = plugInSourceLocalStorage.GetSources()
                .Select(s => s.SourceManifestUrl)
                .Select(sourceService.FetchFrom);
        
            var updatedSources = await Task.WhenAll(sourceTasks);
            foreach (var source in updatedSources)
            {
                if (source is null) continue;
            
                plugInSourceLocalStorage.PersistSource(source);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to check updates");   
        }
    }

    private async Task UpdateIncompatiblePlugIns()
    {
        foreach (var incompatible in registry.NeedsReinstall.ToList())
        {
            var source = plugInSourceLocalStorage.GetSources().FirstOrDefault(s => s.SourceUrl == incompatible.SourceId);
            var sourcePlugIn = source?.PlugIns.FirstOrDefault(p => p.Id == incompatible.Id);
            if (source is null || sourcePlugIn is null) continue;

            var update = await host.UpdateToCompatible(incompatible.AssemblyRawBytes, incompatible.Stored,
                sourcePlugIn.PublishedVersions,
                version => httpClient.GetByteArrayAsync(source.GetDownloadUrl(sourcePlugIn, version)));
            if (update is null)
            {
                logger.LogWarning("Plug-in {id} couldn't update to a version this app can run and needs reinstall", incompatible.Id);
                continue;
            }

            logger.LogInformation("Updated plug-in {id} to {version} for SDK {sdk}", incompatible.Id, update.Version.Version, update.Version.Sdk);
            var installed = registry.Register(source.SourceUrl, source.GetDownloadUrl(sourcePlugIn, update.Version), update);
            await plugInLocalStorage.Persist(installed);
            analytics.TrackUpdated(installed);
        }
    }

    private async Task CheckPlugInVersions()
    {
        try
        {
            logger.LogInformation("Checking new plug-in versions");
            var newVersionsFound = false;

            var allPlugIns = registry.GetAllPlugins()
                .ToList();
            
            if (allPlugIns.Any(p => p.HasNewerVersion))
            {
                logger.LogInformation("Some plug-ins have already been checked for newer versions.");
                newVersionsFound = true;
            }

            var uncheckedRegisteredPlugIns = allPlugIns
                .Where(p => !p.HasNewerVersion) // not yet checked
                .GroupBy(p => p.SourceId)
                .ToDictionary(p => p.Key, p => p.ToList());
            
            var sources = plugInSourceLocalStorage
                .GetSources()
                .Where(s => uncheckedRegisteredPlugIns.Keys.Contains(s.SourceUrl))
                .ToDictionary(s => s.SourceUrl, s => s);

            foreach (var sourceKey in uncheckedRegisteredPlugIns.Keys)
            {
                var sourceFound = sources.TryGetValue(sourceKey, out var source);
                if (!sourceFound || source is null)
                {
                    logger.LogWarning($"Could not find plug-in '{sourceKey}'. Skipping update check.");
                    continue;
                }

                var installedPlugIns = uncheckedRegisteredPlugIns[sourceKey];
                foreach (var installedPlugIn in installedPlugIns)
                {
                    var plugInManifest = source.PlugIns.FirstOrDefault(p => p.Id == installedPlugIn.Id);
                    if (plugInManifest is null)
                    {
                        logger.LogWarning(
                            $"Could not find plug-in '{installedPlugIn.Id}' at source '{sourceKey}'. Skipping update check.");
                        continue;
                    }

                    var latestVersionString = plugInManifest.NewestCompatibleVersion?.Version;
                    if (latestVersionString is null)
                    {
                        logger.LogWarning(
                            $"Plug-in {installedPlugIn.Id} at source '{sourceKey}' has no valid version this app can run. " +
                            "Skipping update check.");

                        continue;
                    }

                    var latestVersion = Version.Parse(latestVersionString);

                    var newVersionFound = latestVersion > installedPlugIn.Version;
                    newVersionsFound = newVersionsFound || newVersionFound;

                    if (newVersionFound)
                    {
                        logger.LogInformation(
                            $"Plug-In {installedPlugIn.Id} ({installedPlugIn.Version}) has a new version: {latestVersionString}");
                        installedPlugIn.HasNewerVersion = true;
                        await plugInLocalStorage.Persist(installedPlugIn);
                    }
                }
            }

            if (newVersionsFound)
            {
                _ = notification.Open(new NotificationConfig
                {
                    Message = $"New plug-in version available",
                    Description = $"Visit the plug-in page and update them.",
                    NotificationType = NotificationType.Info
                });
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to check updates");
        }
    }
}