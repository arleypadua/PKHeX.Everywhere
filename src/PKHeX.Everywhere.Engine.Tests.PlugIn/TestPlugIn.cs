using PKHeX.Everywhere.PlugIns;

namespace PKHeX.Everywhere.Engine.Tests.PlugIn;

public class TestSettings : Settings
{
    public const string Greeting = "Greeting";
    public const string Locked = "Locked";

    public TestSettings() : base(new PlugInManifest("Test Plug-In", "A plug-in for host tests"))
    {
        this[Greeting] = new SettingValue.StringValue("Hello");
        this[Locked] = new SettingValue.StringValue("fixed", ReadOnly: true);

        EnabledByDefault<Greet>();
        EnabledByDefault<Fail>();

        DeclarePage(new PlugInPage("hello", "hello.js", PageLayout.Standard, "Hello"));
    }
}

public class Greet(IGameProvider game, TestSettings settings) : IQuickAction
{
    public string Description => "Greets the trainer";
    public string Label => "Greet";
    public IDisable.DisableInfo DisabledInfo => IDisable.Enabled;

    public Task<Outcome> OnActionRequested() =>
        Outcome.Notify($"{settings.GetString(TestSettings.Greeting)}, {game.LoadedGame.Trainer.Name}").Completed();
}

public class Fail : IQuickAction
{
    public string Description => "Always fails";
    public string Label => "Fail";
    public IDisable.DisableInfo DisabledInfo => IDisable.Enabled;

    public Task<Outcome> OnActionRequested() => throw new InvalidOperationException("Failed on purpose");
}

public class OpenHello : IQuickAction
{
    public string Description => "Opens the hello page";
    public string Label => "Open hello";
    public IDisable.DisableInfo DisabledInfo => IDisable.Enabled;

    public Task<Outcome> OnActionRequested() => Outcome.OpenPage("hello").Completed();
}
