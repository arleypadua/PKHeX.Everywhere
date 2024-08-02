using PKHeX.Core.AutoMod;

namespace PKHeX.Web.Plugins.AutoLegality;

public class AutoLegalityModePlugin : Settings
{
    public AutoLegalityModePlugin() : base(PlugInManifest)
    {
        // default settings
        this[Timeout] = new SettingValue.IntegerValue(2);
        this[ForceLevel100] = new SettingValue.BooleanValue(false);
        
        EnabledByDefault<MakeLegalOnClick>();
    }

    internal void ConfigureAutoLegalityMode()
    {
        APILegality.ForceLevel100for50 = this[ForceLevel100].GetBoolean();
        APILegality.Timeout = this[Timeout].GetInteger();
    }

    private static readonly PlugInManifest PlugInManifest = new(
        "Auto-legality mode", 
        Description: "Best effort to make your pokemon legal when editing them.",
        ProjectUrl: "https://github.com/santacrab2/PKHeX-Plugins",
        Information: "A proxy for the auto legality mode hosted at: https://github.com/santacrab2/PKHeX-Plugins");
    
    private const string Timeout = "Timeout (seconds)";
    private const string ForceLevel100 = "Force lvl 100 from 50";
}