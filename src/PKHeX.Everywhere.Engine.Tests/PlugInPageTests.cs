using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.PlugIns;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class PlugInPageTests
{
    private const string TestPlugInId = "PKHeX.Everywhere.Engine.Tests.PlugIn";

    private static byte[] TestPlugIn =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{TestPlugInId}.dll"));

    private static (Session Session, PlugInHost Host) Hosted()
    {
        var session = new Session();
        var host = new PlugInHost(session);
        host.Register(TestPlugIn);
        return (session, host);
    }

    [Fact]
    public void ListsTheDeclaredPagesOfEnabledPlugIns()
    {
        var (session, host) = Hosted();

        var page = Value(Dispatch(session, "plugins.pages", "[]"))!.AsArray().Should().ContainSingle().Subject!;
        page["plugInId"]!.GetValue<string>().Should().Be(TestPlugInId);
        page["path"]!.GetValue<string>().Should().Be("hello");
        page["title"]!.GetValue<string>().Should().Be("Hello");
        page["layout"]!.GetValue<string>().Should().Be("standard");

        host.SetEnabled(TestPlugInId, false);

        Value(Dispatch(session, "plugins.pages", "[]"))!.AsArray().Should().BeEmpty();
    }

    [Fact]
    public void ReturnsThePageModuleSource()
    {
        var (session, _) = Hosted();

        var source = Value(Dispatch(session, "plugins.pageModule", Args(TestPlugInId, "hello")))!.GetValue<string>();

        source.Should().Contain("export function mount(element, ctx)");
    }

    [Theory]
    [InlineData(TestPlugInId, "missing")]
    [InlineData("Missing.PlugIn", "hello")]
    public void AnUnknownPageIsNotFound(string plugInId, string path)
    {
        var (session, _) = Hosted();

        Error(Dispatch(session, "plugins.pageModule", Args(plugInId, path))).Should().Be(ErrorCodes.NotFound);
    }

    [Fact]
    public void APageOfADisabledPlugInIsNotFound()
    {
        var (session, host) = Hosted();
        host.SetEnabled(TestPlugInId, false);

        Error(Dispatch(session, "plugins.pageModule", Args(TestPlugInId, "hello"))).Should().Be(ErrorCodes.NotFound);
    }

    [Fact]
    public void ReturnsEachSettingTypeWithFilesAsBytes()
    {
        var (session, host) = Hosted();
        host.UpdateSetting(TestPlugInId, "Count", new Settings.SettingValue.IntegerValue(3));
        host.UpdateSetting(TestPlugInId, "On", new Settings.SettingValue.BooleanValue(true));
        host.UpdateSetting(TestPlugInId, "Rom", new Settings.SettingValue.FileValue([1, 2, 3], "rom.gba"));

        Setting(session, "Greeting")!["stringValue"]!.GetValue<string>().Should().Be("Hello");
        Setting(session, "Count")!["integerValue"]!.GetValue<int>().Should().Be(3);
        Setting(session, "On")!["booleanValue"]!.GetValue<bool>().Should().BeTrue();
        var file = Setting(session, "Rom")!;
        file["fileName"]!.GetValue<string>().Should().Be("rom.gba");
        Convert.FromBase64String(file["file"]!.GetValue<string>()).Should().Equal(1, 2, 3);
    }

    [Fact]
    public void AnUnknownSettingIsNull()
    {
        var (session, _) = Hosted();

        Setting(session, "Missing").Should().BeNull();
    }

    [Fact]
    public void TheSettingOfAnUnknownPlugInIsNotFound()
    {
        var (session, _) = Hosted();

        Error(Dispatch(session, "plugins.setting", Args("Missing.PlugIn", "Greeting"))).Should().Be(ErrorCodes.NotFound);
    }

    private static JsonNode? Setting(Session session, string key) =>
        Value(Dispatch(session, "plugins.setting", Args(TestPlugInId, key)));
}
