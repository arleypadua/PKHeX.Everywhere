using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Legacy;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class EmeraldLegacyFormatTests
{
    private readonly EmeraldLegacyFormat _format = new();
    private static byte[] Save(string path) => File.ReadAllBytes(path);

    [Fact]
    public void DescribesItself() =>
        new SaveFormatDescription(_format.Id, _format.Name, _format.BaseGame)
            .Should().Be(new SaveFormatDescription("emerald-legacy", "Pokémon Emerald Legacy", GameVersion.E));

    // Legacy keeps Emerald's event system, so it supports Events and nothing else: everything else PKHeX
    // would infer from the base game (legality, encounters, Showdown) is wrong for a hack.
    [Fact]
    public void SupportsOnlyEvents() => _format.Capabilities.Should().Equal(Capability.Events);

    [Fact]
    public void DetectsALegacySaveAsAPossibleMatch() =>
        _format.Detect(Save(SaveFilePath.EmeraldLegacy)).Should().Be(SaveFormatMatch.Possible);

    [Fact]
    public void DetectsALegacySaveWithTheClockTrailer()
    {
        var withTrailer = Save(SaveFilePath.EmeraldLegacy).Concat(new byte[0x10]).ToArray();
        _format.Detect(withTrailer).Should().Be(SaveFormatMatch.Possible);
    }

    [Fact]
    public void DetectsALegacySaveWithAnotherEmulatorClockFooter() =>
        _format.Detect(SaveFilePath.WithClockFooter(Save(SaveFilePath.EmeraldLegacy), 0x2C)).Should().Be(SaveFormatMatch.Possible);

    [Fact]
    public void ExportsASaveWithAnotherEmulatorClockFooterUnchanged()
    {
        var data = SaveFilePath.WithClockFooter(Save(SaveFilePath.EmeraldLegacy), 0x2C);

        Game.LoadFrom(data.ToArray(), format: _format).ToByteArray().Should().Equal(data);
    }

    [Theory]
    [InlineData(0x11)]
    [InlineData(0x40)]
    public void DoesNotDetectASaveWithAnImplausibleFooter(int length) =>
        _format.Detect(SaveFilePath.WithClockFooter(Save(SaveFilePath.EmeraldLegacy), length)).Should().Be(SaveFormatMatch.No);

    [Theory]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.FireRed)]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    [InlineData(SaveFilePath.Imperium)]
    [InlineData(SaveFilePath.Crystal)]
    public void DoesNotDetectAnyOtherSave(string path) =>
        _format.Detect(Save(path)).Should().Be(SaveFormatMatch.No);

    [Fact]
    public void DoesNotDetectAWrongSize() =>
        _format.Detect(Save(SaveFilePath.EmeraldLegacy).AsSpan(0, 0x10000).ToArray()).Should().Be(SaveFormatMatch.No);

    [Fact]
    public void DoesNotDetectABrokenSignature()
    {
        var data = Save(SaveFilePath.EmeraldLegacy);
        data[0xFF8] ^= 0xFF;
        data[0xFF8 + (14 * 0x1000)] ^= 0xFF;
        _format.Detect(data).Should().Be(SaveFormatMatch.No);
    }

    [Fact]
    public void LoadsWithItsFormat()
    {
        var game = SaveFilePath.Load(SaveFilePath.EmeraldLegacy);
        game.Format!.Id.Should().Be("emerald-legacy");
        game.SaveFile.Version.Should().Be(GameVersion.E);
    }
}
