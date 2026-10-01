using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.PlugIns;

namespace PKHeX.Web.Services.Plugins;

public sealed class InstalledPlugIn(
    string sourceId,
    string fileUrl,
    byte[] assemblyRawBytes,
    PlugInHost host,
    RegisteredPlugIn registered)
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

    public RegisteredPlugIn Registered => registered;
    public string Id => registered.Id;
    public Version Version => registered.Version;

    public string PublicKeyToken => BitConverter.ToString(registered.Assembly.GetName().GetPublicKeyToken() ?? [])
        .Replace("-", string.Empty).ToLowerInvariant();

    public bool Enabled
    {
        get => registered.Enabled;
        set => host.SetEnabled(Id, value);
    }

    public PlugInManifest Manifest => registered.Settings.Manifest;

    public IEnumerable<KeyValuePair<string, Settings.SettingValue>> SettingValues => registered.Settings.All;
    public Settings.SettingValue? GetSetting(string key) => registered.Settings.GetOrDefault(key);
    public void SetSetting(string key, Settings.SettingValue value) => host.UpdateSetting(Id, key, value);

    public IEnumerable<string> HookIds => registered.HookIds;
    public bool IsHookEnabled(string hookId) => registered.IsHookEnabled(hookId);
    public void SetToggle(string hookId, bool enabled) => host.SetToggle(Id, hookId, enabled);
}

public sealed record IncompatiblePlugIn(string Id, string SourceId, byte[] AssemblyRawBytes, StoredPlugIn Stored);

public static class InstalledPlugInExtensions
{
    public static string SourceManifestUrl(this InstalledPlugIn plugIn) =>
        SourceManifestUrl(plugIn.SourceId);

    public static string SourceManifestUrl(string sourceId) =>
        $"{sourceId.TrimEnd('/')}/{PlugInSource.ManifestFileName}";
}
