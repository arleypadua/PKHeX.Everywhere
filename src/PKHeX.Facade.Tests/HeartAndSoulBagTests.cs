using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade.Tests;

public class HeartAndSoulBagTests
{
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.HeartAndSoul);

    private const int SectorSize = 0x1000;
    private const int SectorData = 3968;

    // The fixture's first slot has counter 100 and its second 101.
    private const int NewerSlot = 14;

    private const ushort HyperPotion = 30;

    private static Game Load(byte[]? bytes = null) => Game.LoadFrom(bytes ?? Fixture.ToArray(), SaveFilePath.HeartAndSoul);

    private static int SectorOf(int id) =>
        Enumerable.Range(NewerSlot, 14).Single(sector => ReadUInt16LittleEndian(Fixture.AsSpan((sector * SectorSize) + 0xFF4)) == id) * SectorSize;

    private static ushort Key => (ushort)ReadUInt32LittleEndian(Fixture.AsSpan(SectorOf(0) + 0xAC));

    // SaveBlock1 runs on from sector 1 to sector 4, so a pocket can end in the sector after the one it starts in.
    private static int FileOffset(int saveBlock1Offset) => SectorOf(1 + (saveBlock1Offset / SectorData)) + (saveBlock1Offset % SectorData);

    private static IEnumerable<int> PocketBytes(int offset, int slots) => Enumerable.Range(offset, slots * 4).Select(FileOffset);

    private static readonly Dictionary<string, (int Offset, int Slots)> Pockets = new()
    {
        ["Items"] = (0x564, 236),
        ["KeyItems"] = (0x914, 60),
        ["Balls"] = (0xA04, 39),
        ["TMHMs"] = (0xAA0, 160),
        ["Berries"] = (0xD20, 66),
        ["Medicine"] = (0xE28, 92),
    };

    public static TheoryData<string> PocketNames => new(Pockets.Keys);

    [Fact]
    public void TheBagHasSixPockets() =>
        Load().Trainer.Inventories.InventoryTypes.Should().BeEquivalentTo(Pockets.Keys);

    // The fixture's quantities are what the game reads, odd as some of them are.
    [Theory]
    [InlineData("Items", "Max Repel", 9985)]
    [InlineData("Items", "Escape Rope", 9429)]
    [InlineData("Items", "Lax Incense", 1)]
    [InlineData("Medicine", "Rare Candy", 911)]
    [InlineData("Medicine", "Max Potion", 1)]
    [InlineData("Balls", "Great Ball", 9835)]
    [InlineData("Balls", "Ultra Ball", 1)]
    [InlineData("TMHMs", "TM009", 9408)]
    [InlineData("TMHMs", "TM030", 1)]
    [InlineData("TMHMs", "HM05", 9408)]
    [InlineData("Berries", "Chesto Berry", 2)]
    [InlineData("Berries", "Rawst Berry", 10)]
    [InlineData("KeyItems", "Exp. Share", 4783)]
    [InlineData("KeyItems", "Squirt Bottle", 1)]
    [InlineData("KeyItems", "Good Rod", 1)]
    public void ReadsThePocketsItems(string pocket, string item, int count) =>
        Load().Trainer.Inventories[pocket].Items.Should().ContainSingle(owned => owned.Name == item && owned.Count == count);

    [Theory]
    [InlineData("Items", 8)]
    [InlineData("Medicine", 11)]
    [InlineData("Balls", 3)]
    [InlineData("TMHMs", 25)]
    [InlineData("Berries", 5)]
    [InlineData("KeyItems", 9)]
    public void ReadsEveryItemInThePocket(string pocket, int count) =>
        Load().Trainer.Inventories[pocket].AllExceptNone().Should().HaveCount(count);

    [Theory]
    [InlineData("Potion", "Medicine")]
    [InlineData("X Attack", "Medicine")]
    [InlineData("Nugget", "Items")]
    [InlineData("Escape Rope", "Items")]
    [InlineData("Helix Fossil", "KeyItems")]
    [InlineData("Exp. Share", "KeyItems")]
    [InlineData("Master Ball", "Balls")]
    [InlineData("Oran Berry", "Berries")]
    public void AnItemGoesInThePocketHeartAndSoulPutsItIn(string name, string pocket)
    {
        var game = Load();
        var item = game.ItemRepository.GetGameItemByName(name)!;

        game.Trainer.Inventories.InventoryItems.Where(inventory => inventory.Value.Supports(item)).Select(inventory => inventory.Key)
            .Should().Equal(pocket);
    }

    [Theory]
    [InlineData("Items", "Unknown item #877", 9408)]
    [InlineData("KeyItems", "Unknown item #874", 4783)]
    public void AnItemOnlyHeartAndSoulHasIsUnknown(string pocket, string item, int count) =>
        Load().Trainer.Inventories[pocket].Items.Should().ContainSingle(owned => owned.Name == item && owned.IsUnknown && owned.Count == count);

    [Fact]
    public void AnUnknownItemsCountChangesWhereItIs()
    {
        var game = Load();
        var items = game.Trainer.Inventories["Items"];
        var unknown = items.Items.Single(item => item.Name == "Unknown item #877").Id;
        var before = items.Items.Select(item => item.Id).ToArray();

        items.TrySet(unknown, 5).Should().BeTrue();

        game.SaveAndReload(reloaded =>
        {
            reloaded.Trainer.Inventories["Items"].Items.Select(item => item.Id).Should().Equal(before);
            reloaded.Trainer.Inventories["Items"].Items.Single(item => item.Id == unknown).Count.Should().Be(5);
        });
        ReadUInt16LittleEndian(game.ToByteArray().AsSpan(FileOffset(0x564 + 8))).Should().Be(877);
    }

    [Fact]
    public void AnUnknownItemCanBeRemoved()
    {
        var game = Load();
        var keyItems = game.Trainer.Inventories["KeyItems"];
        var unknown = keyItems.Items.Single(item => item.Name == "Unknown item #874").Id;

        keyItems.Remove(unknown);

        game.SaveAndReload(reloaded => reloaded.Trainer.Inventories["KeyItems"].Items.Should().NotContain(item => item.Id == unknown));
    }

    [Theory]
    [MemberData(nameof(PocketNames))]
    public void AddingChangingAndRemovingAnItemSurvivesExportAndChangesOnlyThePocket(string pocket)
    {
        var game = Load();
        var inventory = game.Trainer.Inventories[pocket];
        var changed = inventory.Items[0].Id;
        var removed = inventory.Items[1].Id;
        var added = inventory.CurrentSupportedItems.First().Id;

        inventory.TrySet(changed, 7).Should().BeTrue();
        inventory.Remove(removed);
        inventory.TrySet(added, 3).Should().BeTrue();

        game.SaveAndReload(reloaded =>
        {
            var items = reloaded.Trainer.Inventories[pocket].Items;
            items[0].Should().BeEquivalentTo(new { Id = changed, Count = 7 });
            items.Should().NotContain(item => item.Id == removed);
            items.Should().ContainSingle(item => item.Id == added && item.Count == 3);
        });

        var exported = game.ToByteArray();
        var (offset, slots) = Pockets[pocket];
        ReadUInt16LittleEndian(exported.AsSpan(FileOffset(offset) + 2)).Should().Be((ushort)(7 ^ Key));
        var owned = PocketBytes(offset, slots)
            .Concat(Enumerable.Range(NewerSlot, 14).SelectMany(sector => new[] { (sector * SectorSize) + 0xFF6, (sector * SectorSize) + 0xFF7 }))
            .ToHashSet();
        Enumerable.Range(0, Fixture.Length).Where(index => exported[index] != Fixture[index])
            .Should().NotBeEmpty().And.OnlyContain(index => owned.Contains(index));
    }

    [Fact]
    public void MedicineStoredInTheNextSectorSurvivesExport()
    {
        var lastSlots = SectorOf(2);
        var bytes = Fixture.ToArray();
        WriteUInt16LittleEndian(bytes.AsSpan(lastSlots), HyperPotion);
        WriteUInt16LittleEndian(bytes.AsSpan(lastSlots + 2), (ushort)(3 ^ Key));
        var game = Load(bytes);
        var medicine = game.Trainer.Inventories["Medicine"];
        var hyperPotion = medicine.Items.Single(item => item.Name == "Hyper Potion");
        hyperPotion.Count.Should().Be(3);

        medicine.TrySet(hyperPotion.Id, 8).Should().BeTrue();

        game.SaveAndReload(reloaded =>
            reloaded.Trainer.Inventories["Medicine"].Items.Should().ContainSingle(item => item.Name == "Hyper Potion" && item.Count == 8));
        var exported = game.ToByteArray();
        ReadUInt16LittleEndian(exported.AsSpan(lastSlots)).Should().Be(HyperPotion);
        ReadUInt16LittleEndian(exported.AsSpan(lastSlots + 2)).Should().Be((ushort)(8 ^ Key));
    }

    [Fact]
    public void APocketHoldsUpTo999OfAnItem()
    {
        var game = Load();
        var balls = game.Trainer.Inventories["Balls"];

        balls.MaxItemCountAllowed.Should().Be(999);
        balls.TrySet(balls.Items[0].Id, 1500).Should().BeTrue();

        game.SaveAndReload(reloaded => reloaded.Trainer.Inventories["Balls"].Items[0].Count.Should().Be(999));
    }
}
