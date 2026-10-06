using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Trades;

namespace PKHeX.Facade.Tests;

// A preview judges legality under the destination's ParseSettings, which are global, so no other test runs meanwhile.
[CollectionDefinition(nameof(TradeTests), DisableParallelization = true)]
public class TradeCollection;

[Collection(nameof(TradeTests))]
public class TradeTests
{
    [Theory]
    [InlineData(SaveFilePath.FireRed, SaveFilePath.Emerald, TradeRoute.Link, TradeRoute.Link)]
    [InlineData(SaveFilePath.Yellow, SaveFilePath.Crystal, TradeRoute.TimeCapsule, TradeRoute.TimeCapsule)]
    [InlineData(SaveFilePath.Emerald, SaveFilePath.HgSs, TradeRoute.PalPark, null)]
    [InlineData(SaveFilePath.Crystal, SaveFilePath.Emerald, null, null)]
    [InlineData(SaveFilePath.Unbound, SaveFilePath.FireRed, null, null)]
    [InlineData(SaveFilePath.LetsGoPikachu, SaveFilePath.LetsGoEevee, null, null)]
    public void RoutesFollowTheGames(string mine, string partner, TradeRoute? send, TradeRoute? receive)
    {
        var trade = new Trade(SaveFilePath.Load(mine), SaveFilePath.Load(partner));

        (trade.SendRoute, trade.ReceiveRoute).Should().Be((send, receive));
    }

    [Fact]
    public void RoomCountsTheEmptyBoxSlots() =>
        new Trade(SaveFilePath.Load(SaveFilePath.FireRed), SaveFilePath.Load(SaveFilePath.Emerald)).Room.Should().Be(new TradeRoom(63, 7));

    [Fact]
    public void ALinkTradeChangesNothing()
    {
        var trade = new Trade(SaveFilePath.Load(SaveFilePath.FireRed), SaveFilePath.Load(SaveFilePath.Emerald));

        var offered = trade.Preview(Send(Box(0))).Offers.Single();

        offered.Changes.Should().BeEmpty();
        offered.Arrives.Pkm.Data.ToArray().Should().Equal(offered.Pokemon.Pkm.Data.ToArray());
        offered.Legality.Valid.Should().BeTrue();
    }

    [Fact]
    public void PalParkMovesTheMetLocationAndRemovesHmMoves()
    {
        var trade = new Trade(SaveFilePath.Load(SaveFilePath.Emerald), SaveFilePath.Load(SaveFilePath.HgSs));

        var offered = trade.Preview(Send(Box(5))).Offers.Single();

        offered.Changes.Should().Contain(new TradeChange(TradeField.Moves, "Fly", null, TradeChangeReason.HmRemoved))
            .And.Contain(new TradeChange(TradeField.MetLocation, "Pallet Town", "Pal Park", TradeChangeReason.PalPark));
        offered.SaveChanges.Should().ContainSingle(c => c.Kind == TradeSaveChangeKind.PokedexCaught && c.Species.Name == "Charizard");
    }

    [Fact]
    public void TheTimeCapsuleReportsNoMetDataOrFriendshipForGen1()
    {
        var trade = new Trade(SaveFilePath.Load(SaveFilePath.Yellow), SaveFilePath.Load(SaveFilePath.Crystal));

        var changes = trade.Preview(Send(Box(0))).Offers.Single().Changes;

        changes.Should().Equal(
            new TradeChange(TradeField.HeldItem, null, "Bitter Berry", TradeChangeReason.ItemRemapped),
            new TradeChange(TradeField.Friendship, null, "70", TradeChangeReason.TimeCapsule));
    }

    [Fact]
    public void AGen2SpeciesCantGoToGen1()
    {
        var trade = new Trade(SaveFilePath.Load(SaveFilePath.Crystal), SaveFilePath.Load(SaveFilePath.Yellow));

        trade.Preview(Send(Box(1))).Refused.Should().Equal(new RefusedPokemon(TradeDirection.Send, Box(1), TradeRefusal.SpeciesNotInGame));
    }

    [Fact]
    public void AnEggOnlyMovesByLinkTrade()
    {
        var mine = Game.LoadFrom(EditedEmerald.WithFirstBoxPokemon(pk => pk.IsEgg = true), "emerald.sav");

        new Trade(mine, SaveFilePath.Load(SaveFilePath.HgSs)).Preview(Send(Box(0))).Refused.Single().Reason.Should().Be(TradeRefusal.EggAcrossGenerations);
        new Trade(mine, SaveFilePath.Load(SaveFilePath.FireRed)).Preview(Send(Box(0))).Refused.Should().BeEmpty();
    }

    [Fact]
    public void TheSendingPartyKeepsOneNonEggPokemon()
    {
        var trade = new Trade(SaveFilePath.Load(SaveFilePath.Emerald), SaveFilePath.Load(SaveFilePath.FireRed));

        var preview = trade.Preview(Send(Party(0), Party(1), Party(2)));

        preview.Offers.Should().HaveCount(2);
        preview.Refused.Should().Equal(new RefusedPokemon(TradeDirection.Send, Party(2), TradeRefusal.LastPartyMember));
    }

    [Fact]
    public void PokemonBeyondTheEmptySlotsHaveNoRoom()
    {
        var trade = new Trade(SaveFilePath.Load(SaveFilePath.FireRed), SaveFilePath.Load(SaveFilePath.Emerald));

        var preview = trade.Preview(Send(Enumerable.Range(0, 8).Select(Box).ToArray()));

        preview.Offers.Select(o => o.ArrivesAt).Should().OnlyHaveUniqueItems().And.HaveCount(7);
        preview.Refused.Should().Equal(new RefusedPokemon(TradeDirection.Send, Box(7), TradeRefusal.NoRoom));
    }

    [Fact]
    public void CommitMovesThePokemonBothWays()
    {
        var (mine, partner) = (SaveFilePath.Load(SaveFilePath.Emerald), SaveFilePath.Load(SaveFilePath.FireRed));
        var trade = new Trade(mine, partner);

        var arrived = trade.Commit(new TradeOffer([Party(0)], [Box(1)]));

        mine.Trainer.Party.Pokemons.Select(p => p.Species.Name).Should().Equal("Wurmple", "Wingull");
        partner.Trainer.PokemonBox.All[arrived[0].BoxIndex].Species.Name.Should().Be("Torchic");
        partner.Trainer.PokemonBox.All[1].IsEmpty.Should().BeTrue();
        mine.Trainer.PokemonBox.All[arrived[1].BoxIndex].Species.Name.Should().Be("Pikachu");
        mine.SaveFile.GetCaught((ushort)Species.Pikachu).Should().BeTrue();
    }

    [Fact]
    public void ARefusedCommitChangesNeitherSave()
    {
        var (mine, partner) = (SaveFilePath.Load(SaveFilePath.FireRed), SaveFilePath.Load(SaveFilePath.Emerald));
        var (mineBefore, partnerBefore) = (mine.ToByteArray(), partner.ToByteArray());

        var commit = () => new Trade(mine, partner).Commit(Send(Box(0), Party(0)));

        commit.Should().Throw<TradeRefusedException>().Which.Refused.Single().Reason.Should().Be(TradeRefusal.LastPartyMember);
        mine.ToByteArray().Should().Equal(mineBefore);
        partner.ToByteArray().Should().Equal(partnerBefore);
    }

    private static TradeOffer Send(params TradeSlot[] send) => new(send, []);

    private static TradeSlot Box(int index) => new(PokemonSource.Box, index);

    private static TradeSlot Party(int slot) => new(PokemonSource.Party, slot);
}
