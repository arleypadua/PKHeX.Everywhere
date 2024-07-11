namespace PKHeX.Web.Plugins;

/// <summary>
/// Class representing the settings of a given plugin.
///
/// This class is required to be implemented once per pluginpublic class Settings
/// </summary>
public abstract class Settings(PlugInManifest manifest)
{
    private readonly IDictionary<Type, bool> _defaultFeatureToggles = new Dictionary<Type, bool>();
    public PlugInManifest Manifest { get; } = manifest ?? throw new ArgumentNullException(nameof(manifest));
    
    public IReadOnlyDictionary<Type, bool> DefaultFeatureToggles => _defaultFeatureToggles.AsReadOnly();

    private readonly Dictionary<string, SettingValue> _settings = new();

    public IEnumerable<KeyValuePair<string, SettingValue>> All => _settings;

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

            if (_settings.ContainsKey(key) && _settings[key].ReadOnly)
                throw new InvalidOperationException($"Setting {key} is read-only. Cannot set it.");

            _settings[key] = value;
        }
    }

    protected void EnabledByDefault<THook>() where THook : IPluginHook
    {
        _defaultFeatureToggles[typeof(THook)] = true;
    }

    public abstract record SettingValue(bool ReadOnly)
    {
        public record StringValue(String Value, bool ReadOnly = false) : SettingValue(ReadOnly);

        public record BooleanValue(bool Value, bool ReadOnly = false) : SettingValue(ReadOnly);

        public record IntegerValue(int Value, bool ReadOnly = false) : SettingValue(ReadOnly);
        
        public string GetString()
        {
            if (this is not StringValue str) throw new InvalidOperationException($"{this} is not a string value.");
            return str.Value;
        }
        
        public bool GetBoolean()
        {
            if (this is not BooleanValue boolean) throw new InvalidOperationException($"{this} is not a boolean value.");
            return boolean.Value;
        }
        
        public int GetInteger()
        {
            if (this is not IntegerValue integer) throw new InvalidOperationException($"{this} is not a integer value.");
            return integer.Value;
        }
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public record PlugInManifest(
    string PlugInName, 
    string? Description = null, 
    string? ProjectUrl = null,
    string? Information = null)
{
    public string PlugInName { get; } = string.IsNullOrWhiteSpace(PlugInName)
        ? throw new ArgumentNullException(nameof(PlugInName))
        : PlugInName;
}