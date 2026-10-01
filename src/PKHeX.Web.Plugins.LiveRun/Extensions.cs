using PKHeX.Core;
using PKHeX.Facade;

namespace PKHeX.Web.Plugins.LiveRun;

public static class Extensions
{
    public static IDisable.DisableInfo GetDisabled(this IGameProvider gameProvider, LiveRunPlugin settings)
    {
        var game = gameProvider.Game;
        if (game == null) return IDisable.Disabled("No game loaded");

        var version = game.SaveVersion;
        return version.Version switch
        {
            GameVersion.RBY => settings.GetDisabled(LiveRunPlugin.RedBlueYellow, NotConfigured(version.Name)),
            GameVersion.GSC or GameVersion.GS or GameVersion.C => settings.GetDisabled(LiveRunPlugin.GoldSilverCrystal, NotConfigured(version.Name)),
            GameVersion.FRLG or GameVersion.FR or GameVersion.LG => settings.GetDisabled(LiveRunPlugin.FireredLeafgreen, NotConfigured(version.Name)),
            GameVersion.E => settings.GetDisabled(LiveRunPlugin.EmeraldRomFile, NotConfigured(version.Name)),
            GameVersion.RS or GameVersion.R or GameVersion.S => settings.GetDisabled(LiveRunPlugin.RubySapphire, NotConfigured(version.Name)),
            _ => NotSupported,
        };
    }

    private static IDisable.DisableInfo GetDisabled(this LiveRunPlugin settings, string key,
        IDisable.DisableInfo fallback) => settings.GetFile(key).Any()
        ? IDisable.Enabled
        : fallback;

    private static readonly IDisable.DisableInfo NotSupported = IDisable.Disabled("Version not supported");

    private static IDisable.DisableInfo NotConfigured(string versionName) =>
        IDisable.Disabled($"Version {versionName} not configured");
}