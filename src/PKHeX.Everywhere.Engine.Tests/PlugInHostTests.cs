using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.PlugIns;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;

namespace PKHeX.Everywhere.Engine.Tests;

public class PlugInHostTests
{
    private const string TestPlugInId = "PKHeX.Everywhere.Engine.Tests.PlugIn";
    private const string Greet = "PKHeX.Everywhere.Engine.Tests.PlugIn.Greet";
    private const string Fail = "PKHeX.Everywhere.Engine.Tests.PlugIn.Fail";
    private const string OpenHello = "PKHeX.Everywhere.Engine.Tests.PlugIn.OpenHello";

    private static byte[] TestPlugIn => PlugInBytes("PKHeX.Everywhere.Engine.Tests.PlugIn");
    private static byte[] V1PlugIn => PlugInBytes("V1PlugIn");

    private static byte[] PlugInBytes(string name) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{name}.dll"));

    [Fact]
    public void RegistersAPlugInFromBytesAndListsIt()
    {
        var host = new PlugInHost(new Session());

        host.Register(TestPlugIn);

        var plugIn = host.List().Should().ContainSingle().Subject;
        plugIn.Id.Should().Be(TestPlugInId);
        plugIn.Version.Should().Be(new Version(1, 2, 3, 0));
        plugIn.Enabled.Should().BeTrue();
        plugIn.Settings.Manifest.PlugInName.Should().Be("Test Plug-In");
        plugIn.Settings.GetString("Greeting").Should().Be("Hello");
        plugIn.Settings.Pages.Should().Equal(new PlugInPage("hello", "hello.js", PageLayout.Standard, "Hello"));
        plugIn.Hooks.Should().BeEquivalentTo([
            new PlugInHook(Greet, "Greets the trainer", true),
            new PlugInHook(Fail, "Always fails", true),
            new PlugInHook(OpenHello, "Opens the hello page", false),
            new PlugInHook($"{TestPlugInId}.Awaits", "Waits for the test to release it", false),
            new PlugInHook($"{TestPlugInId}.Unavailable", "Is never available", false),
            new PlugInHook($"{TestPlugInId}.EchoItem", "Echoes the changed item", true),
            new PlugInHook($"{TestPlugInId}.FailOnItem", "Fails on every item change", true),
            new PlugInHook($"{TestPlugInId}.WriteOnItem", "Writes the save on every item change", false),
            new PlugInHook($"{TestPlugInId}.RenameOnChange", "Renames a changed Pokémon", true),
            new PlugInHook($"{TestPlugInId}.RenameOnSave", "Renames a saved Pokémon", true),
            new PlugInHook($"{TestPlugInId}.LevelUp", "Raises the Pokémon's level by one", false),
        ]);
    }

    [Fact]
    public void RegistersAPlugInWithItsStoredSettingsAndToggles()
    {
        var host = new PlugInHost(new Session());

        var plugIn = host.Register(TestPlugIn, new StoredPlugIn(
            Enabled: false,
            Toggles: new Dictionary<string, bool> { [Greet] = false, [OpenHello] = true },
            Settings: new Dictionary<string, Settings.SettingValue>
            {
                ["Greeting"] = new Settings.SettingValue.StringValue("Hi"),
                ["Locked"] = new Settings.SettingValue.StringValue("changed"),
                ["Removed"] = new Settings.SettingValue.StringValue("stale"),
            }));

        plugIn.Enabled.Should().BeFalse();
        plugIn.Settings.GetString("Greeting").Should().Be("Hi");
        plugIn.Settings.GetString("Locked").Should().Be("fixed");
        plugIn.Settings.ContainsKey("Removed").Should().BeFalse();
        plugIn.Hooks.Where(h => h.Id is Greet or Fail or OpenHello).Select(h => (h.Id, h.Enabled)).Should().BeEquivalentTo([
            (Greet, false),
            (Fail, true),
            (OpenHello, true),
        ]);
    }

    [Fact]
    public async Task TogglesHooksAndThePlugIn()
    {
        var session = Loaded(SaveFilePath.PathFrom(GameVersion.E));
        var host = new PlugInHost(session);
        var ran = new List<PlugInRan>();
        host.Ran += ran.Add;
        host.Register(TestPlugIn);

        host.SetToggle(TestPlugInId, Fail, false);
        host.SetToggle(TestPlugInId, OpenHello, true);
        await host.RunAll<IQuickAction>(h => h.OnActionRequested());

        host.Find(TestPlugInId)!.Hooks.Single(h => h.Id == Fail).Enabled.Should().BeFalse();
        ran.Select(r => r.HookId).Should().BeEquivalentTo([Greet, OpenHello]);

        ran.Clear();
        host.SetEnabled(TestPlugInId, false);
        await host.RunAll<IQuickAction>(h => h.OnActionRequested());

        host.Find(TestPlugInId)!.Enabled.Should().BeFalse();
        ran.Should().BeEmpty();
    }

    [Fact]
    public async Task HooksGetTheLoadedGameAndTheirOwnSettings()
    {
        var session = Loaded(SaveFilePath.PathFrom(GameVersion.E));
        var host = new PlugInHost(session);
        var ran = new List<PlugInRan>();
        host.Ran += ran.Add;
        host.Register(TestPlugIn);
        host.SetToggle(TestPlugInId, Fail, false);

        host.UpdateSetting(TestPlugInId, "Greeting", new Settings.SettingValue.StringValue("Hi"));
        await host.RunAll<IQuickAction>(h => h.OnActionRequested());

        host.Find(TestPlugInId)!.Settings.GetString("Greeting").Should().Be("Hi");
        var notification = ran.Should().ContainSingle().Subject.Outcome.Should().BeOfType<Outcome.Notification>().Subject;
        notification.Message.Should().Be($"Hi, {session.Game!.Trainer.Name}");
    }

    [Fact]
    public void ReadOnlySettingsCantBeUpdated()
    {
        var host = new PlugInHost(new Session());
        host.Register(TestPlugIn);

        var update = () => host.UpdateSetting(TestPlugInId, "Locked", new Settings.SettingValue.StringValue("changed"));

        update.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UnregistersAPlugIn()
    {
        var host = new PlugInHost(new Session());
        host.Register(TestPlugIn);

        host.Unregister(TestPlugInId);

        host.List().Should().BeEmpty();
        host.Find(TestPlugInId).Should().BeNull();
    }

    [Fact]
    public void RegisteringAgainReplacesThePlugIn()
    {
        var host = new PlugInHost(new Session());
        host.Register(TestPlugIn);
        host.SetEnabled(TestPlugInId, false);

        host.Register(TestPlugIn);

        host.List().Should().ContainSingle().Which.Enabled.Should().BeTrue();
    }

    [Fact]
    public async Task PublishesEachRunWithItsOutcomeOrFailureAndInvalidatesEverything()
    {
        var session = Loaded(SaveFilePath.PathFrom(GameVersion.E));
        var host = new PlugInHost(session);
        var ran = new List<PlugInRan>();
        var invalidated = new List<string[]>();
        host.Ran += ran.Add;
        host.Register(TestPlugIn);
        host.SetToggle(TestPlugInId, OpenHello, true);
        session.Changed += invalidated.Add;

        await host.RunAll<IQuickAction>(h => h.OnActionRequested());

        ran.Should().HaveCount(3).And.OnlyContain(r => r.PlugInId == TestPlugInId);
        ran.Single(r => r.HookId == Greet).Outcome.Should().BeOfType<Outcome.Notification>();
        ran.Single(r => r.HookId == OpenHello).Outcome.Should().BeOfType<Outcome.PageRequest>()
            .Which.Path.Should().Be("hello");
        var failed = ran.Single(r => r.HookId == Fail);
        failed.Outcome.Should().BeNull();
        failed.Failure.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Be("Failed on purpose");
        invalidated.Should().ContainSingle().Which.Should().Equal(Topics.All);
    }

    [Fact]
    public async Task RunningWithoutEnabledHooksInvalidatesNothing()
    {
        var session = new Session();
        var host = new PlugInHost(session);
        var invalidated = new List<string[]>();
        session.Changed += invalidated.Add;

        await host.RunAll<IQuickAction>(h => h.OnActionRequested());

        invalidated.Should().BeEmpty();
    }

    [Fact]
    public void DetectsTheSdkAnAssemblyIsBuiltAgainst()
    {
        PlugInHost.DetectSdk(TestPlugIn).Should().Be(PlugInSdk.V2);
        PlugInHost.DetectSdk(V1PlugIn).Should().Be(PlugInSdk.V1);
        PlugInHost.DetectSdk(File.ReadAllBytes(typeof(Session).Assembly.Location)).Should().Be(PlugInSdk.None);
        PlugInHost.DetectSdk([1, 2, 3]).Should().Be(PlugInSdk.None);
    }

    [Fact]
    public void RejectsAV1AssemblyWithoutLoadingIt()
    {
        var host = new PlugInHost(new Session());

        var register = () => host.Register(V1PlugIn);

        register.Should().Throw<IncompatiblePlugInException>().Which.Sdk.Should().Be(PlugInSdk.V1);
        host.List().Should().BeEmpty();
        AppDomain.CurrentDomain.GetAssemblies()
            .Should().NotContain(a => a.GetName().Name == "PKHeX.Web.Plugins.Demo");
    }
}
