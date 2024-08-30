using Blazored.LocalStorage;

namespace PKHeX.Web.Services.Plugins;

public class PlugInSourceLocalStorage(
    ISyncLocalStorageService localStorage)
{
    public IEnumerable<PlugInSource> GetSources() => localStorage
        .Keys()
        .Where(k => k.StartsWith(SourcesKeyPrefix))
        .Select(localStorage.GetItem<PlugInSource>)
        .Where(s => s is not null)!;

    public void PersistSource(PlugInSource source) => localStorage
        .SetItem(StorageKey(source), source);
    
    public void RemoveSource(PlugInSource source) => localStorage
        .RemoveItem(StorageKey(source));

    public void MigrateOldDefaultSource()
    {
        var oldKey = $"{SourcesKeyPrefix}/plugins";
        var newKey = $"{SourcesKeyPrefix}https://raw.githubusercontent.com/pkhex-web/plugins-source-assets/main";

        var oldSource = localStorage.GetItem<PlugInSource>(oldKey);
        if (oldSource is null) return;

        oldSource.SourceUrl = "https://raw.githubusercontent.com/pkhex-web/plugins-source-assets/main";
        
        localStorage.RemoveItem(oldKey);
        localStorage.SetItem(newKey, oldSource);
    }

    private string StorageKey(PlugInSource source) => $"{SourcesKeyPrefix}{source.SourceUrl}";
    
    private const string SourcesKeyPrefix = "__plug_in__source__#";
}