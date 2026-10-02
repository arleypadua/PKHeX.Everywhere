using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Abstractions;

namespace PKHeX.Facade.Tests;

public class SaveFormatsTests
{
    private static readonly byte[] Marked = Enumerable.Repeat((byte)0x5A, 0x1234).ToArray();

    static SaveFormatsTests()
    {
        SaveFormats.Register(new MarkedFormat());
        SaveFormats.Register(new PossibleForEverything());
    }

    [Fact]
    public void ACertainMatchLoadsWithItsFormat() =>
        Game.LoadFrom(Marked).SaveFile.Version.Should().Be(GameVersion.SL);

    [Fact]
    public void APossibleMatchFallsBackToPKHeX() =>
        Game.LoadFrom(SaveFilePath.Emerald).SaveFile.Should().BeOfType<SAV3E>();

    [Fact]
    public void RegisteringAFormatTwiceKeepsOne()
    {
        SaveFormats.Register(new MarkedFormat());

        SaveFormats.All.Count(format => format.Id == "marked").Should().Be(1);
    }

    private sealed class MarkedFormat : ISaveFormat
    {
        public string Id => "marked";
        public string Name => "Marked";
        public GameVersion BaseGame => GameVersion.SL;
        public IReadOnlySet<Capability> Capabilities { get; } = new HashSet<Capability>();
        public SaveFormatMatch Detect(ReadOnlySpan<byte> data) => data.SequenceEqual(Marked) ? SaveFormatMatch.Certain : SaveFormatMatch.No;
        public SaveFile Load(byte[] data) => BlankSaveFile.Get(GameVersion.SL, "Marked");
    }

    private sealed class PossibleForEverything : ISaveFormat
    {
        public string Id => "possible";
        public string Name => "Possible";
        public GameVersion BaseGame => GameVersion.FR;
        public IReadOnlySet<Capability> Capabilities { get; } = new HashSet<Capability>();
        public SaveFormatMatch Detect(ReadOnlySpan<byte> data) => SaveFormatMatch.Possible;
        public SaveFile Load(byte[] data) => throw new InvalidOperationException("A possible match must not load.");
    }
}
