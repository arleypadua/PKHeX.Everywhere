using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade.Tests;

public class ImperiumBagTests
{
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.Imperium);

    private const int SectorSize = 0x1000;
    private const int SectorData = 4084;

    private static Game Load(byte[]? bytes = null) => Game.LoadFrom(bytes ?? Fixture.ToArray(), SaveFilePath.Imperium);

    private static int SectorOf(int id) =>
        Enumerable.Range(0, 28).Single(sector => ReadUInt16LittleEndian(Fixture.AsSpan((sector * SectorSize) + 0xFF4)) == id) * SectorSize;

    private static ushort Key => (ushort)ReadUInt32LittleEndian(Fixture.AsSpan(SectorOf(0) + 0x44));

    // SaveBlock1 runs on from sector 1 to sector 4, so a pocket can end in the sector after the one it starts in.
    private static int FileOffset(int saveBlock1Offset) => SectorOf(1 + (saveBlock1Offset / SectorData)) + (saveBlock1Offset % SectorData);

    private static IEnumerable<int> PocketBytes(int offset, int slots) => Enumerable.Range(offset, slots * 4).Select(FileOffset);

    private static readonly Dictionary<string, (int Offset, int Slots)> Pockets = new()
    {
        ["Items"] = (0x560, 180),
        ["MegaStones"] = (0x830, 76),
        ["KeyItems"] = (0x960, 60),
        ["Balls"] = (0xA50, 50),
        ["TMHMs"] = (0xB18, 252),
        ["Berries"] = (0xF08, 70),
    };

    public static TheoryData<string> PocketNames => new(Pockets.Keys);

    [Fact]
    public void TheBagHasSixPockets() =>
        Load().Trainer.Inventories.InventoryTypes.Should().BeEquivalentTo(Pockets.Keys);

    [Theory]
    [InlineData("Items", "Ability Capsule", 992)]
    [InlineData("Items", "Blue Shard", 100)]
    [InlineData("MegaStones", "Banettite", 1)]
    [InlineData("KeyItems", "Shiny Charm", 1)]
    [InlineData("Balls", "Quick Ball", 72)]
    [InlineData("Balls", "Poké Ball", 100)]
    [InlineData("TMHMs", "TM021", 1)]
    [InlineData("TMHMs", "TM110", 1)]
    [InlineData("Berries", "Cheri Berry", 10)]
    [InlineData("Berries", "Roseli Berry", 10)]
    public void ReadsThePocketsItems(string pocket, string item, int count) =>
        Load().Trainer.Inventories[pocket].Items.Should().ContainSingle(owned => owned.Name == item && owned.Count == count);

    [Theory]
    [InlineData("Items", 73)]
    [InlineData("MegaStones", 32)]
    [InlineData("KeyItems", 12)]
    [InlineData("Balls", 7)]
    [InlineData("TMHMs", 68)]
    [InlineData("Berries", 27)]
    public void ReadsEveryItemInThePocket(string pocket, int count) =>
        Load().Trainer.Inventories[pocket].AllExceptNone().Should().HaveCount(count);

    [Theory]
    [InlineData("Exp. Share")]
    [InlineData("Escape Rope")]
    public void AKeyItemInImperiumThatModernGamesKeepInItemsCanBeChangedInKeyItems(string name)
    {
        var game = Load();
        var keyItems = game.Trainer.Inventories["KeyItems"];
        var item = keyItems.Items.Single(owned => owned.Name == name).Id;

        keyItems.TrySet(item, 2).Should().BeTrue();

        game.Trainer.Inventories["Items"].Supports(keyItems.Items.Single(owned => owned.Id == item).Definition).Should().BeFalse();
        game.SaveAndReload(reloaded => reloaded.Trainer.Inventories["KeyItems"].Items.Should().ContainSingle(owned => owned.Id == item && owned.Count == 2));
    }

    [Fact]
    public void AnItemOnlyImperiumHasIsUnknown() =>
        Load().Trainer.Inventories["Items"].Items.Should().ContainSingle(item => item.Name == "Unknown item #1000" && item.IsUnknown && item.Count == 10);

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
            .Concat(Enumerable.Range(0, 28).SelectMany(sector => new[] { (sector * SectorSize) + 0xFF6, (sector * SectorSize) + 0xFF7 }))
            .ToHashSet();
        Enumerable.Range(0, Fixture.Length).Where(index => exported[index] != Fixture[index])
            .Should().NotBeEmpty().And.OnlyContain(index => owned.Contains(index));
    }

    [Fact]
    public void BerriesStoredInTheNextSectorSurviveExport()
    {
        var game = Load();
        var berries = game.Trainer.Inventories["Berries"];
        while (berries.CurrentSupportedItems.FirstOrDefault() is { } berry && berries.TrySet(berry.Id, 1)) { }
        var filled = berries.AllExceptNone().Select(item => (item.Id, item.Count)).ToArray();

        filled.Should().HaveCountGreaterThan(59);
        game.SaveAndReload(reloaded =>
            reloaded.Trainer.Inventories["Berries"].AllExceptNone().Select(item => (item.Id, item.Count)).Should().Equal(filled));
    }

    [Fact]
    public void AnUnknownItemsCountChangesWhereItIs()
    {
        var game = Load();
        var items = game.Trainer.Inventories["Items"];
        var unknown = items.Items.Single(item => item.Name == "Unknown item #1000").Id;
        var before = items.Items.Select(item => item.Id).ToArray();

        items.TrySet(unknown, 5).Should().BeTrue();

        game.SaveAndReload(reloaded =>
        {
            reloaded.Trainer.Inventories["Items"].Items.Select(item => item.Id).Should().Equal(before);
            reloaded.Trainer.Inventories["Items"].Items.Single(item => item.Id == unknown).Count.Should().Be(5);
        });
    }

    [Fact]
    public void AnUnknownItemCanBeRemoved()
    {
        var game = Load();
        var megaStones = game.Trainer.Inventories["MegaStones"];
        var unknown = megaStones.Items.First(item => item.IsUnknown).Id;

        megaStones.Remove(unknown);

        game.SaveAndReload(reloaded => reloaded.Trainer.Inventories["MegaStones"].Items.Should().NotContain(item => item.Id == unknown));
    }

    [Fact]
    public void APocketHoldsUpTo999OfAnItem()
    {
        var game = Load();
        var balls = game.Trainer.Inventories["Balls"];

        balls.MaxItemCountAllowed.Should().Be(999);
        balls.TrySet(balls.Items[0].Id, 999);

        game.SaveAndReload(reloaded => reloaded.Trainer.Inventories["Balls"].Items[0].Count.Should().Be(999));
    }
}
