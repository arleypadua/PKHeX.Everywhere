using PKHeX.Core.AutoMod;

namespace PKHeX.Web.Plugins.LiveRun;

public class LiveRunPlugin : Settings
{
    public LiveRunPlugin() : base(PlugInManifest)
    {
        // default settings
        this[EmeraldRomFile] = new SettingValue.FileValue([], string.Empty);
        this[FireRedRomFile] = new SettingValue.FileValue([], string.Empty);
        
        EnabledByDefault<GoToLiveRun>();
    }

    private static readonly PlugInManifest PlugInManifest = new(
        "Live Run (Browser Emulator)", 
        Description: "Allows to use the save file in an emulator in the browser.",
        ProjectUrl: "https://github.com/arleypadua/PKHeX.Everywhere",
        Information: "To run it you should provide your own legal ROM files.");
    
    public const string EmeraldRomFile = "Emerald ROM File";
    public const string FireRedRomFile = "FireRed ROM File";
}