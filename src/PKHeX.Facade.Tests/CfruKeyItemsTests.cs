using AwesomeAssertions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class CfruKeyItemsTests
{
    // Sector 30 sits outside both save slots and has no checksum, so the pocket's bytes are the only ones an edit changes.
    private static readonly Range Pocket = 0x1E1F0..0x1E31C;

    private static Inventory KeyItems(Game game) => game.Trainer.Inventories["KeyItems"];

    private static int[] ChangedOffsets(string path, Game game)
    {
        var fixture = File.ReadAllBytes(path);
        var exported = game.ToByteArray();
        return Enumerable.Range(0, fixture.Length).Where(offset => exported[offset] != fixture[offset]).ToArray();
    }

    [Theory]
    [InlineData(SaveFilePath.Unbound21, 38)]
    [InlineData(SaveFilePath.Unbound, 41)]
    [InlineData(SaveFilePath.RadicalRed, 21)]
    public void ReadsEveryKeyItem(string path, int count) =>
        KeyItems(SaveFilePath.Load(path)).AllExceptNone().Should().HaveCount(count);

    [Theory]
    [InlineData(SaveFilePath.Unbound21, "Shiny Charm", 1)]
    [InlineData(SaveFilePath.Unbound21, "Catching Charm", 1)]
    [InlineData(SaveFilePath.Unbound21, "Mega Cuff", 1)]
    [InlineData(SaveFilePath.Unbound21, "Unknown item #267", 2)]
    [InlineData(SaveFilePath.Unbound, "Dowsing Machine", 1)]
    [InlineData(SaveFilePath.Unbound, "N-Lunarizer", 1)]
    [InlineData(SaveFilePath.RadicalRed, "Silph Scope", 1)]
    public void ReadsKeyItemsByName(string path, string item, int count) =>
        KeyItems(SaveFilePath.Load(path)).Items.Should().ContainSingle(owned => owned.Name == item && owned.Count == count);

    private static bool InPocket(int offset) => offset >= Pocket.Start.Value && offset < Pocket.End.Value;

    // Writing Radical Red's bag rewrites a main pocket item whose index shares a modern item with another (#231), so only its pocket is checked.
    [Theory]
    [InlineData(SaveFilePath.Unbound21)]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    public void EveryKeyItemCanBeSetAgainWithoutChangingThePocket(string path)
    {
        var game = SaveFilePath.Load(path);
        var keyItems = KeyItems(game);

        foreach (var item in keyItems.AllExceptNone().ToArray())
            keyItems.TrySet(item.Id, (uint)item.Count).Should().BeTrue();

        ChangedOffsets(path, game).Where(InPocket).Should().BeEmpty();
    }

    [Theory]
    [InlineData(SaveFilePath.Unbound21)]
    [InlineData(SaveFilePath.Unbound)]
    public void AddingAndRemovingAKeyItemSurvivesExportAndChangesOnlyThePocket(string path)
    {
        var game = SaveFilePath.Load(path);
        var keyItems = KeyItems(game);
        var removed = keyItems.Items[0].Id;
        var added = keyItems.CurrentSupportedItems.First().Id;

        keyItems.Remove(removed);
        keyItems.TrySet(added, 1).Should().BeTrue();

        game.SaveAndReload(reloaded =>
        {
            var items = KeyItems(reloaded).Items;
            items.Should().NotContain(item => item.Id == removed);
            items.Should().ContainSingle(item => item.Id == added && item.Count == 1);
        });
        ChangedOffsets(path, game).Should().NotBeEmpty().And.OnlyContain(offset => InPocket(offset));
    }

    [Theory]
    [InlineData(SaveFilePath.Unbound21, "Mega Cuff")]
    [InlineData(SaveFilePath.RadicalRed, "Exp. Share")]
    [InlineData(SaveFilePath.RadicalRed, "TM Case")]
    public void AKeyItemModernGamesKeepElsewhereBelongsToKeyItems(string path, string name)
    {
        var game = SaveFilePath.Load(path);
        var item = KeyItems(game).Items.Single(owned => owned.Name == name).Definition;

        KeyItems(game).Supports(item).Should().BeTrue();
        game.Trainer.Inventories["Items"].Supports(item).Should().BeFalse();
    }
}
