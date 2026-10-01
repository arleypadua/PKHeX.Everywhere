using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class Gen3EventsTests
{
    private const ushort SsTicket = 265;
    private const ushort EonTicket = 275;
    private const ushort MysticTicket = 370;
    private const ushort AuroraTicket = 371;
    private const ushort OldSeaMap = 376;

    [Theory]
    [InlineData(GameVersion.R, new ushort[] { SsTicket, EonTicket })]
    [InlineData(GameVersion.FR, new ushort[] { SsTicket, MysticTicket, AuroraTicket })]
    [InlineData(GameVersion.E, new ushort[] { SsTicket, EonTicket, MysticTicket, AuroraTicket, OldSeaMap })]
    public void Tickets_ShouldOnlyOfferWhatTheGameAccepts(GameVersion version, ushort[] expected)
    {
        var tickets = Blank(version).Events!.Gen3!.Tickets;

        tickets.All.Select(i => i.Id).Should().Equal(expected);
    }

    [Fact]
    public void Tickets_Emerald_ShouldGiveAllTicketsToKeyItems()
    {
        var game = Game.LoadFrom(SaveFilePath.Emerald);

        game.Events!.Gen3!.Tickets.Give(includeOldSeaMap: true);

        game.SaveAndReload(reloaded =>
        {
            reloaded.Trainer.Inventories["KeyItems"].Items.Select(i => i.Id)
                .Should().Contain([SsTicket, EonTicket, MysticTicket, AuroraTicket, OldSeaMap]);
            reloaded.Events!.Gen3!.Tickets.Missing.Should().BeEmpty();
        });
    }

    [Fact]
    public void Tickets_ShouldSkipOldSeaMapWhenNotIncluded()
    {
        var game = Game.LoadFrom(SaveFilePath.Emerald);

        var added = game.Events!.Gen3!.Tickets.Give(includeOldSeaMap: false);

        added.Select(i => i.Id).Should().NotContain(OldSeaMap);
        game.Trainer.Inventories["KeyItems"].Items.Select(i => i.Id).Should().NotContain(OldSeaMap);
    }

    [Fact]
    public void Tickets_ShouldNotDuplicateTicketsAlreadyOwned()
    {
        var game = Game.LoadFrom(SaveFilePath.Emerald);
        var tickets = game.Events!.Gen3!.Tickets;
        tickets.Give(includeOldSeaMap: true);

        var added = tickets.Give(includeOldSeaMap: true);

        added.Should().BeEmpty();
        game.Trainer.Inventories["KeyItems"].Items.Count(i => i.Id == EonTicket).Should().Be(1);
    }

    [Fact]
    public void Tickets_EnglishEmerald_ShouldRequireConfirmationForOldSeaMapUntilOwned()
    {
        var tickets = Game.LoadFrom(SaveFilePath.Emerald).Events!.Gen3!.Tickets;
        tickets.OldSeaMapNeedsConfirmation.Should().BeTrue();

        tickets.Give(includeOldSeaMap: true);

        tickets.OldSeaMapNeedsConfirmation.Should().BeFalse();
    }

    [Fact]
    public void Islands_Emerald_ShouldRoundTrip()
    {
        var game = Game.LoadFrom(SaveFilePath.Emerald);
        var islands = game.Events!.Gen3!.Islands;
        islands.Select(i => i.Name).Should().Contain(["Can ride the ferry", "Reachable: Faraway Island", "Initial event: Navel Rock"]);
        var faraway = islands.Single(i => i.Name == "Reachable: Faraway Island");
        var original = faraway.Value;

        faraway.Value = !original;

        game.SaveAndReload(reloaded =>
            reloaded.Events!.Gen3!.Islands.Single(i => i.Name == faraway.Name).Value.Should().Be(!original));
    }

    [Theory]
    [InlineData(GameVersion.R)]
    [InlineData(GameVersion.FR)]
    public void Islands_ShouldOnlyBeOfferedOnEmerald(GameVersion version)
    {
        Blank(version).Events!.Gen3!.Islands.Should().BeEmpty();
    }

    [Theory]
    [InlineData(GameVersion.C)]
    [InlineData(GameVersion.HG)]
    public void Gen3_ShouldBeUnavailableOutsideGen3(GameVersion version)
    {
        Game.LoadFrom(SaveFilePath.PathFrom(version)).Events!.Gen3.Should().BeNull();
    }

    private static Game Blank(GameVersion version) =>
        Game.EmptyOf(GameVersionRepository.Instance.Get(version), "ASH");
}
