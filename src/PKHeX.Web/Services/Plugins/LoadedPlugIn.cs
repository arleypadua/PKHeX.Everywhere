using System.Reflection;
using PKHeX.Web.Plugins;

namespace PKHeX.Web.Services.Plugins;

public class LoadedPlugIn(
    string sourceId,
    string fileUrl,
    Settings settings,
    Assembly assembly,
    byte[] assemblyRawBytes)
{
    private readonly Dictionary<string, bool> _hookToggles = new();

    /// <summary>
    /// The plug-in source id, which is also its url
    /// </summary>
    public string SourceId => sourceId;

    public string FileUrl { get; private set; } = string.IsNullOrWhiteSpace(fileUrl)
        ? throw new ArgumentNullException(nameof(fileUrl))
        : fileUrl;

    public bool HasNewerVersion { get; set; }
    public bool Enabled { get; set; } = true;
    public Settings Settings { get; } = settings;
    public Assembly Assembly { get; } = assembly;
    public byte[] AssemblyRawBytes { get; set; } = assemblyRawBytes;
    public List<Type> Hooks { get; } = assembly.GetConcreteTypesOf<IPluginHook>().ToList();

    public string Id => Assembly.GetName().Name!;
    public Version Version => Assembly.GetName().Version!;

    public string PublicKeyToken => BitConverter.ToString(Assembly.GetName().GetPublicKeyToken() ?? [])
        .Replace("-", string.Empty).ToLowerInvariant();

    public void SetToggle(IPluginHook hook, bool toggle)
    {
        var type = hook.GetType();
        SetToggle(type.GetFullNameOrName(), toggle);
    }

    internal void SetToggle(string typeName, bool toggle)
    {
        _hookToggles.TryAdd(typeName, false);
        _hookToggles[typeName] = toggle;
    }

    public bool IsPlugInAndHookEnabled(IPluginHook hook) =>
        Enabled && IsHookEnabled(hook.GetType());

    public bool IsHookEnabled(Type hookType) =>
        _hookToggles.ContainsKey(hookType.GetFullNameOrName()) && _hookToggles[hookType.GetFullNameOrName()];

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
}

public static class LoadedPlugInExtensions
{
    public static string SourceManifestUrl(this LoadedPlugIn plugIn) =>
        SourceManifestUrl(plugIn.SourceId);
    
    public static string SourceManifestUrl(string sourceId) =>
        $"{sourceId}/{PlugInSource.ManifestFileName}"
            .Replace("//", "/");
}