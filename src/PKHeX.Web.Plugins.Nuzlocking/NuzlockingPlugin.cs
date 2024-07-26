namespace PKHeX.Web.Plugins.Nuzlocking;

public class NuzlockingPlugin : Settings
{
    public NuzlockingPlugin() : base(Manifest)
    {
        this[AmountOfExperienceToEdge] = new SettingValue.IntegerValue(1);
        this[EdgeOnPreviousLevel] = new SettingValue.BooleanValue(true);
        
        EnabledByDefault<EdgeLevelClick>();
    }

    private static readonly PlugInManifest Manifest = new PlugInManifest(
        "Nuzlocking",
        "Helpers for nuzlocking in pokemon games");

    public const string AmountOfExperienceToEdge = "AmountOfExperienceToEdge";
    public const string EdgeOnPreviousLevel = "EdgeOnPreviousLevel";
}