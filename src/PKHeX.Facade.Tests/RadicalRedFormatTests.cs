using AwesomeAssertions;
using PKHeX.Everywhere.RomHacks.Cfru.RadicalRed;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class RadicalRedFormatTests
{
    private static readonly RadicalRedFormat Format = new();
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.RadicalRed);

    [Fact]
    public void DetectsARadicalRedSaveWithAnEmulatorClockFooter() =>
        Format.Detect(SaveFilePath.WithClockFooter(Fixture, 0x2C)).Should().Be(SaveFormatMatch.Possible);

    [Fact]
    public void ExportsASaveWithAnEmulatorClockFooterUnchanged()
    {
        var data = SaveFilePath.WithClockFooter(Fixture, 0x2C);

        Game.LoadFrom(data.ToArray(), format: Format).ToByteArray().Should().Equal(data);
    }

    [Theory]
    [InlineData(0x11)]
    [InlineData(0x40)]
    public void DoesNotDetectASaveWithAnImplausibleFooter(int length) =>
        Format.Detect(SaveFilePath.WithClockFooter(Fixture, length)).Should().Be(SaveFormatMatch.No);
}
