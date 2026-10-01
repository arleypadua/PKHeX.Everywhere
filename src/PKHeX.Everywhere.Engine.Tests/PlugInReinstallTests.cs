using AwesomeAssertions;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.PlugIns;

namespace PKHeX.Everywhere.Engine.Tests;

public class PlugInReinstallTests
{
    private const string TestPlugInId = "PKHeX.Everywhere.Engine.Tests.PlugIn";
    private const string Greet = "PKHeX.Everywhere.Engine.Tests.PlugIn.Greet";

    private static byte[] V2PlugIn => PlugInBytes("PKHeX.Everywhere.Engine.Tests.PlugIn");
    private static byte[] V1PlugIn => PlugInBytes("V1PlugIn");

    private static byte[] PlugInBytes(string name) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{name}.dll"));

    private static readonly StoredPlugIn Stored = new(
        Enabled: false,
        Toggles: new Dictionary<string, bool> { [Greet] = false },
        Settings: new Dictionary<string, Settings.SettingValue>
        {
            ["Greeting"] = new Settings.SettingValue.StringValue("Hi"),
        });

    [Fact]
    public void TheHostSupportsSdk2Only()
    {
        PlugInHost.SupportedSdks.Should().BeEquivalentTo([2]);
    }

    [Fact]
    public async Task AStoredV1PlugInUpdatesToItsV2VersionKeepingItsSettings()
    {
        var host = new PlugInHost(new Session());
        var downloaded = new List<PublishedVersion>();

        var update = await host.UpdateToCompatible(V1PlugIn, Stored,
            [new("1.0.0", 1), new("2.0.0", 2)],
            v => { downloaded.Add(v); return Task.FromResult(V2PlugIn); });

        update.Should().NotBeNull();
        update!.Version.Should().Be(new PublishedVersion("2.0.0", 2));
        update.Assembly.Should().Equal(V2PlugIn);
        downloaded.Should().Equal(new PublishedVersion("2.0.0", 2));
        var plugIn = host.List().Should().ContainSingle().Subject;
        plugIn.Should().BeSameAs(update.PlugIn);
        plugIn.Id.Should().Be(TestPlugInId);
        plugIn.Enabled.Should().BeFalse();
        plugIn.Settings.GetString("Greeting").Should().Be("Hi");
        plugIn.IsHookEnabled(Greet).Should().BeFalse();
    }

    [Fact]
    public async Task AStoredV1PlugInWhoseSourceHasNoV2VersionNeedsReinstall()
    {
        var host = new PlugInHost(new Session());
        var downloads = 0;

        var update = await host.UpdateToCompatible(V1PlugIn, Stored,
            [new("1.0.0", 1), new("1.1.0", 1)],
            _ => { downloads++; return Task.FromResult(V2PlugIn); });

        update.Should().BeNull();
        downloads.Should().Be(0);
        host.List().Should().BeEmpty();
    }

    [Fact]
    public async Task AStoredV1PlugInNeedsReinstallWhenTheDownloadFails()
    {
        var host = new PlugInHost(new Session());

        var update = await host.UpdateToCompatible(V1PlugIn, Stored,
            [new("2.0.0", 2)],
            _ => throw new HttpRequestException("offline"));

        update.Should().BeNull();
        host.List().Should().BeEmpty();
    }

    [Fact]
    public async Task AStoredV1PlugInNeedsReinstallWhenTheDownloadedVersionIsntV2()
    {
        var host = new PlugInHost(new Session());

        var update = await host.UpdateToCompatible(V1PlugIn, Stored,
            [new("2.0.0", 2)],
            _ => Task.FromResult(V1PlugIn));

        update.Should().BeNull();
        host.List().Should().BeEmpty();
    }
}
