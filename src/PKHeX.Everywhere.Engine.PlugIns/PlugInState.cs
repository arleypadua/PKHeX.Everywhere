using PKHeX.Everywhere.PlugIns;

namespace PKHeX.Everywhere.Engine.PlugIns;

/// <summary>
/// A plug-in's saved state. Persist it and pass it to <c>plugins.register</c> to restore the plug-in as the user left it.
/// </summary>
/// <param name="Toggles">Whether each of the plug-in's hooks is on.</param>
/// <param name="Settings">The plug-in's settings, including file contents.</param>
/// <param name="HasNewerVersion">Whether the last <c>plugins.newestCompatible</c> check for this plug-in found a newer version it can run.</param>
public sealed record PlugInState(bool Enabled, HookToggle[] Toggles, PlugInSetting[] Settings, bool HasNewerVersion)
{
    public static PlugInState From(StoredPlugIn stored) => new(
        stored.Enabled,
        stored.Toggles.Select(t => new HookToggle(t.Key, t.Value)).ToArray(),
        stored.Settings.Select(s => PlugInSetting.From(s.Key, s.Value)).ToArray(),
        stored.HasNewerVersion);

    public StoredPlugIn ToStored() => new(
        Enabled,
        Toggles.ToDictionary(t => t.HookId, t => t.Enabled),
        Settings
            .Select(s => (s.Key, Value: s.ToValue()))
            .Where(s => s.Value is not null)
            .ToDictionary(s => s.Key, s => s.Value!),
        HasNewerVersion);
}

/// <summary>
/// Whether one hook of a plug-in is on.
/// </summary>
/// <param name="HookId">The hook's id, the full name of its .NET type.</param>
public sealed record HookToggle(string HookId, bool Enabled);

/// <summary>
/// One plug-in setting. Exactly one value field is set, and which one gives the setting's type.
/// </summary>
/// <param name="Key">The setting's name, as the plug-in defines it.</param>
/// <param name="ReadOnly">Whether only the plug-in can change it. <c>plugins.updateSetting</c> rejects read-only settings.</param>
/// <param name="FileName">The file's name, set for file settings.</param>
/// <param name="File">The file's contents. <c>plugins.details</c> leaves them out.</param>
public sealed record PlugInSetting(
    string Key,
    bool ReadOnly,
    string? StringValue = null,
    bool? BooleanValue = null,
    int? IntegerValue = null,
    string? FileName = null,
    byte[]? File = null)
{
    public static PlugInSetting From(string key, Settings.SettingValue value) => value switch
    {
        Settings.SettingValue.StringValue s => new(key, s.ReadOnly, StringValue: s.Value),
        Settings.SettingValue.BooleanValue b => new(key, b.ReadOnly, BooleanValue: b.Value),
        Settings.SettingValue.IntegerValue i => new(key, i.ReadOnly, IntegerValue: i.Value),
        Settings.SettingValue.FileValue f => new(key, f.ReadOnly, FileName: f.FileName, File: f.Value),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    public Settings.SettingValue? ToValue() =>
        FileName is not null ? new Settings.SettingValue.FileValue(File ?? [], FileName, ReadOnly)
        : IntegerValue is { } integer ? new Settings.SettingValue.IntegerValue(integer, ReadOnly)
        : BooleanValue is { } boolean ? new Settings.SettingValue.BooleanValue(boolean, ReadOnly)
        : StringValue is not null ? new Settings.SettingValue.StringValue(StringValue, ReadOnly)
        : null;
}

/// <summary>
/// A registered plug-in's manifest, hooks and settings, for showing it to the user.
/// </summary>
/// <param name="Id">The plug-in's id, its assembly name.</param>
/// <param name="Name">The display name from the plug-in's manifest.</param>
/// <param name="Information">Longer text about the plug-in from its manifest.</param>
/// <param name="Version">The plug-in assembly's version.</param>
/// <param name="PublicKeyToken">The assembly's public key token in lowercase hex, or empty when the assembly isn't signed.</param>
/// <param name="HasNewerVersion">Whether the last <c>plugins.newestCompatible</c> check for this plug-in found a newer version it can run.</param>
/// <param name="Hooks">Every hook the plug-in has, on or off.</param>
/// <param name="Settings">The plug-in's settings, without file contents.</param>
public sealed record PlugInDetails(
    string Id,
    string Name,
    string? Description,
    string? ProjectUrl,
    string? Information,
    string Version,
    string PublicKeyToken,
    bool Enabled,
    bool HasNewerVersion,
    PlugInHook[] Hooks,
    PlugInSetting[] Settings)
{
    public static PlugInDetails From(RegisteredPlugIn plugIn, bool hasNewerVersion)
    {
        var manifest = plugIn.Settings.Manifest;
        return new(
            plugIn.Id,
            manifest.PlugInName,
            manifest.Description,
            manifest.ProjectUrl,
            manifest.Information,
            plugIn.Version.ToString(),
            Convert.ToHexStringLower(plugIn.Assembly.GetName().GetPublicKeyToken() ?? []),
            plugIn.Enabled,
            hasNewerVersion,
            plugIn.Hooks.ToArray(),
            plugIn.Settings.All.Select(s => PlugInSetting.From(s.Key, s.Value) with { File = null }).ToArray());
    }
}
