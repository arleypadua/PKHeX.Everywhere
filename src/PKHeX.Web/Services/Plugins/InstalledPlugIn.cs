using System.Reflection;
using PKHeX.Everywhere.PlugIns;

namespace PKHeX.Web.Services.Plugins;

public abstract class InstalledPlugIn(string sourceId, string fileUrl, byte[] assemblyRawBytes)
{
    /// <summary>
    /// The plug-in source id, which is also its url
    /// </summary>
    public string SourceId => sourceId;

    public string FileUrl { get; } = string.IsNullOrWhiteSpace(fileUrl)
        ? throw new ArgumentNullException(nameof(fileUrl))
        : fileUrl;

    public byte[] AssemblyRawBytes { get; } = assemblyRawBytes;
    public bool HasNewerVersion { get; set; }

    public abstract Assembly Assembly { get; }
    public string Id => Assembly.GetName().Name!;
    public Version Version => Assembly.GetName().Version!;

    public string PublicKeyToken => BitConverter.ToString(Assembly.GetName().GetPublicKeyToken() ?? [])
        .Replace("-", string.Empty).ToLowerInvariant();

    public abstract bool Enabled { get; set; }
    public abstract PlugInManifest Manifest { get; }

    public abstract IEnumerable<KeyValuePair<string, Settings.SettingValue>> SettingValues { get; }
    public abstract Settings.SettingValue? GetSetting(string key);
    public abstract void SetSetting(string key, Settings.SettingValue value);

    public abstract IEnumerable<string> HookIds { get; }
    public abstract bool IsHookEnabled(string hookId);
    public abstract void SetToggle(string hookId, bool enabled);
}

public static class InstalledPlugInExtensions
{
    public static string SourceManifestUrl(this InstalledPlugIn plugIn) =>
        SourceManifestUrl(plugIn.SourceId);

    public static string SourceManifestUrl(string sourceId) =>
        $"{sourceId.TrimEnd('/')}/{PlugInSource.ManifestFileName}";
}
