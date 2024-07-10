namespace PKHeX.Web.Plugins.Demo;

public class MyDemoPlugin : Settings
{
    public MyDemoPlugin() : base(PlugInManifest)
    {
        // default settings
        this["MyString"] = new SettingValue.StringValue("Hello world!");
        this["Boolean"] = new SettingValue.BooleanValue(true);
        this["Integer"] = new SettingValue.IntegerValue(1);
    }

    private static readonly PlugInManifest PlugInManifest = new("My Demo Plugin", "This is just for demo purposes.");
}