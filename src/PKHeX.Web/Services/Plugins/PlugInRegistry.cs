using System.Reflection;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Web.Plugins;

namespace PKHeX.Web.Services.Plugins;

public class PlugInRegistry
{
    private readonly IServiceProvider _appServiceProvider;
    private readonly HttpClient _httpClientFactory;
    private readonly PlugInHost _host;
    private readonly Dictionary<string, InstalledPlugIn> _loadedPlugins = new();

    private ServiceProvider _pluginServiceProvider = null!;

    public event Action<InstalledPlugIn, ChangeType>? OnPlugInChanged;

    public enum ChangeType
    {
        Registered,
        Deregistered,
    }


    public PlugInRegistry(
        HttpClient httpClientFactory,
        PlugInHost host,
        IServiceProvider appServiceProvider)
    {
        _appServiceProvider = appServiceProvider;
        _httpClientFactory = httpClientFactory;
        _host = host;

        RefreshServiceProvider();
    }

    public async Task<InstalledPlugIn> RegisterFrom(string sourceId, string fileUrl, StoredPlugIn? stored = null)
    {
        var assembly = await _httpClientFactory.GetByteArrayAsync(fileUrl);
        return LoadPlugInFrom(sourceId, fileUrl, assembly, stored);
    }

    public void Register(InstalledPlugIn plugIn)
    {
        if (plugIn is not HostPlugIn && _loadedPlugins.GetValueOrDefault(plugIn.Id) is HostPlugIn)
            _host.Unregister(plugIn.Id);

        _loadedPlugins[plugIn.Id] = plugIn;
        RefreshServiceProvider(_pluginServiceProvider);
        OnPlugInChanged?.Invoke(plugIn, ChangeType.Registered);
    }

    public void Deregister(InstalledPlugIn plugIn)
    {
        _loadedPlugins.Remove(plugIn.Id);
        if (plugIn is HostPlugIn) _host.Unregister(plugIn.Id);
        RefreshServiceProvider(_pluginServiceProvider);
        OnPlugInChanged?.Invoke(plugIn, ChangeType.Deregistered);
    }

    public InstalledPlugIn GetBy(string id) => _loadedPlugins[id];
    public InstalledPlugIn? GetByOrNull(string id) => _loadedPlugins.GetValueOrDefault(id);
    public bool IsRegistered(string id) => _loadedPlugins.ContainsKey(id);

    public IEnumerable<InstalledPlugIn> GetAllPlugins() => _loadedPlugins.Values;

    public LoadedPlugIn GetPlugInOwningHook<T>(T hook) where T : IPluginHook =>
        V1PlugIns.Single(p => p.Hooks.Any(t => t == hook.GetType()));

    public IEnumerable<PlugInHook> GetAllHooksOf(InstalledPlugIn plugIn) => plugIn switch
    {
        HostPlugIn hosted => hosted.Registered.Hooks,
        LoadedPlugIn loaded => loaded.Hooks
            .Select(t => _pluginServiceProvider.GetFromImplementation(t) as IPluginHook)
            .OfType<IPluginHook>()
            .Select(h => new PlugInHook(h.GetType().GetFullNameOrName(), h.Description, loaded.IsHookEnabled(h)))
            .ToList(),
        _ => [],
    };

    public IEnumerable<T> GetAllHooks<T>() where T : IPluginHook => _pluginServiceProvider.GetServices<T>();

    public IEnumerable<T> GetAllEnabledHooks<T>() where T : IPluginHook => _pluginServiceProvider.GetServices<T>()
        .Where(h => GetPlugInOwningHook(h).IsPlugInAndHookEnabled(h));

    private InstalledPlugIn LoadPlugInFrom(string sourceId, string fileUrl, byte[] assemblyBytes, StoredPlugIn? stored)
    {
        if (assemblyBytes.Length == 0) throw new InvalidOperationException("No plugin found in this assembly");

        InstalledPlugIn loadedPlugIn = PlugInHost.DetectSdk(assemblyBytes) == PlugInSdk.V2
            ? new HostPlugIn(sourceId, fileUrl, assemblyBytes, _host, _host.Register(assemblyBytes, stored))
            : LoadedPlugIn.From(sourceId, fileUrl, Assembly.Load(assemblyBytes), assemblyBytes);

        Register(loadedPlugIn);

        return loadedPlugIn;
    }

    private void RefreshServiceProvider(ServiceProvider? existing = null)
    {
        existing?.Dispose();
        _pluginServiceProvider = BuildServiceCollection().BuildServiceProvider();
    }

    private IServiceCollection BuildServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IGameProvider>(
            new GameServiceProxy(_appServiceProvider.GetRequiredService<GameService>()));

        return services
            .RegisterPluginAt(V1PlugIns);
    }

    private IEnumerable<LoadedPlugIn> V1PlugIns => _loadedPlugins.Values.OfType<LoadedPlugIn>();
}