using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Legacy;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class EmeraldLegacySaveTests
{
    private static Game Load() => SaveFilePath.Load(SaveFilePath.EmeraldLegacy);

    [Fact]
    public void ExportsTheSaveUnchanged()
    {
        var original = File.ReadAllBytes(SaveFilePath.EmeraldLegacy);
        Load().ToByteArray().Should().Equal(original);
    }

    [Fact]
    public void ReadsTheTrainer()
    {
        var game = Load();
        game.Trainer.Name.Should().Be("TESTR");
        game.Trainer.Money.Amount.Should().Be(9885);
    }

    [Fact]
    public void ReadsTheParty()
    {
        var party = Load().SaveFile.PartyData;
        party.Should().HaveCount(6);
        party.Select(p => p.Nickname).Should()
            .Equal("CACTURNE", "METAGROSS", "SALAMENCE", "SWAMPERT", "FLYGON", "MEL");
        party.Select(p => (int)p.Species).Should().Equal(332, 376, 373, 260, 330, 323);
        party.Should().OnlyContain(p => p.ChecksumValid);
    }

    [Fact]
    public void ReadsTheEightBadges()
    {
        var progress = Load().Progress;
        progress.Badges!.Earned.Should().Be(8);
    }

    // The pockets the hack moved. Their counts come from reading the fixture at Legacy's offsets.
    [Theory]
    [InlineData(InventoryType.KeyItems, 16)]
    [InlineData(InventoryType.Balls, 11)]
    [InlineData(InventoryType.TMHMs, 38)]
    [InlineData(InventoryType.Berries, 23)]
    [InlineData(InventoryType.Items, 64)]
    public void ReadsEachPouchAtItsShiftedOffset(InventoryType type, int expected)
    {
        var pouch = Load().SaveFile.Inventory.Pouches.First(p => p.Type == type);
        pouch.Items.Count(item => item.Index != 0).Should().Be(expected);
    }

    [Fact]
    public void TheItemsPocketHolds120SlotsCappedAt99()
    {
        var pouch = Load().SaveFile.Inventory.Pouches.First(p => p.Type == InventoryType.Items);
        pouch.Items.Length.Should().Be(120);
        pouch.MaxCount.Should().Be(99); // the hack leaves MAX_BAG_ITEM_CAPACITY alone
    }

    [Fact]
    public void ChangingAnItemCountSurvivesExport()
    {
        var game = Load();
        var berries = game.Trainer.Inventories[nameof(InventoryType.Berries)];
        var berry = berries.AllExceptNone().First();
        berries.Set((ushort)berry.Definition.Id, 42);

        var reloaded = Game.LoadFrom(game.ToByteArray(), SaveFilePath.EmeraldLegacy, new EmeraldLegacyFormat());
        reloaded.Trainer.Inventories[nameof(InventoryType.Berries)]
            .AllExceptNone().First(i => i.Definition.Id == berry.Definition.Id)
            .Count.Should().Be(42);
    }

    // Sector bytes are data below 0xFF4; 0xFF6 is the checksum the export recomputes.
    private static bool IsDataOrChecksum(int index)
    {
        var within = index & 0xFFF;
        return within < 0xFF4 || within == 0xFF6 || within == 0xFF7;
    }

    [Fact]
    public void TheDexSeenCopiesAgree()
    {
        var save = (SAV3)Load().SaveFile;
        for (ushort species = 1; species <= 386; species++)
        {
            if (!save.GetSeen(species)) continue;
            var bit = species - 1;
            var ofs = bit >> 3;
            FlagUtil.GetFlag(save.Large, save.LargeBlock.SeenOffset2 + ofs, bit & 7).Should().BeTrue();
            FlagUtil.GetFlag(save.Large, save.LargeBlock.SeenOffset3 + ofs, bit & 7).Should().BeTrue();
        }
    }
}
