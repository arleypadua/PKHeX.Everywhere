using System.Reflection;
using Blazored.LocalStorage;
using PKHeX.Web.Plugins;
using TG.Blazor.IndexedDB;

namespace PKHeX.Web.Services.Plugins;

public class PlugInLocalStorage(
    ILocalStorageService localStorageAsync,
    ISyncLocalStorageService localStorage,
    PlugInFilesRepository plugInFilesRepository,
    ILogger<PlugInLocalStorage> logger)
{
    public async Task Remove(LoadedPlugIn plugIn)
    {
        await localStorageAsync.RemoveItemAsync(LocalStorageKey(plugIn));
        await plugInFilesRepository.RemoveAllFrom(plugIn);
    }

    public async Task Persist(LoadedPlugIn plugIn)
    {
        var plugInSettings = plugIn.Settings.All.Select(s => s.Value switch
        {
            Settings.SettingValue.StringValue setting => new PlugInStorageRepresentation.PlugInSetting
                { Key = s.Key, ReadOnly = setting.ReadOnly, StringValue = setting.Value },
            Settings.SettingValue.BooleanValue setting => new PlugInStorageRepresentation.PlugInSetting
                { Key = s.Key, ReadOnly = setting.ReadOnly, BooleanValue = setting.Value },
            Settings.SettingValue.IntegerValue setting => new PlugInStorageRepresentation.PlugInSetting
                { Key = s.Key, ReadOnly = setting.ReadOnly, IntegerValue = setting.Value },
            Settings.SettingValue.FileValue setting => new PlugInStorageRepresentation.PlugInSetting
            {
                Key = s.Key, ReadOnly = setting.ReadOnly, FileName = setting.FileName, FilePlugInId = plugIn.Id
            },
            _ => throw new InvalidOperationException($"{s.Value} not supported")
        }).ToList();

        var representation = new PlugInStorageRepresentation
        {
            Id = plugIn.Id,
            HasNewerVersion = plugIn.HasNewerVersion,
            PlugInSourceId = plugIn.SourceId,
            FileUrl = plugIn.FileUrl,
            AssemblyBytes = plugIn.AssemblyRawBytes,
            Enabled = plugIn.Enabled,
            FeatureToggles = plugIn.Hooks
                .Select(h => new { hook = h, enabled = plugIn.IsHookEnabled(h) })
                .ToDictionary(
                    key => key.hook.GetFullNameOrName(),
                    value => value.enabled),
            PlugInSettings = plugInSettings
        };

        await localStorageAsync.SetItemAsync(LocalStorageKey(plugIn), representation);
        await PersistAllFilesFrom(plugIn);

        logger.LogInformation("Saved plug-in {p} locally", plugIn.Id);
    }

    private async Task PersistAllFilesFrom(LoadedPlugIn plugIn)
    {
        var saveTasks = plugIn.Settings.All
            .Select(p => p.Value)
            .OfType<Settings.SettingValue.FileValue>()
            .Select(async f =>
            {
                await plugInFilesRepository.CreateOrUpdate(plugIn,
                    new PlugInFilesRepository.File(f.Value, f.FileName));
            });
        
        await Task.WhenAll(saveTasks);
    }

    public async Task<IEnumerable<LoadedPlugIn>> RestoreAll()
    {
        var representations = localStorage.Keys()
            .Where(k => k.StartsWith(PlugInPrefix))
            .Select(k =>
            {
                try
                {
                    logger.LogInformation("Parsing plugin {k} from local storage", k);
                    return localStorage.GetItem<PlugInStorageRepresentation>(k);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to parse plugin {k} from local storage", k);
                    return null;
                }
            })
            .Where(r => r is not null);

        var settingTasks = representations
            .Select(async r =>
            {
                try
                {
                    logger.LogInformation("Loading plugin {k}", r!.Id);
                    var assembly = Assembly.Load(r.AssemblyBytes);
                    var settings = assembly.GetSettings();
                    foreach (var setting in r.PlugInSettings)
                    {
                        if (setting.StringValue is not null)
                            settings[setting.Key] =
                                new Settings.SettingValue.StringValue(setting.StringValue, setting.ReadOnly);

                        if (setting.BooleanValue is not null)
                            settings[setting.Key] =
                                new Settings.SettingValue.BooleanValue(setting.BooleanValue.Value, setting.ReadOnly);

                        if (setting.IntegerValue is not null)
                            settings[setting.Key] =
                                new Settings.SettingValue.IntegerValue(setting.IntegerValue.Value, setting.ReadOnly);

                        if (setting.FileName is not null && setting.FilePlugInId is not null)
                        {
                            var file = await plugInFilesRepository.GetFile(setting.FilePlugInId, setting.FileName);

                            settings[setting.Key] =
                                new Settings.SettingValue.FileValue(file?.Data ?? [],
                                    setting.FileName ?? string.Empty, setting.ReadOnly);
                        }
                    }

                    var plugIn = new LoadedPlugIn(r.PlugInSourceId, r.FileUrl, settings, assembly, r.AssemblyBytes)
                    {
                        HasNewerVersion = r.HasNewerVersion
                    };

                    foreach (var (typeName, active) in r.FeatureToggles)
                    {
                        plugIn.SetToggle(typeName, active);
                    }

                    logger.LogInformation("Loaded plugin {k}", r.Id);

                    return plugIn;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "");
                    return null;
                }
            });

        var settings = await Task.WhenAll(settingTasks);
        return settings.Where(p => p is not null)!;
    }

    private string LocalStorageKey(LoadedPlugIn plugIn) => $"{PlugInPrefix}{plugIn.Id}";

    private const string PlugInPrefix = "__plug_in__#";

    public class PlugInStorageRepresentation
    {
        public required string Id { get; set; }
        public bool HasNewerVersion { get; set; }

        private string? _plugInSourceId;

        public string PlugInSourceId
        {
            get => _plugInSourceId ?? PlugInSource.DefaultSourcePath;
            set => _plugInSourceId = value;
        }

        [Obsolete("Replaced by FileUrl")] public string SourceUrl { get; set; } = default!;

        private string? _fileUrl;

        public string FileUrl
        {
            get => _fileUrl ?? SourceUrl;
            set => _fileUrl = value;
        }

        public byte[] AssemblyBytes { get; set; } = [];
        public bool Enabled { get; set; }
        public Dictionary<string, bool> FeatureToggles { get; set; } = [];
        public List<PlugInSetting> PlugInSettings { get; set; } = [];

        public class PlugInSetting
        {
            public string Key { get; init; } = default!;
            public bool ReadOnly { get; init; }
            public string? StringValue { get; init; }
            public bool? BooleanValue { get; init; }
            public int? IntegerValue { get; init; }

            public string? FileName { get; init; }
            public string? FilePlugInId { get; init; }
        }
    }
}