using System.Reflection;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.PlugIns;

namespace PKHeX.Web.Services.Plugins;

public sealed class HostPlugIn(
    string sourceId,
    string fileUrl,
    byte[] assemblyRawBytes,
    PlugInHost host,
    RegisteredPlugIn registered) : InstalledPlugIn(sourceId, fileUrl, assemblyRawBytes)
{
    public RegisteredPlugIn Registered => registered;

    public override Assembly Assembly => registered.Assembly;

    public override bool Enabled
    {
        get => registered.Enabled;
        set => host.SetEnabled(Id, value);
    }

    public override PlugInManifest Manifest => registered.Settings.Manifest;

    public override IEnumerable<KeyValuePair<string, Settings.SettingValue>> SettingValues => registered.Settings.All;
    public override Settings.SettingValue? GetSetting(string key) => registered.Settings.GetOrDefault(key);
    public override void SetSetting(string key, Settings.SettingValue value) => host.UpdateSetting(Id, key, value);

    public override IEnumerable<string> HookIds => registered.HookIds;
    public override bool IsHookEnabled(string hookId) => registered.IsHookEnabled(hookId);
    public override void SetToggle(string hookId, bool enabled) => host.SetToggle(Id, hookId, enabled);
}
