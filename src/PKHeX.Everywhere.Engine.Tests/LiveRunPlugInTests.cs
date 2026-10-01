using AwesomeAssertions;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.PlugIns;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class LiveRunPlugInTests
{
    private const string LiveRunId = "PKHeX.Web.Plugins.LiveRun";
    private const string EmeraldRom = "Emerald ROM";

    private static byte[] LiveRun =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{LiveRunId}.dll"));

    private static (Session Session, PlugInHost Host) Hosted(StoredPlugIn? stored = null)
    {
        var session = new Session();
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
