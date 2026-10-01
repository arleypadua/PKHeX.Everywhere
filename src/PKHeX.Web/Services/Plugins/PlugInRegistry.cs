using PKHeX.Everywhere.Engine.PlugIns;

namespace PKHeX.Web.Services.Plugins;

public class PlugInRegistry(HttpClient httpClient, PlugInHost host)
{
    private readonly Dictionary<string, InstalledPlugIn> _loadedPlugins = new();
    private readonly Dictionary<string, IncompatiblePlugIn> _needsReinstall = new();

    public event Action<string, ChangeType>? OnPlugInChanged;

    public enum ChangeType
    {
        Registered,
        Updated,
        Deregistered,
    }

    public async Task<InstalledPlugIn> RegisterFrom(string sourceId, string fileUrl, StoredPlugIn? stored = null)
    {
        var assembly = await httpClient.GetByteArrayAsync(fileUrl);
        if (assembly.Length == 0) throw new InvalidOperationException("No plugin found in this assembly");

        var plugIn = new InstalledPlugIn(sourceId, fileUrl, assembly, host, host.Register(assembly, stored));
        Register(plugIn);
        return plugIn;
    }

    public InstalledPlugIn Register(string sourceId, string fileUrl, CompatibleUpdate update)
    {
        var plugIn = new InstalledPlugIn(sourceId, fileUrl, update.Assembly, host, update.PlugIn);
        Register(plugIn, ChangeType.Updated);
        return plugIn;
    }

    public void Register(InstalledPlugIn plugIn, ChangeType change = ChangeType.Registered)
    {
        _needsReinstall.Remove(plugIn.Id);
        _loadedPlugins[plugIn.Id] = plugIn;
        OnPlugInChanged?.Invoke(plugIn.Id, change);
    }

    public void MarkNeedsReinstall(IncompatiblePlugIn plugIn) => _needsReinstall[plugIn.Id] = plugIn;

    public void Deregister(string id)
    {
        _loadedPlugins.Remove(id);
        _needsReinstall.Remove(id);
        host.Unregister(id);
        OnPlugInChanged?.Invoke(id, ChangeType.Deregistered);
    }

    public InstalledPlugIn GetBy(string id) => _loadedPlugins[id];
    public InstalledPlugIn? GetByOrNull(string id) => _loadedPlugins.GetValueOrDefault(id);
    public bool IsRegistered(string id) => _loadedPlugins.ContainsKey(id);

    public IEnumerable<InstalledPlugIn> GetAllPlugins() => _loadedPlugins.Values;
    public IEnumerable<IncompatiblePlugIn> NeedsReinstall => _needsReinstall.Values;
}
