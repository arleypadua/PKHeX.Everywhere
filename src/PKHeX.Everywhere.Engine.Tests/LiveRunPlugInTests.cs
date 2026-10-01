using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.PlugIns;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class LiveRunPlugInTests
{
    private const string LiveRunId = "PKHeX.Web.Plugins.LiveRun";
    private const string EmeraldRom = "Emerald ROM";
    private const string GoToLiveRun = $"{LiveRunId}.GoToLiveRun";

    private static byte[] LiveRun =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{LiveRunId}.dll"));

    private static (Session Session, PlugInHost Host) Hosted(StoredPlugIn? stored = null, string? saveFile = null)
    {
        var session = saveFile is null ? new Session() : Loaded(saveFile);
        var host = new PlugInHost(session);
        host.Register(LiveRun, stored);
        return (session, host);
    }

    [Fact]
    public void DeclaresItsPageWithAnEmptyLayout()
    {
        var (session, _) = Hosted();

        var page = Value(Dispatcher.Dispatch(session, "plugins.pages", "[]"))!.AsArray().Should().ContainSingle().Subject!;
        page["plugInId"]!.GetValue<string>().Should().Be(LiveRunId);
        page["path"]!.GetValue<string>().Should().Be("live-run");
        page["layout"]!.GetValue<string>().Should().Be("empty");
    }

    [Fact]
    public void ReturnsItsPageModule()
    {
        var (session, _) = Hosted();

        var source = Value(Dispatcher.Dispatch(session, "plugins.pageModule", Args(LiveRunId, "live-run")))!.GetValue<string>();

        source.Should().Contain("export function mount(element, ctx)").And.Contain("ctx.loadSave");
    }

    private static JsonNode PlayAction(Session session) =>
        Value(Dispatcher.Dispatch(session, "plugins.actions", Args("quick", null!)))!.AsArray()
            .Should().ContainSingle(a => a!["id"]!.GetValue<string>() == GoToLiveRun).Subject!;

    [Fact]
    public void PlayIsDisabledWithAReasonUntilARomIsUploadedForTheSavesVersion()
    {
        var (session, host) = Hosted(saveFile: SaveFilePath.Emerald);

        var disabled = PlayAction(session);
        disabled["label"]!.GetValue<string>().Should().Be("Play with this save");
        disabled["disabled"]!.GetValue<bool>().Should().BeTrue();
        disabled["reason"]!.GetValue<string>().Should().Be("Version Emerald not configured");

        host.UpdateSetting(LiveRunId, EmeraldRom, new Settings.SettingValue.FileValue([1, 2, 3], "emerald.gba"));

        var enabled = PlayAction(session);
        enabled["disabled"]!.GetValue<bool>().Should().BeFalse();
        enabled["reason"].Should().BeNull();
    }

    [Fact]
    public void PlayOpensTheLiveRunPage()
    {
        var (session, host) = Hosted(saveFile: SaveFilePath.Emerald);
        host.UpdateSetting(LiveRunId, EmeraldRom, new Settings.SettingValue.FileValue([1, 2, 3], "emerald.gba"));

        var outcome = Value(Dispatcher.Dispatch(session, "plugins.run", Args(GoToLiveRun, null!)))!;

        outcome["kind"]!.GetValue<string>().Should().Be("openPage");
        outcome["path"]!.GetValue<string>().Should().Be("live-run");
    }

    [Fact]
    public void KeepsStoredRomsAndHookTogglesWhenRegistered()
    {
        byte[] rom = [1, 2, 3];
        var (_, host) = Hosted(new StoredPlugIn(
            Enabled: true,
            Toggles: new Dictionary<string, bool> { [$"{LiveRunId}.GoToLiveRun"] = true },
            Settings: new Dictionary<string, Settings.SettingValue>
            {
                [EmeraldRom] = new Settings.SettingValue.FileValue(rom, "emerald.gba"),
            }));

        var plugIn = host.Find(LiveRunId)!;
        plugIn.Settings[EmeraldRom].Should().BeOfType<Settings.SettingValue.FileValue>()
            .Which.Should().Match<Settings.SettingValue.FileValue>(f => f.Value.SequenceEqual(rom) && f.FileName == "emerald.gba");
        plugIn.IsHookEnabled($"{LiveRunId}.GoToLiveRun").Should().BeTrue();
    }
}
