using System.Text.Json;
using AwesomeAssertions;
using PKHeX.Everywhere.Engine.PlugIns;

namespace PKHeX.Everywhere.Engine.Tests;

public class PlugInVersionTests
{
    private static byte[] V2PlugIn => PlugInBytes("PKHeX.Everywhere.Engine.Tests.PlugIn");
    private static byte[] V1PlugIn => PlugInBytes("V1PlugIn");

    private static byte[] PlugInBytes(string name) =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{name}.dll"));

    private static PublishedVersion[] Versions(string json) =>
        JsonSerializer.Deserialize<PublishedVersion[]>(json, JsonSerializerOptions.Web)!;

    [Fact]
    public void EntriesWithoutSdkCountAsSdk1()
    {
        Versions("""["1.0.0", { "Version": "1.1.0" }, { "version": "1.2.0", "sdk": null }]""").Should().Equal(
            new PublishedVersion("1.0.0", 1),
            new PublishedVersion("1.1.0", 1),
            new PublishedVersion("1.2.0", 1));
    }

    [Fact]
    public void ReadsTheSdkOfAnEntry()
    {
        Versions("""[{ "Version": "2.0.0", "Sdk": 2 }, { "version": "3.0.0", "sdk": 3 }]""").Should().Equal(
            new PublishedVersion("2.0.0", 2),
            new PublishedVersion("3.0.0", 3));
    }

    [Fact]
    public void WritesAnEntryThatReadsBack()
    {
        PublishedVersion[] versions = [new("1.0.0", 1), new("2.0.0", 2)];

        Versions(JsonSerializer.Serialize(versions)).Should().Equal(versions);
    }

    [Fact]
    public void PicksNothingFromAManifestWithoutSdk()
    {
        PlugInHost.NewestCompatible(Versions("""["1.0.9", "1.0.10", "1.0.0"]""")).Should().BeNull();
    }

    [Fact]
    public void PicksTheNewestVersionComparingVersionNumbers()
    {
        var newest = PlugInHost.NewestCompatible(Versions("""
            [{ "Version": "2.0.9", "Sdk": 2 }, { "Version": "2.0.10", "Sdk": 2 }, { "Version": "2.0.0", "Sdk": 2 }]
            """));

        newest.Should().Be(new PublishedVersion("2.0.10", 2));
    }

    [Fact]
    public void PicksTheNewestVersionWithASupportedSdk()
    {
        var newest = PlugInHost.NewestCompatible(Versions("""
            ["1.1.2", { "Version": "2.0.0", "Sdk": 2 }, "1.1.3", { "Version": "3.0.0", "Sdk": 3 }]
            """));

        newest.Should().Be(new PublishedVersion("2.0.0", 2));
    }

    [Fact]
    public void PicksNothingWhenNoVersionHasASupportedSdk()
    {
        PlugInHost.NewestCompatible(Versions("""[{ "Version": "3.0.0", "Sdk": 3 }]""")).Should().BeNull();
        PlugInHost.NewestCompatible([]).Should().BeNull();
    }

    [Fact]
    public void SkipsVersionsThatArentVersionNumbers()
    {
        PlugInHost.NewestCompatible(Versions("""[{ "Version": "2.0.0", "Sdk": 2 }, { "Version": "latest", "Sdk": 2 }]"""))
            .Should().Be(new PublishedVersion("2.0.0", 2));
    }

    [Fact]
    public void AnInstalledV1PlugInUpdatesToTheNewestVersionWithANewerSdk()
    {
        var update = PlugInHost.SdkUpdateFor(V1PlugIn, Versions("""
            ["1.1.3", { "Version": "2.0.0", "Sdk": 2 }, { "Version": "2.1.0", "Sdk": 2 }]
            """));

        update.Should().Be(new PublishedVersion("2.1.0", 2));
    }

    [Fact]
    public void APlugInDoesntUpdateWhenNoCompatibleVersionHasANewerSdk()
    {
        PlugInHost.SdkUpdateFor(V1PlugIn, Versions("""["1.1.3", "1.1.4"]""")).Should().BeNull();
        PlugInHost.SdkUpdateFor(V1PlugIn, Versions("""["1.1.3", { "Version": "3.0.0", "Sdk": 3 }]""")).Should().BeNull();
        PlugInHost.SdkUpdateFor(V2PlugIn, Versions("""[{ "Version": "2.1.0", "Sdk": 2 }]""")).Should().BeNull();
    }

    [Fact]
    public void AnInstalledV1PlugInUpdatesToANewerSdkEvenWhenAV1VersionHasAHigherNumber()
    {
        PlugInHost.SdkUpdateFor(V1PlugIn, Versions("""["1.1.3", { "Version": "1.0.0", "Sdk": 2 }]"""))
            .Should().Be(new PublishedVersion("1.0.0", 2));
    }

    [Fact]
    public void AnUnknownInstalledSdkDoesntUpdate()
    {
        PlugInHost.SdkUpdateFor([1, 2, 3], Versions("""[{ "Version": "2.0.0", "Sdk": 2 }]""")).Should().BeNull();
    }
}
