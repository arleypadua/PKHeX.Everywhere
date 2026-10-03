using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Everywhere.Engine.PlugIns;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class PlugInRegistrationTests
{
    private const string TestPlugInId = "PKHeX.Everywhere.Engine.Tests.PlugIn";
    private const string V1PlugInId = "PKHeX.Web.Plugins.Demo";
    private const string Greet = $"{TestPlugInId}.Greet";
    private const string Fail = $"{TestPlugInId}.Fail";
    private const string OpenHello = $"{TestPlugInId}.OpenHello";

    private static string TestPlugIn => Convert.ToBase64String(PlugInBytes(TestPlugInId));
    private static string V1PlugIn => Convert.ToBase64String(PlugInBytes("V1PlugIn"));

    private static byte[] PlugInBytes(string name) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{name}.dll"));

    private static Session Hosted() => Hosted(out _);

    private static Session Hosted(out PlugInHost host)
    {
        var session = new Session();
        host = new PlugInHost(session);
        return session;
    }

    private static string Call(params object?[] args) => JsonSerializer.Serialize(args);

    private static object Stored(bool enabled = true, object[]? toggles = null, object[]? settings = null, bool hasNewerVersion = false) =>
        new { enabled, toggles = toggles ?? [], settings = settings ?? [], hasNewerVersion };

    private static object Toggle(string hookId, bool enabled) => new { hookId, enabled };

    private static object StringSetting(string key, string value) => new { key, readOnly = false, stringValue = value };

    private static object BooleanSetting(string key, bool value) => new { key, readOnly = false, booleanValue = value };

    private static JsonNode? Register(Session session, string assembly, object? stored = null) =>
        Value(Dispatch(session, "plugins.register", Call(assembly, stored)));

    private static JsonArray Installed(Session session) => Value(Dispatch(session, "plugins.installed", "[]"))!.AsArray();

    private static JsonNode? State(Session session) => Value(Dispatch(session, "plugins.state", Call(TestPlugInId)));

    private static string Versions(params object[] versions) =>
        JsonSerializer.Serialize(versions.Select(v => v is string s ? new { version = s, sdk = 1 } : v));

    [Fact]
    public void RegisteringWithStoredStateRestoresItsEnabledTogglesAndSettings()
    {
        var session = Hosted(out var host);

        Register(session, TestPlugIn, Stored(
            enabled: false,
            toggles: [Toggle(Greet, false), Toggle(OpenHello, true)],
            settings: [StringSetting("Greeting", "Hi")]));

        var plugIn = host.Find(TestPlugInId)!;
        plugIn.Enabled.Should().BeFalse();
        plugIn.IsHookEnabled(Greet).Should().BeFalse();
        plugIn.IsHookEnabled(Fail).Should().BeTrue();
        plugIn.IsHookEnabled(OpenHello).Should().BeTrue();
        plugIn.Settings.GetString("Greeting").Should().Be("Hi");
    }

    [Fact]
    public void RegisteringDropsStoredSettingsThatAreMissingOrChangedType()
    {
        var session = Hosted(out var host);

        Register(session, TestPlugIn, Stored(settings: [BooleanSetting("Greeting", true), StringSetting("Removed", "stale")]));

        var settings = host.Find(TestPlugInId)!.Settings;
        settings.GetString("Greeting").Should().Be("Hello");
        settings.ContainsKey("Removed").Should().BeFalse();
    }

    [Fact]
    public void StateReadsBackWhatRegisteringRestored()
    {
        var session = Hosted();
        Register(session, TestPlugIn, Stored(enabled: false, toggles: [Toggle(Greet, false)], settings: [StringSetting("Greeting", "Hi")]));

        var state = State(session)!;

        state["enabled"]!.GetValue<bool>().Should().BeFalse();
        state["hasNewerVersion"]!.GetValue<bool>().Should().BeFalse();
        state["toggles"]!.AsArray().Should().ContainSingle(t => t!["hookId"]!.GetValue<string>() == Greet)
            .Which!["enabled"]!.GetValue<bool>().Should().BeFalse();
        var greeting = state["settings"]!.AsArray().Single(s => s!["key"]!.GetValue<string>() == "Greeting")!;
        greeting["stringValue"]!.GetValue<string>().Should().Be("Hi");
        greeting["readOnly"]!.GetValue<bool>().Should().BeFalse();
        var locked = state["settings"]!.AsArray().Single(s => s!["key"]!.GetValue<string>() == "Locked")!;
        locked["readOnly"]!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public void UpdatingKeepsTheStateOfTheRegisteredPlugIn()
    {
        var session = Hosted(out var host);
        Register(session, TestPlugIn, Stored(
            enabled: false,
            toggles: [Toggle(Greet, false)],
            settings: [StringSetting("Greeting", "Hi")],
            hasNewerVersion: true));

        Register(session, TestPlugIn);

        var plugIn = host.Find(TestPlugInId)!;
        plugIn.Enabled.Should().BeFalse();
        plugIn.IsHookEnabled(Greet).Should().BeFalse();
        plugIn.Settings.GetString("Greeting").Should().Be("Hi");
        host.HasNewerVersion(TestPlugInId).Should().BeFalse();
    }

    [Fact]
    public void ListsInstalledPlugIns()
    {
        var session = Hosted();
        Register(session, TestPlugIn, Stored(enabled: false, hasNewerVersion: true));

        Installed(session).Should().ContainSingle().Which!.ToJsonString().Should().Be(new JsonObject
        {
            ["id"] = TestPlugInId,
            ["name"] = "Test Plug-In",
            ["version"] = "1.2.3.0",
            ["enabled"] = false,
            ["hasNewerVersion"] = true,
            ["needsReinstall"] = false,
        }.ToJsonString());
    }

    [Fact]
    public void UnsupportedBytesAreListedAsNeedingReinstall()
    {
        var session = Hosted(out var host);

        Register(session, V1PlugIn, Stored())!["needsReinstall"]!.GetValue<bool>().Should().BeTrue();

        var installed = Installed(session).Should().ContainSingle().Subject!;
        installed["id"]!.GetValue<string>().Should().Be(V1PlugInId);
        installed["needsReinstall"]!.GetValue<bool>().Should().BeTrue();
        host.List().Should().BeEmpty();
    }

    [Fact]
    public void UnregisteringRemovesAPlugInThatNeedsReinstall()
    {
        var session = Hosted();
        Register(session, TestPlugIn, Stored());
        Register(session, V1PlugIn, Stored());

        Value(Dispatch(session, "plugins.unregister", Call(V1PlugInId)));

        Installed(session).Should().ContainSingle().Which!["id"]!.GetValue<string>().Should().Be(TestPlugInId);
    }

    [Fact]
    public void TellsWhetherTheHostSupportsAnAssembly()
    {
        var session = Hosted();

        Value(Dispatch(session, "plugins.isSupported", Call(TestPlugIn)))!.GetValue<bool>().Should().BeTrue();
        Value(Dispatch(session, "plugins.isSupported", Call(V1PlugIn)))!.GetValue<bool>().Should().BeFalse();
        Value(Dispatch(session, "plugins.isSupported", Call(Convert.ToBase64String([1, 2, 3]))))!.GetValue<bool>().Should().BeFalse();
    }

    [Fact]
    public void NewestCompatiblePicksTheHighestVersionWithASupportedSdk()
    {
        var session = Hosted();
        var versions = Versions("1.1.3", new { version = "2.0.10", sdk = 3 }, new { version = "2.0.9", sdk = 3 }, new { version = "3.0.0", sdk = 2 });

        var newest = Value(Dispatch(session, "plugins.newestCompatible", $"[{versions}, null]"))!;

        newest.ToJsonString().Should().Be(new JsonObject { ["version"] = "2.0.10", ["sdk"] = 3 }.ToJsonString());
        Value(Dispatch(session, "plugins.newestCompatible", $"[{Versions("1.0.0")}, null]")).Should().BeNull();
    }

    [Fact]
    public void NewestCompatibleFlagsAnInstalledPlugInWithANewerVersion()
    {
        var session = Hosted();
        Register(session, TestPlugIn);
        var changed = new List<string>();
        session.Changed += changed.AddRange;

        Dispatch(session, "plugins.newestCompatible", $"[{Versions(new { version = "1.2.3", sdk = 3 })}, \"{TestPlugInId}\"]");
        Installed(session)[0]!["hasNewerVersion"]!.GetValue<bool>().Should().BeFalse();
        changed.Should().BeEmpty();

        Dispatch(session, "plugins.newestCompatible", $"[{Versions(new { version = "1.3.0", sdk = 3 })}, \"{TestPlugInId}\"]");

        Installed(session)[0]!["hasNewerVersion"]!.GetValue<bool>().Should().BeTrue();
        State(session)!["hasNewerVersion"]!.GetValue<bool>().Should().BeTrue();
        changed.Should().Equal(Topics.PlugIns);
    }

    [Fact]
    public void RegisteringAndUnregisteringReportThePlugInsTopic()
    {
        var session = Hosted();
        var changed = new List<string[]>();
        session.Changed += changed.Add;

        Register(session, TestPlugIn);
        Value(Dispatch(session, "plugins.unregister", Call(TestPlugInId)));

        changed.Should().HaveCount(2).And.OnlyContain(topics => topics.SequenceEqual(new[] { Topics.PlugIns }));
        Installed(session).Should().BeEmpty();
    }

    [Fact]
    public void InstallingAndUpdatingRaiseEventsButRestoringDoesnt()
    {
        var session = Hosted();
        var published = new List<IEngineEvent>();
        session.Published += published.Add;

        Register(session, TestPlugIn, Stored());
        published.Should().BeEmpty();

        Value(Dispatch(session, "plugins.unregister", Call(TestPlugInId)));
        Register(session, TestPlugIn);
        published.Should().Equal(new PlugInInstalled(TestPlugInId, "1.2.3.0"));

        published.Clear();
        Register(session, TestPlugIn, Stored());
        published.Should().Equal(new PlugInUpdated(TestPlugInId, "1.2.3.0"));
    }
}
