using PKHeX.Everywhere.PlugIns;

namespace PKHeX.Everywhere.Engine.PlugIns;

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

public sealed record HookToggle(string HookId, bool Enabled);

/// <summary>
/// One setting, typed by whichever value is set, as plug-in storage has always written it.
/// </summary>
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
