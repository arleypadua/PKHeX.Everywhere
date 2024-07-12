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
    
    private string StorageKey(PlugInSource source) => $"{SourcesKeyPrefix}{source.SourceUrl}";
    
    private const string SourcesKeyPrefix = "__plug_in__source__#";
}