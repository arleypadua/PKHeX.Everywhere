namespace PKHeX.Web.Plugins;

/// <summary>
/// Class representing the settings of a given plugin.
///
/// This class is required to be implemented once per pluginpublic class Settings
/// </summary>
public abstract class Settings(PlugInManifest manifest)
{
    public PlugInManifest Manifest { get; } = manifest ?? throw new ArgumentNullException(nameof(manifest));

    private readonly Dictionary<string, SettingValue> _settings = new();

    public IEnumerable<KeyValuePair<string, SettingValue>> All => _settings;

    public bool Enabled { get; set; } = true;

    public SettingValue this[string key]
    {
        get => _settings[key];
        set
        {
            if (_settings.ContainsKey(key) && _settings[key].GetType() != value.GetType())
            {
                throw new InvalidOperationException(
                    $"Setting {key} exists with type {value.GetType().Name}. Cannot set it to value {value}.");
            }

            _settings[key] = value;
        }
    }

    public abstract record SettingValue
    {
        public record StringValue(String Value) : SettingValue;

        public record BooleanValue(bool Value) : SettingValue;

        public record IntegerValue(int Value) : SettingValue;
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public record PlugInManifest(string PlugInName, string? Description = null)
{
    public string PlugInName { get; init; } = string.IsNullOrWhiteSpace(PlugInName)
        ? throw new ArgumentNullException(nameof(PlugInName))
        : PlugInName;
}