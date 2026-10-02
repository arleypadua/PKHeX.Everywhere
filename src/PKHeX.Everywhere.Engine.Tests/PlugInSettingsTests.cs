using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.PlugIns;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class PlugInSettingsTests
{
    private const string TestPlugInId = "PKHeX.Everywhere.Engine.Tests.PlugIn";
    private const string Greet = $"{TestPlugInId}.Greet";

    private static Session Hosted(out PlugInHost host, out List<string[]> changed)
    {
        var session = new Session();
        host = new PlugInHost(session);
        host.Register(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{TestPlugInId}.dll")));
        var reported = changed = [];
        session.Changed += reported.Add;
        return session;
    }

    private static string Call(params object?[] args) => JsonSerializer.Serialize(args);

    private static JsonNode Setting(JsonNode state, string key) =>
        state["settings"]!.AsArray().Single(s => s!["key"]!.GetValue<string>() == key)!;

    private static JsonNode Toggle(JsonNode state, string hookId) =>
        state["toggles"]!.AsArray().Single(t => t!["hookId"]!.GetValue<string>() == hookId)!;

    private static string[] QuickActions(Session session) => Value(Dispatch(session, "plugins.actions", Call("quick", null)))!
        .AsArray().Select(a => a!["id"]!.GetValue<string>()).ToArray();

    [Fact]
    public void DetailsDescribeThePlugIn()
    {
        var session = Hosted(out _, out _);

        var details = Value(Dispatch(session, "plugins.details", Call(TestPlugInId)))!;

        details["name"]!.GetValue<string>().Should().Be("Test Plug-In");
        details["description"]!.GetValue<string>().Should().Be("A plug-in for host tests");
        details["information"]!.GetValue<string>().Should().Be("Read me first");
        details["version"]!.GetValue<string>().Should().Be("1.2.3.0");
        details["enabled"]!.GetValue<bool>().Should().BeTrue();
        details["hooks"]!.AsArray().Should().Contain(h => h!["id"]!.GetValue<string>() == Greet
                                                         && h["description"]!.GetValue<string>() == "Greets the trainer"
                                                         && h["enabled"]!.GetValue<bool>());
        Setting(details, "Locked")["readOnly"]!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public void DetailsOfAMissingPlugInAreNotFound()
    {
        var session = Hosted(out _, out _);

        Error(Dispatch(session, "plugins.details", Call("Missing"))).Should().Be(ErrorCodes.NotFound);
    }

    [Fact]
    public void SettingEnabledReturnsTheUpdatedState()
    {
        var session = Hosted(out var host, out var changed);

        var state = Value(Dispatch(session, "plugins.setEnabled", Call(TestPlugInId, false)))!;

        state["enabled"]!.GetValue<bool>().Should().BeFalse();
        host.Find(TestPlugInId)!.Enabled.Should().BeFalse();
        changed.Should().ContainSingle().Which.Should().Equal(Topics.PlugIns);
    }

    [Fact]
    public void SettingAHookToggleReturnsTheUpdatedState()
    {
        var session = Hosted(out var host, out var changed);

        var state = Value(Dispatch(session, "plugins.setHookEnabled", Call(TestPlugInId, Greet, false)))!;

        Toggle(state, Greet)["enabled"]!.GetValue<bool>().Should().BeFalse();
        host.Find(TestPlugInId)!.IsHookEnabled(Greet).Should().BeFalse();
        changed.Should().ContainSingle().Which.Should().Equal(Topics.PlugIns);
    }

    [Fact]
    public void SettingAnUnknownHookIsABadArgument()
    {
        var session = Hosted(out _, out _);

        Error(Dispatch(session, "plugins.setHookEnabled", Call(TestPlugInId, "Unknown", false))).Should().Be(ErrorCodes.BadArguments);
    }

    [Fact]
    public void QuickActionsSkipToggledOffHooksAndDisabledPlugIns()
    {
        var session = Hosted(out _, out _);

        Dispatch(session, "plugins.setHookEnabled", Call(TestPlugInId, Greet, false));
        QuickActions(session).Should().NotContain(Greet).And.NotBeEmpty();

        Dispatch(session, "plugins.setHookEnabled", Call(TestPlugInId, Greet, true));
        Dispatch(session, "plugins.setEnabled", Call(TestPlugInId, false));
        QuickActions(session).Should().BeEmpty();
    }

    [Fact]
    public void UpdatingASettingReturnsTheUpdatedState()
    {
        var session = Hosted(out var host, out var changed);

        var state = Value(Dispatch(session, "plugins.updateSetting", Call(TestPlugInId, new { key = "Greeting", readOnly = false, stringValue = "Hi" })))!;

        Setting(state, "Greeting")["stringValue"]!.GetValue<string>().Should().Be("Hi");
        host.Find(TestPlugInId)!.Settings.GetString("Greeting").Should().Be("Hi");
        changed.Should().ContainSingle().Which.Should().Equal(Topics.PlugIns);
    }

    [Fact]
    public void UploadingAFileReturnsItsBytesInTheState()
    {
        var session = Hosted(out var host, out _);
        byte[] bytes = [1, 2, 3];

        var state = Value(Dispatch(session, "plugins.updateSetting",
            Call(TestPlugInId, new { key = "Data", readOnly = false, fileName = "data.bin", file = Convert.ToBase64String(bytes) })))!;

        var data = Setting(state, "Data");
        data["fileName"]!.GetValue<string>().Should().Be("data.bin");
        data["file"]!.GetValue<string>().Should().Be(Convert.ToBase64String(bytes));
        host.Find(TestPlugInId)!.Settings.GetFile("Data").Should().Equal(bytes);
        Setting(Value(Dispatch(session, "plugins.details", Call(TestPlugInId)))!, "Data")["file"].Should().BeNull();
    }

    [Fact]
    public void RemovingAFileEmptiesTheSetting()
    {
        var session = Hosted(out var host, out _);
        host.UpdateSetting(TestPlugInId, "Data", new Settings.SettingValue.FileValue([1, 2, 3], "data.bin"));

        var state = Value(Dispatch(session, "plugins.updateSetting", Call(TestPlugInId, new { key = "Data", readOnly = false, fileName = "", file = "" })))!;

        Setting(state, "Data")["fileName"]!.GetValue<string>().Should().BeEmpty();
        host.Find(TestPlugInId)!.Settings.GetFile("Data").Should().BeEmpty();
    }

    [Theory]
    [InlineData("Locked")]
    [InlineData("Missing")]
    public void UpdatingAReadOnlyOrMissingSettingIsABadArgument(string key)
    {
        var session = Hosted(out _, out _);

        Error(Dispatch(session, "plugins.updateSetting", Call(TestPlugInId, new { key, readOnly = false, stringValue = "changed" })))
            .Should().Be(ErrorCodes.BadArguments);
    }

    [Fact]
    public void UpdatingASettingWithAnotherTypeIsABadArgument()
    {
        var session = Hosted(out var host, out _);

        Error(Dispatch(session, "plugins.updateSetting", Call(TestPlugInId, new { key = "Greeting", readOnly = false, booleanValue = true })))
            .Should().Be(ErrorCodes.BadArguments);
        host.Find(TestPlugInId)!.Settings.GetString("Greeting").Should().Be("Hello");
    }
}
