using System.Reflection;
using PKHeX.Web.Plugins;

namespace PKHeX.Web.Services.Plugins;

public class LoadedPlugIn(string sourceUrl, Settings settings, Assembly assembly, byte[] assemblyRawBytes)
{
    private readonly Dictionary<string, bool> _hookToggles = new();

    public string SourceUrl { get; private set; } = string.IsNullOrWhiteSpace(sourceUrl)
        ? throw new ArgumentNullException(nameof(sourceUrl))
        : sourceUrl;

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

    public void Toggle(IPluginHook hook)
    {
        var type = hook.GetType();
        _hookToggles.TryAdd(type.GetFullNameOrName(), false);
        _hookToggles[type.GetFullNameOrName()] = !_hookToggles[type.GetFullNameOrName()];
    }
    
    public void SetToggle(string typeName, bool toggle)
    {
        _hookToggles.TryAdd(typeName, false);
        _hookToggles[typeName] = !_hookToggles[typeName];
    }

    public bool IsPlugInAndHookEnabled(IPluginHook hook) =>
        Enabled && IsHookEnabled(hook.GetType());

    // enabled by default
    public bool IsHookEnabled(Type hookType) =>
        !_hookToggles.ContainsKey(hookType.GetFullNameOrName()) || _hookToggles[hookType.GetFullNameOrName()];

    public bool IsHookEnabled(IPluginHook hook) => IsHookEnabled(hook.GetType());

    public static LoadedPlugIn From(string sourceUrl, Assembly assembly, byte[] assemblyRawBytes) =>
        new(sourceUrl, assembly.GetSettings(), assembly, assemblyRawBytes);
}