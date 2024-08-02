using System.Reflection;
using AntDesign;

namespace PKHeX.Web.Services.Plugins;

public class PlugInLocalStorageLoader(
    PlugInLocalStorage plugInStorage,
    PlugInRegistry registry,
    PlugInSourceService sourceService,
    PlugInLocalStorage plugInLocalStorage,
    INotificationService notification,
    ILogger<PlugInLocalStorageLoader> logger)
{
    public void InitializePlugIns()
    {
        var installed = plugInStorage.RestoreAll();
        foreach (var plugIn in installed)
        {
            registry.Register(plugIn);
        }

        _ = CheckNewVersions();
    }

    private async Task CheckNewVersions()
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
            
            var sourceManifestUrls = uncheckedRegisteredPlugIns.Keys.Select(LoadedPlugInExtensions.SourceManifestUrl);

            var sources = (await Task.WhenAll(sourceManifestUrls.Select(sourceService.FetchFrom)))
                .Where(s => s != null)
                .ToDictionary(s => s!.SourceUrl, s => s!);

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

                    var latestVersionString = plugInManifest.PublishedVersions.LastOrDefault();
                    var validVersion = Version.TryParse(latestVersionString, out var latestVersion);
                    if (latestVersionString is null || !validVersion || latestVersion is null)
                    {
                        logger.LogWarning(
                            $"Plug-in {installedPlugIn.Id} at source '{sourceKey}' has no versions published or the version format is invalid {latestVersionString}. " +
                            "Skipping update check.");

                        continue;
                    }

                    var newVersionFound = latestVersion > installedPlugIn.Version;
                    newVersionsFound = newVersionsFound || newVersionFound;

                    if (newVersionFound)
                    {
                        logger.LogInformation(
                            $"Plug-In {installedPlugIn.Id} ({installedPlugIn.Version}) has a new version: {latestVersionString}");
                        installedPlugIn.HasNewerVersion = true;
                        plugInLocalStorage.Persist(installedPlugIn);
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