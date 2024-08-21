using PKHeX.Core;
using PKHeX.Facade;

namespace PKHeX.Web.Plugins.LiveRun;

public static class Extensions
{
    public static IDisable.DisableInfo GetDisabled(this IGameProvider gameProvider, LiveRunPlugin settings)
    {
        var game = gameProvider.Game;
        if (game == null) return IDisable.Disabled("");

        var version = game.GameVersionApproximation;
        return version.Version switch
        {
            GameVersion.E => settings.GetDisabled(LiveRunPlugin.EmeraldRomFile, NotConfigured(version.Name)),
            GameVersion.FR => settings.GetDisabled(LiveRunPlugin.FireRedRomFile, NotConfigured(version.Name)),
            GameVersion.C => NotSupported,
            GameVersion.SS => NotSupported,
            GameVersion.W2 => NotSupported,
            GameVersion.AS => NotSupported,
            GameVersion.UM => NotSupported,
            GameVersion.SH => NotSupported,
            GameVersion.VL => NotSupported,
            GameVersion.GP => NotSupported,
            GameVersion.PLA => NotSupported,
            GameVersion.BD => NotSupported,
            _ => NotSupported,
        };
    }

    public static Settings.SettingValue.FileValue? GetFileSettings(this Game game, Settings settings)
    {
        var version = game.GameVersionApproximation;
        return version.Version switch
        {
            GameVersion.E => settings[LiveRunPlugin.EmeraldRomFile],
            GameVersion.FR => settings[LiveRunPlugin.FireRedRomFile],
            GameVersion.C => null,
            GameVersion.SS => null,
            GameVersion.W2 => null,
            GameVersion.AS => null,
            GameVersion.UM => null,
            GameVersion.SH => null,
            GameVersion.VL => null,
            GameVersion.GP => null,
            GameVersion.PLA => null,
            GameVersion.BD => null,
            _ => null,
        } as Settings.SettingValue.FileValue;
    }

    private static IDisable.DisableInfo GetDisabled(this LiveRunPlugin settings, string key,
        IDisable.DisableInfo fallback) => settings.GetFile(key).Any()
        ? IDisable.Enabled
        : fallback;

    private static readonly IDisable.DisableInfo NotSupported = IDisable.Disabled("Version not supported");

    private static IDisable.DisableInfo NotConfigured(string versionName) =>
        IDisable.Disabled($"Version {versionName} not configured");
}