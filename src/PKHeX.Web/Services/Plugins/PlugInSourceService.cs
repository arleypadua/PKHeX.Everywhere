using System.Net.Http.Json;

namespace PKHeX.Web.Services.Plugins;

public class PlugInSourceService(
    PlugInRegistry registry,
    PlugInSourceLocalStorage sourceStorage,
    HttpClient httpClient)
{
    public async Task<List<PlugInSource>> GetPlugInSources()
    {
        var stored = sourceStorage
            .GetSources()
            .ToList();

        // if it is a fresh instance with no sources available
        if (stored.Count == 0)
        {
            var defaultSource = await FetchDefault();
            if (defaultSource != null)
            {
                sourceStorage.PersistSource(defaultSource);
                stored.Add(defaultSource);
            }
        }
        
        return stored;
    }

    public async Task<PlugInSource?> FetchLatest(PlugInSource source)
    {
        var latest = await httpClient.GetFromJsonAsync<PlugInSource>(source.SourceManifestUrl);
        return latest;
    }
    
    public async Task<PlugInSource?> FetchFrom(LoadedPlugIn plugIn)
    {
        var source = await httpClient.GetFromJsonAsync<PlugInSource>(plugIn.SourceManifestUrl());
        return source;
    }

    public async Task<PlugInSource?> FetchFrom(string url)
    {
        var source = await httpClient.GetFromJsonAsync<PlugInSource>(url);
        return source;
    }

    public async Task<PlugInSource?> FetchFrom(Uri uri)
    {
        var source = await httpClient.GetFromJsonAsync<PlugInSource>(uri);
        return source;
    }

    public void Remove(PlugInSource source)
    {
        var existingRegistered = source.PlugIns
            .Where(p => registry.GetByOrNull(p.Id) is not null)
            .Select(p => p.Id)
            .ToList();
        
        if (existingRegistered.Count != 0) 
            throw new InvalidOperationException($"There following plug-ins are still in use: {string.Join(", ", existingRegistered)}");

        sourceStorage.RemoveSource(source);
    }

    private Task<PlugInSource?> FetchDefault() => FetchFrom(PlugInSource.DefaultSourceUrl);
}