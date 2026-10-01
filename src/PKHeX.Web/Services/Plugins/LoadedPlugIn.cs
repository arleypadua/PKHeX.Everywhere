using System.Reflection;
using PKHeX.Web.Plugins;
using V2 = PKHeX.Everywhere.PlugIns;

namespace PKHeX.Web.Services.Plugins;

public class LoadedPlugIn(
    string sourceId,
    string fileUrl,
    Settings settings,
    Assembly assembly,
    byte[] assemblyRawBytes) : InstalledPlugIn(sourceId, fileUrl, assemblyRawBytes)
{
    private readonly Dictionary<string, bool> _hookToggles = new();

    public override bool Enabled { get; set; } = true;
    public Settings Settings { get; } = settings;
    public override Assembly Assembly { get; } = assembly;
    public List<Type> Hooks { get; } = assembly.GetConcreteTypesOf<IPluginHook>().ToList();

    public override V2.PlugInManifest Manifest => new(
        Settings.Manifest.PlugInName,
        Settings.Manifest.Description,
        Settings.Manifest.ProjectUrl,
        Settings.Manifest.Information);

    public override IEnumerable<KeyValuePair<string, V2.Settings.SettingValue>> SettingValues =>
        Settings.All.Select(s => KeyValuePair.Create(s.Key, ToV2(s.Value)));

    public override V2.Settings.SettingValue? GetSetting(string key) =>
        Settings.GetOrDefault(key) is { } value ? ToV2(value) : null;

    public override void SetSetting(string key, V2.Settings.SettingValue value) => Settings[key] = ToV1(value);

    public override IEnumerable<string> HookIds => Hooks.Select(h => h.GetFullNameOrName());

    public void SetToggle(IPluginHook hook, bool toggle)
    {
        var type = hook.GetType();
        SetToggle(type.GetFullNameOrName(), toggle);
    }

    public override void SetToggle(string typeName, bool toggle)
    {
        _hookToggles.TryAdd(typeName, false);
        _hookToggles[typeName] = toggle;
    }

    public bool IsPlugInAndHookEnabled(IPluginHook hook) =>
        Enabled && IsHookEnabled(hook.GetType());

    public bool IsHookEnabled(Type hookType) => IsHookEnabled(hookType.GetFullNameOrName());

    public override bool IsHookEnabled(string hookId) => _hookToggles.GetValueOrDefault(hookId);

    public bool IsHookEnabled(IPluginHook hook) => IsHookEnabled(hook.GetType());

    public static LoadedPlugIn From(
        string sourceId,
        string fileUrl,
        Assembly assembly,
        byte[] assemblyRawBytes)
    {
        var settings = assembly.GetSettings();
        var plugIn = new LoadedPlugIn(sourceId, fileUrl, settings, assembly, assemblyRawBytes);
        foreach (var (type, toggle) in settings.DefaultFeatureToggles)
            plugIn.SetToggle(type.GetFullNameOrName(), toggle);

        return plugIn;
    }

    public static V2.Settings.SettingValue ToV2(Settings.SettingValue value) => value switch
    {
        Settings.SettingValue.StringValue s => new V2.Settings.SettingValue.StringValue(s.Value, s.ReadOnly),
        Settings.SettingValue.BooleanValue b => new V2.Settings.SettingValue.BooleanValue(b.Value, b.ReadOnly),
        Settings.SettingValue.IntegerValue i => new V2.Settings.SettingValue.IntegerValue(i.Value, i.ReadOnly),
        Settings.SettingValue.FileValue f => new V2.Settings.SettingValue.FileValue(f.Value, f.FileName, f.ReadOnly),
        _ => throw new InvalidOperationException($"{value} not supported")
    };

    public static Settings.SettingValue ToV1(V2.Settings.SettingValue value) => value switch
    {
        V2.Settings.SettingValue.StringValue s => new Settings.SettingValue.StringValue(s.Value, s.ReadOnly),
        V2.Settings.SettingValue.BooleanValue b => new Settings.SettingValue.BooleanValue(b.Value, b.ReadOnly),
        V2.Settings.SettingValue.IntegerValue i => new Settings.SettingValue.IntegerValue(i.Value, i.ReadOnly),
        V2.Settings.SettingValue.FileValue f => new Settings.SettingValue.FileValue(f.Value, f.FileName, f.ReadOnly),
        _ => throw new InvalidOperationException($"{value} not supported")
    };
}
