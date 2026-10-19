using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Expansion.HeartAndSoul;
using PKHeX.Facade.Abstractions;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade.Tests;

public class HeartAndSoulFormatTests
{
    private static readonly HeartAndSoulFormat Format = new();
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.HeartAndSoul);

    [Fact]
    public void DescribesHeartAndSoul()
    {
        Format.Id.Should().Be("heart-and-soul");
        Format.Name.Should().Be("Pokémon Heart & Soul");
        Format.BaseGame.Should().Be(GameVersion.E);
    }

    [Fact]
    public void DetectsAHeartAndSoulSaveAsCertain() =>
        Format.Detect(Fixture).Should().Be(SaveFormatMatch.Certain);

    [Fact]
    public void DetectsAHeartAndSoulSaveWithoutTheClockTrailer() =>
        Format.Detect(Fixture[..0x20000]).Should().Be(SaveFormatMatch.Certain);

    [Theory]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.EmeraldLegacy)]
    [InlineData(SaveFilePath.Imperium)]
    [InlineData(SaveFilePath.FireRed)]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    public void DoesNotDetectOtherGen3Saves(string path) =>
        Format.Detect(File.ReadAllBytes(path)).Should().Be(SaveFormatMatch.No);

    [Fact]
    public void DoesNotDetectASaveOfAnotherSize() =>
        Format.Detect([.. Fixture, 0]).Should().Be(SaveFormatMatch.No);

    [Fact]
    public void DoesNotDetectASaveWithAnotherSignature()
    {
        var data = Fixture.ToArray();
        for (var sector = 0; sector < 28; sector++)
            WriteUInt32LittleEndian(data.AsSpan((sector * 0x1000) + 0xFF8), 0x08012025 + 1);

        Format.Detect(data).Should().Be(SaveFormatMatch.No);
    }

    [Fact]
    public void DoesNotDetectASaveWithoutTheMagic()
    {
        var data = Fixture.ToArray();
        data[MagicIn(data, OlderSlot)] ^= 0xFF;
        data[MagicIn(data, NewerSlot)] ^= 0xFF;

        Format.Detect(data).Should().Be(SaveFormatMatch.No);
    }

    [Fact]
    public void ReadsTheMagicFromTheNewerSlot()
    {
        var data = Fixture.ToArray();
        data[MagicIn(data, NewerSlot)] ^= 0xFF;

        Format.Detect(data).Should().Be(SaveFormatMatch.No);
    }

    [Fact]
    public void ReadsTheOlderSlotWhenTheNewerHasARepeatedSectorId()
    {
        var data = Fixture.ToArray();
        data[MagicIn(data, NewerSlot)] ^= 0xFF;
        data.AsSpan((NewerSlot * 0x1000) + 0xFF4, 2).CopyTo(data.AsSpan(((NewerSlot + 1) * 0x1000) + 0xFF4));

        Format.Detect(data).Should().Be(SaveFormatMatch.Certain);
    }

    [Fact]
    public void DoesNotDetectASaveWithoutAWholeSlot()
    {
        var data = Fixture.ToArray();
        data.AsSpan((OlderSlot * 0x1000) + 0xFF4, 2).CopyTo(data.AsSpan(((OlderSlot + 1) * 0x1000) + 0xFF4));
        data.AsSpan((NewerSlot * 0x1000) + 0xFF4, 2).CopyTo(data.AsSpan(((NewerSlot + 1) * 0x1000) + 0xFF4));

        Format.Detect(data).Should().Be(SaveFormatMatch.No);
    }

    [Fact]
    public void RefusesToLoadAHeartAndSoulSaveForNow()
    {
        var load = () => Format.Load(Fixture);

        load.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void RefusesASaveOfAnotherSaveVersion()
    {
        var data = SaveFilePath.HeartAndSoulWithSaveVersion(6);
        Format.Detect(data).Should().Be(SaveFormatMatch.Certain);

        var load = () => Format.Load(data);

        load.Should().Throw<GameNotLoadedException>();
    }

    [Fact]
    public void DoesNotLoadAsEmeraldWhileTheFormatIsOff()
    {
        SaveUtil.GetSaveFile(Fixture.ToArray()).Should().BeOfType<SAV3E>();

        var load = () => Game.LoadFrom(Fixture.ToArray(), SaveFilePath.HeartAndSoul);

        load.Should().Throw<GameNotLoadedException>();
    }

    // First sector of each slot. The fixture's first slot has counter 100 and its second 101.
    private const int OlderSlot = 0;
    private const int NewerSlot = 14;

    private static int MagicIn(byte[] data, int slot)
    {
        var sector = Enumerable.Range(slot, 14).Single(index => ReadUInt16LittleEndian(data.AsSpan((index * 0x1000) + 0xFF4)) == 2);
        return (sector * 0x1000) + 0x10E0 - 3968;
    }
}
