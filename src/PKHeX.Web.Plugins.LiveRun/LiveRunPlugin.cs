namespace PKHeX.Web.Plugins.LiveRun;

public class LiveRunPlugin : Settings
{
    public LiveRunPlugin() : base(PlugInManifest)
    {
        // default settings
        this[ShowFrameCount] = new SettingValue.BooleanValue(true);
        this[EmeraldRomFile] = new SettingValue.FileValue([], string.Empty);
        this[FireredLeafgreen] = new SettingValue.FileValue([], string.Empty);
        this[RedBlueYellow] = new SettingValue.FileValue([], string.Empty);
        this[GoldSilverCrystal] = new SettingValue.FileValue([], string.Empty);
        this[RubySapphire] = new SettingValue.FileValue([], string.Empty);
        
        EnabledByDefault<GoToLiveRun>();    }

    private static readonly PlugInManifest PlugInManifest = new(
        "Live Run (Browser Emulator)", 
        Description: "Allows to use the save file in an emulator in the browser. This is still in preview and may contain several bugs.",
        ProjectUrl: "https://github.com/arleypadua/PKHeX.Everywhere",
        Information: "To run it you should provide your own legal ROM files");
    
    public const string EmeraldRomFile = "Emerald ROM";
    public const string FireredLeafgreen = "FireRed/LeafGreen ROM";
    public const string RedBlueYellow = "Red/Blue/Yellow ROM";
    public const string GoldSilverCrystal = "Gold/Silver/Crystal ROM";
    public const string RubySapphire = "Ruby/Sapphire ROM";
    public const string ShowFrameCount = "Show frame count";
}
