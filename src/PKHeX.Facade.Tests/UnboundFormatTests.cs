using AwesomeAssertions;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class UnboundFormatTests
{
    private static readonly UnboundFormat Format = new();
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.Unbound);

    [Fact]
    public void DetectsAnUnboundSaveWithAnEmulatorClockFooter() =>
        Format.Detect(SaveFilePath.WithClockFooter(Fixture, 0x2C)).Should().Be(SaveFormatMatch.Certain);

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
