using AwesomeAssertions;
using PKHeX.Core;

namespace PKHeX.Facade.Tests;

public class BadgesTests
{
    [Theory]
    [InlineData(SaveFilePath.Yellow, new[] { "Boulder", "Cascade", "Thunder", "Rainbow", "Soul", "Marsh", "Volcano", "Earth" })]
    [InlineData(SaveFilePath.Crystal, new[]
    {
        "Zephyr", "Hive", "Plain", "Fog", "Storm", "Mineral", "Glacier", "Rising",
        "Boulder", "Cascade", "Thunder", "Rainbow", "Soul", "Marsh", "Volcano", "Earth",
    })]
    [InlineData(SaveFilePath.Emerald, new[] { "Stone", "Knuckle", "Dynamo", "Heat", "Balance", "Feather", "Mind", "Rain" })]
    [InlineData(SaveFilePath.FireRed, new[] { "Boulder", "Cascade", "Thunder", "Rainbow", "Soul", "Marsh", "Volcano", "Earth" })]
    public void All_NamesTheBadgesInGameOrder(string saveFile, string[] names) =>
        SaveFilePath.Load(saveFile).Badges!.All.Select(b => b.Name).Should().Equal(names);

    [Theory]
    [InlineData(SaveFilePath.Yellow)]
    [InlineData(SaveFilePath.Crystal)]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.FireRed)]
    public void All_MatchesTheBadgesPKHeXReads(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);

        game.Badges!.All.Select(b => b.Earned).Should().Equal(Bits(BadgesOf(game.SaveFile), game.Badges.All.Count));
    }

    [Theory]
    [InlineData(SaveFilePath.Yellow)]
    [InlineData(SaveFilePath.Crystal)]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.FireRed)]
    public void Set_WritesTheBadgesThroughPKHeX(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var flipped = game.Badges!.All.Select((b, i) => i % 3 == 0 ? !b.Earned : b.Earned).ToArray();

        game.Badges.Set(flipped);

        game.Badges.All.Select(b => b.Earned).Should().Equal(flipped);
        Bits(BadgesOf(game.SaveFile), flipped.Length).Should().Equal(flipped);
        game.Progress.Badges!.Earned.Should().Be(flipped.Count(e => e));
        game.SaveAndReload(reloaded => reloaded.Badges!.All.Select(b => b.Earned).Should().Equal(flipped));
    }

    [Fact]
    public void Set_RejectsTheWrongNumberOfBadges()
    {
        var badges = SaveFilePath.Load(SaveFilePath.Crystal).Badges!;

        var set = () => badges.Set(new bool[8]);

        set.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(SaveFilePath.HgSs)]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    [InlineData(SaveFilePath.Imperium)]
    public void Badges_AreNullWhenTheyCannotBeWritten(string saveFile) =>
        SaveFilePath.Load(saveFile).Badges.Should().BeNull();

    private static int BadgesOf(SaveFile save) => save switch
    {
        SAV1 gen1 => gen1.Badges,
        SAV2 gen2 => gen2.Badges,
        SAV3 gen3 => gen3.Badges,
        _ => throw new InvalidOperationException(),
    };

    private static IEnumerable<bool> Bits(int value, int count) => Enumerable.Range(0, count).Select(i => (value & (1 << i)) != 0);
}
