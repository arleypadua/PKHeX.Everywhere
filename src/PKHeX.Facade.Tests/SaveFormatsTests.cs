using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Abstractions;

namespace PKHeX.Facade.Tests;

public class SaveFormatsTests
{
    private static readonly byte[] Marked = Enumerable.Repeat((byte)0x5A, 0x1234).ToArray();
    private static readonly byte[] PossiblyMarked = PossiblyMarkedEmerald();
    private static readonly byte[] MarkedForDisabled = Enumerable.Repeat((byte)0xD1, 0x1234).ToArray();
    private static readonly byte[] MarkedForEnabledLater = Enumerable.Repeat((byte)0xE1, 0x1234).ToArray();
    private static readonly byte[] PossiblyMarkedForDisabled = Enumerable.Repeat((byte)0xD2, 0x1234).ToArray();

    static SaveFormatsTests()
    {
        SaveFormats.Register(new MarkedFormat("marked", Marked));
        SaveFormats.Register(new PossiblyMarkedFormat("possible", PossiblyMarked));
        SaveFormats.Register(new PossiblyMarkedFormat("also-possible", PossiblyMarked));
        SaveFormats.Register(new PossiblyMarkedFormat("disabled-possible", PossiblyMarked), enabled: false);
        SaveFormats.Register(new MarkedFormat("disabled-marked", MarkedForDisabled), enabled: false);
        SaveFormats.Register(new MarkedFormat("enabled-later", MarkedForEnabledLater), enabled: false);
        SaveFormats.Register(new PossiblyMarkedFormat("only-disabled-possible", PossiblyMarkedForDisabled), enabled: false);
    }

    private static byte[] PossiblyMarkedEmerald()
    {
        var data = File.ReadAllBytes(SaveFilePath.Emerald);
        data[^1] ^= 0xFF;
        return data;
    }

    [Fact]
    public void ACertainMatchLoadsWithItsFormat() =>
        Game.LoadFrom(Marked).SaveFile.Version.Should().Be(GameVersion.SL);

    [Fact]
    public void PossibleMatchesRequireAChoiceBetweenThem()
    {
        var load = () => Game.LoadFrom(PossiblyMarked);

        load.Should().Throw<FormatChoiceRequiredException>()
            .Which.Candidates.Select(format => format.Id).Should().Equal("possible", "also-possible");
    }

    [Fact]
    public void ASaveNoFormatMatchesLoadsWithPKHeX() =>
        Game.LoadFrom(SaveFilePath.Emerald).SaveFile.Should().BeOfType<SAV3E>();

    [Fact]
    public void ChoosingPKHeXLoadsAPossibleMatchWithPKHeXsDetection()
    {
        var game = Game.LoadFrom(PossiblyMarked, "emerald.sav", SaveFormats.Find("pkhex"));

        game.SaveFile.Should().BeOfType<SAV3E>();
        game.Format.Should().BeNull();
        game.Capabilities.Should().BeEquivalentTo(Enum.GetValues<Capability>());
    }

    [Fact]
    public void PKHeXIsNotARegisteredFormat() =>
        SaveFormats.All.Should().NotContain(SaveFormats.PKHeX);

    [Fact]
    public void RegisteringAFormatTwiceKeepsOne()
    {
        SaveFormats.Register(new MarkedFormat("marked", Marked));

        SaveFormats.All.Count(format => format.Id == "marked").Should().Be(1);
    }

    [Fact]
    public void FindsARegisteredFormatById() =>
        SaveFormats.Find("marked").Should().BeOfType<MarkedFormat>();

    [Fact]
    public void FindsNoFormatForAnUnknownId() =>
        SaveFormats.Find("nope").Should().BeNull();

    [Fact]
    public void AChosenFormatLoadsTheSaveEvenWhenPKHeXKnowsIt()
    {
        var game = Game.LoadFrom(File.ReadAllBytes(SaveFilePath.Emerald), "emerald.sav", SaveFormats.Find("marked")!);

        game.SaveFile.Version.Should().Be(GameVersion.SL);
        game.Format!.Id.Should().Be("marked");
    }

    [Fact]
    public void AChosenFormatThatCannotReadTheSaveFailsToLoad()
    {
        var load = () => Game.LoadFrom(Marked, "marked.sav", SaveFormats.Find("possible")!);

        load.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void ChoosingPKHeXForASavePKHeXCannotReadFailsToLoad()
    {
        var load = () => Game.LoadFrom(Marked, "marked.sav", SaveFormats.PKHeX);

        load.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void ACertainMatchOnADisabledFormatFailsToLoad()
    {
        var load = () => Game.LoadFrom(MarkedForDisabled);

        load.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void AnEnabledFormatLoadsTheSaveItMatches()
    {
        var load = () => Game.LoadFrom(MarkedForEnabledLater);
        load.Should().Throw<GameNotLoadedException>();

        SaveFormats.Enable("enabled-later").Should().BeTrue();

        load().Format!.Id.Should().Be("enabled-later");
    }

    [Fact]
    public void EnablingAnUnknownFormatFindsNothing() =>
        SaveFormats.Enable("nope").Should().BeFalse();

    [Fact]
    public void ADisabledPossibleMatchIsNotACandidate()
    {
        var load = () => Game.LoadFrom(PossiblyMarked);

        load.Should().Throw<FormatChoiceRequiredException>()
            .Which.Candidates.Select(format => format.Id).Should().NotContain("disabled-possible");
    }

    [Fact]
    public void APossibleMatchOnlyOnDisabledFormatsFailsToLoad()
    {
        var load = () => Game.LoadFrom(PossiblyMarkedForDisabled);

        load.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void FindsNoDisabledFormat() =>
        SaveFormats.Find("disabled-marked").Should().BeNull();

    [Fact]
    public void ADisabledFormatIsNotListed() =>
        SaveFormats.All.Select(format => format.Id).Should().NotContain("disabled-marked");

    private sealed class MarkedFormat(string id, byte[] marker) : ISaveFormat
    {
        public string Id => id;
        public string Name => id;
        public GameVersion BaseGame => GameVersion.SL;
        public IReadOnlySet<Capability> Capabilities { get; } = new HashSet<Capability>();
        public SaveFormatMatch Detect(ReadOnlySpan<byte> data) => data.SequenceEqual(marker) ? SaveFormatMatch.Certain : SaveFormatMatch.No;
        public SaveFile Load(byte[] data) => BlankSaveFile.Get(GameVersion.SL, "Marked");
    }

    private sealed class PossiblyMarkedFormat(string id, byte[] marker) : ISaveFormat
    {
        public string Id => id;
        public string Name => id;
        public GameVersion BaseGame => GameVersion.E;
        public IReadOnlySet<Capability> Capabilities { get; } = new HashSet<Capability>();
        public SaveFormatMatch Detect(ReadOnlySpan<byte> data) => data.SequenceEqual(marker) ? SaveFormatMatch.Possible : SaveFormatMatch.No;
        public SaveFile Load(byte[] data) => throw new InvalidOperationException("This format can't read any save.");
    }
}
