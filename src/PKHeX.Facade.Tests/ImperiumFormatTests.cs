using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Expansion.Imperium;
using PKHeX.Facade.Abstractions;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade.Tests;

public class ImperiumFormatTests
{
    private static readonly ImperiumFormat Format = new();
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.Imperium);

    [Fact]
    public void DescribesEmeraldImperium()
    {
        Format.Id.Should().Be("emerald-imperium");
        Format.Name.Should().Be("Emerald Imperium");
        Format.BaseGame.Should().Be(GameVersion.E);
        Format.Capabilities.Should().BeEmpty();
    }

    [Fact]
    public void DetectsAnImperiumSaveAsCertain() =>
        Format.Detect(Fixture).Should().Be(SaveFormatMatch.Certain);

    [Fact]
    public void DetectsAnImperiumSaveWithoutTheClockTrailer() =>
        Format.Detect(Fixture[..0x20000]).Should().Be(SaveFormatMatch.Certain);

    [Theory]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.FireRed)]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    public void DoesNotDetectOtherGen3Saves(string path) =>
        Format.Detect(File.ReadAllBytes(path)).Should().Be(SaveFormatMatch.No);

    [Fact]
    public void DoesNotDetectASaveOfAnotherSize() =>
        Format.Detect([.. Fixture, 0]).Should().Be(SaveFormatMatch.No);

    [Fact]
    public void DoesNotDetectASaveWithABadChecksum()
    {
        var data = Fixture.ToArray();
        data[0xA000] ^= 0xFF;

        Format.Detect(data).Should().Be(SaveFormatMatch.No);
    }

    [Fact]
    public void DoesNotDetectASaveWithAnotherSignature()
    {
        var data = Fixture.ToArray();
        WriteUInt32LittleEndian(data.AsSpan(0xFF8), 0x08012025 + 1);

        Format.Detect(data).Should().Be(SaveFormatMatch.No);
    }

    [Fact]
    public void DoesNotDetectASaveWithARepeatedSectorId()
    {
        var data = Fixture.ToArray();
        data.AsSpan(0x1FF4, 2).CopyTo(data.AsSpan(0xFF4));

        Format.Detect(data).Should().Be(SaveFormatMatch.No);
    }

    [Fact]
    public void RefusesToLoadAVersion1SaveForNow()
    {
        var load = () => Format.Load(Fixture);

        load.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void ASaveRotatedLikeEmeraldDoesNotLoadWhileTheFormatIsOff()
    {
        var data = RotatedToSectorZero();
        SaveUtil.GetSaveFile(data).Should().BeOfType<SAV3E>();

        var load = () => Game.LoadFrom(data);

        load.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void DetectsAVersion2SaveAsCertain() =>
        Format.Detect(Version2Save()).Should().Be(SaveFormatMatch.Certain);

    [Fact]
    public void RefusesToLoadAVersion2Save()
    {
        var load = () => Format.Load(Version2Save());

        load.Should().Throw<GameNotLoadedException>();
    }

    private static byte[] RotatedToSectorZero()
    {
        var data = Fixture.ToArray();
        var first = Enumerable.Range(0, 28).First(sector => ReadUInt16LittleEndian(Fixture.AsSpan((sector * 0x1000) + 0xFF4)) == 0);
        for (var sector = 0; sector < 28; sector++)
            Fixture.AsSpan(((sector + first) % 28) * 0x1000, 0x1000).CopyTo(data.AsSpan(sector * 0x1000));

        return data;
    }

    private static byte[] Version2Save()
    {
        int[] checksummed = [2940, 4084, 4084, 4084, 772, 0, 0, 0, 0, 0, 0, 0, 0, 0, .. Enumerable.Repeat(4084, 12), 1512, 0];
        var data = new byte[0x20000];
        new Random(20).NextBytes(data);
        for (var id = 0; id < checksummed.Length; id++)
        {
            var sector = data.AsSpan(((id + 3) % 28) * 0x1000, 0x1000);
            WriteUInt16LittleEndian(sector[0xFF4..], (ushort)id);
            WriteUInt16LittleEndian(sector[0xFF6..], Checksums.CheckSum32(sector[..checksummed[id]]));
            WriteUInt32LittleEndian(sector[0xFF8..], 0x08012025);
        }

        return data;
    }
}
