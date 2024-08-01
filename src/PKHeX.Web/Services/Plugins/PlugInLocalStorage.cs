using System.Reflection;
using Blazored.LocalStorage;
using PKHeX.Web.Plugins;

namespace PKHeX.Web.Services.Plugins;

public class PlugInLocalStorage(
    ISyncLocalStorageService localStorage,
    ILogger<PlugInLocalStorage> logger)
{
    public void Remove(LoadedPlugIn plugIn)
    {
        localStorage.RemoveItem(LocalStorageKey(plugIn));
    }

    public void Persist(LoadedPlugIn plugIn)
    {
        var representation = new PlugInStorageRepresentation
        {
            Id = plugIn.Id,
            HasNewerVersion = plugIn.HasNewerVersion,
            SourceUrl = plugIn.SourceUrl,
            AssemblyBytes = plugIn.AssemblyRawBytes,
            Enabled = plugIn.Enabled,
            FeatureToggles = plugIn.Hooks
                .Select(h => new { hook = h, enabled = plugIn.IsHookEnabled(h) })
                .ToDictionary(
                    key => key.hook.GetFullNameOrName(),
                    value => value.enabled),
            PlugInSettings = plugIn.Settings.All.Select(s => s.Value switch
            {
                Settings.SettingValue.StringValue str => new PlugInStorageRepresentation.PlugInSetting
                    { Key = s.Key, ReadOnly = str.ReadOnly, StringValue = str.Value },
                Settings.SettingValue.BooleanValue str => new PlugInStorageRepresentation.PlugInSetting
                    { Key = s.Key, ReadOnly = str.ReadOnly, BooleanValue = str.Value },
                Settings.SettingValue.IntegerValue str => new PlugInStorageRepresentation.PlugInSetting
                    { Key = s.Key, ReadOnly = str.ReadOnly, IntegerValue = str.Value },
                _ => throw new InvalidOperationException($"{s.Value} not supported")
            }).ToList()
        };

        localStorage.SetItem(LocalStorageKey(plugIn), representation);

        logger.LogInformation("Saved plug-in {p} locally", plugIn.Id);
    }

    public IEnumerable<LoadedPlugIn> RestoreAll()
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

        return representations!
            .Select(r =>
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
                    }

                    var plugIn = new LoadedPlugIn(r.SourceUrl, settings, assembly, r.AssemblyBytes);
                    
                    foreach (var (typeName, active) in r.FeatureToggles)
                    {
                        plugIn.SetToggle(typeName, active);
                    }

                    logger.LogInformation("Loaded plugin {k}", r.Id);

                    return plugIn;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
            })
            .Where(p => p is not null)!;
    }

    private string LocalStorageKey(LoadedPlugIn plugIn) => $"{PlugInPrefix}{plugIn.Id}";

    private const string PlugInPrefix = "__plug_in__#";

    public class PlugInStorageRepresentation
    {
        public required string Id { get; set; }
        public bool HasNewerVersion { get; set; }
        public string SourceUrl { get; set; } = default!;
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
        }
    }
}