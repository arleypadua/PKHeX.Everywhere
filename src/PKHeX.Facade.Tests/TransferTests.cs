using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Transfers;

namespace PKHeX.Facade.Tests;

// A preview judges legality under the destination's ParseSettings, which are global, so no other test runs meanwhile.
[CollectionDefinition(nameof(TransferTests), DisableParallelization = true)]
public class TransferCollection;

[Collection(nameof(TransferTests))]
public class TransferTests
{
    [Theory]
    [InlineData(SaveFilePath.FireRed, SaveFilePath.Emerald, TransferRoute.Link, TransferRoute.Link)]
    [InlineData(SaveFilePath.Yellow, SaveFilePath.Crystal, TransferRoute.TimeCapsule, TransferRoute.TimeCapsule)]
    [InlineData(SaveFilePath.Emerald, SaveFilePath.HgSs, TransferRoute.PalPark, TransferRoute.Unofficial)]
    [InlineData(SaveFilePath.Crystal, SaveFilePath.Emerald, TransferRoute.Unofficial, TransferRoute.Unofficial)]
    [InlineData(SaveFilePath.Unbound, SaveFilePath.FireRed, null, null)]
    [InlineData(SaveFilePath.LetsGoPikachu, SaveFilePath.LetsGoEevee, null, null)]
    public void RoutesFollowTheGames(string mine, string partner, TransferRoute? send, TransferRoute? receive)
    {
        var transfer = new Transfer(SaveFilePath.Load(mine), SaveFilePath.Load(partner));

        (transfer.SendRoute, transfer.ReceiveRoute).Should().Be((send, receive));
    }

    [Theory]
    [InlineData(GameVersion.Pt, GameVersion.R, TransferRoute.Unofficial)]
    [InlineData(GameVersion.R, GameVersion.Pt, TransferRoute.PalPark)]
    [InlineData(GameVersion.B, GameVersion.Pt, TransferRoute.Unofficial)]
    [InlineData(GameVersion.C, GameVersion.E, TransferRoute.Unofficial)]
    public void EveryOtherPairTakesTheUnofficialRoute(GameVersion from, GameVersion to, TransferRoute route) =>
        Transfer.RouteBetween(Blank(from), Blank(to)).Should().Be(route);

    [Fact]
    public void RoomCountsTheEmptyBoxSlots() =>
        new Transfer(SaveFilePath.Load(SaveFilePath.FireRed), SaveFilePath.Load(SaveFilePath.Emerald)).Room.Should().Be(new TransferRoom(63, 7));

    [Fact]
    public void ALinkTradeChangesNothing()
    {
        var transfer = new Transfer(SaveFilePath.Load(SaveFilePath.FireRed), SaveFilePath.Load(SaveFilePath.Emerald));

        var offered = transfer.Preview(Send(Box(0))).Offers.Single();

        offered.Changes.Should().BeEmpty();
        offered.Arrives.Pkm.Data.ToArray().Should().Equal(offered.Pokemon.Pkm.Data.ToArray());
        offered.Legality.Valid.Should().BeTrue();
    }

    [Fact]
    public void PalParkMovesTheMetLocationAndRemovesHmMoves()
    {
        var transfer = new Transfer(SaveFilePath.Load(SaveFilePath.Emerald), SaveFilePath.Load(SaveFilePath.HgSs));

        var offered = transfer.Preview(Send(Box(5))).Offers.Single();

        offered.Changes.Should().Contain(new TransferChange(TransferField.Moves, "Fly", null, TransferChangeReason.HmRemoved))
            .And.Contain(new TransferChange(TransferField.MetLocation, "Pallet Town", "Pal Park", TransferChangeReason.PalPark));
        offered.SaveChanges.Should().ContainSingle(c => c.Kind == TransferSaveChangeKind.PokedexCaught && c.Label == "Charizard");
    }

    [Fact]
    public void TheTimeCapsuleReportsNoMetDataOrFriendshipForGen1()
    {
        var transfer = new Transfer(SaveFilePath.Load(SaveFilePath.Yellow), SaveFilePath.Load(SaveFilePath.Crystal));

        var changes = transfer.Preview(Send(Box(0))).Offers.Single().Changes;

        changes.Should().Equal(
            new TransferChange(TransferField.HeldItem, null, "Bitter Berry", TransferChangeReason.ItemRemapped),
            new TransferChange(TransferField.Friendship, null, "70", TransferChangeReason.TimeCapsule));
    }

    [Fact]
    public void AGen2SpeciesCantGoToGen1()
    {
        var transfer = new Transfer(SaveFilePath.Load(SaveFilePath.Crystal), SaveFilePath.Load(SaveFilePath.Yellow));

        transfer.Preview(Send(Box(1))).Refused.Should().Equal(new RefusedPokemon(TransferDirection.Send, Box(1), TransferRefusal.SpeciesNotInGame));
    }

    [Fact]
    public void AnEggOnlyMovesByLinkTrade()
    {
        var mine = Game.LoadFrom(EditedEmerald.WithFirstBoxPokemon(pk => pk.IsEgg = true), "emerald.sav");

        new Transfer(mine, SaveFilePath.Load(SaveFilePath.HgSs)).Preview(Send(Box(0))).Refused.Single().Reason.Should().Be(TransferRefusal.EggAcrossGenerations);
        new Transfer(mine, SaveFilePath.Load(SaveFilePath.FireRed)).Preview(Send(Box(0))).Refused.Should().BeEmpty();
    }

    [Fact]
    public void TheSendingPartyKeepsOneNonEggPokemon()
    {
        var transfer = new Transfer(SaveFilePath.Load(SaveFilePath.Emerald), SaveFilePath.Load(SaveFilePath.FireRed));

        var preview = transfer.Preview(Send(Party(0), Party(1), Party(2)));

        preview.Offers.Should().HaveCount(2);
        preview.Refused.Should().Equal(new RefusedPokemon(TransferDirection.Send, Party(2), TransferRefusal.LastPartyMember));
    }

    [Fact]
    public void PokemonBeyondTheEmptySlotsHaveNoRoom()
    {
        var transfer = new Transfer(SaveFilePath.Load(SaveFilePath.FireRed), SaveFilePath.Load(SaveFilePath.Emerald));

        var preview = transfer.Preview(Send(Enumerable.Range(0, 8).Select(Box).ToArray()));

        preview.Offers.Select(o => o.ArrivesAt).Should().OnlyHaveUniqueItems().And.HaveCount(7);
        preview.Refused.Should().Equal(new RefusedPokemon(TransferDirection.Send, Box(7), TransferRefusal.NoRoom));
    }

    [Fact]
    public void CommitMovesThePokemonBothWays()
    {
        var (mine, partner) = (SaveFilePath.Load(SaveFilePath.Emerald), SaveFilePath.Load(SaveFilePath.FireRed));
        var transfer = new Transfer(mine, partner);

        var arrived = transfer.Commit(new TransferOffer([Party(0)], [Box(1)]));

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

        var commit = () => new Transfer(mine, partner).Commit(Send(Box(0), Party(0)));

        commit.Should().Throw<TransferRefusedException>().Which.Refused.Single().Reason.Should().Be(TransferRefusal.LastPartyMember);
        mine.ToByteArray().Should().Equal(mineBefore);
        partner.ToByteArray().Should().Equal(partnerBefore);
    }

    [Fact]
    public void RotomLeavingPlatinumForgetsItsFormMove()
    {
        var rotom = new PK4 { Species = (ushort)Species.Rotom, Form = 1, CurrentLevel = 50, Move1 = (ushort)Move.Overheat, Move2 = (ushort)Move.Thunderbolt };
        var platinum = new SAV4Pt();
        platinum.SetBoxSlotAtIndex(rotom, 0, EntityImportSettings.None);

        var offered = new Transfer(new Game(platinum), SaveFilePath.Load(SaveFilePath.HgSs)).Preview(Send(Box(0))).Offers.Single();

        offered.Arrives.Form.Form.Name.Should().Be("Normal");
        offered.Arrives.Moves.Values.Select(slot => slot.Move.Name).Should().Equal("Thunderbolt", "(None)", "(None)", "(None)");
        offered.Changes.Should().Contain(new TransferChange(TransferField.Moves, "Overheat", null, TransferChangeReason.FormReverted));
    }

    [Fact]
    public void PlatinumRefusesATradeWhenItsBagCantTakeTheGriseousOrbBack()
    {
        var platinum = new SAV4Pt();
        platinum.SetBoxSlotAtIndex(new PK4 { Species = (ushort)Species.Giratina, Form = 1, HeldItem = 112, CurrentLevel = 50 }, 0, EntityImportSettings.None);
        var mine = new Game(platinum);
        var pouch = mine.Trainer.Inventories.InventoryItems.Values.Single(pouch => pouch.Supports(mine.ItemRepository.GetGameItem(112)));
        pouch.TrySet(112, (uint)pouch.MaxCountOf(112));

        new Transfer(mine, SaveFilePath.Load(SaveFilePath.HgSs)).Preview(Send(Box(0))).Refused.Single().Reason.Should().Be(TransferRefusal.BagFull);
    }

    [Fact]
    public void AnUnofficialTransferStripsTheMovesTheDestinationDoesntHave()
    {
        var gengar = new PK4 { Species = (ushort)Species.Gengar, CurrentLevel = 50, Move1 = (ushort)Move.ShadowClaw, Move2 = (ushort)Move.ShadowBall };

        var offered = PlatinumToRuby(gengar).Offers.Single();

        offered.Arrives.Pkm.Should().BeOfType<PK3>();
        Moves(offered.Arrives).Should().Equal("Shadow Ball");
        offered.Changes.Should().Contain(new TransferChange(TransferField.Moves, "Shadow Claw", null, TransferChangeReason.NotInGame));
    }

    [Fact]
    public void ABallTheDestinationDoesntHaveBecomesAPokeBall()
    {
        var gengar = new PK4 { Species = (ushort)Species.Gengar, CurrentLevel = 50, Move1 = (ushort)Move.ShadowBall, Ball = (byte)Ball.Dusk };

        var offered = PlatinumToRuby(gengar).Offers.Single();

        offered.Arrives.Ball.Name.Should().Be("Poké Ball");
        offered.Changes.Should().Contain(new TransferChange(TransferField.Ball, "Dusk Ball", "Poké Ball", TransferChangeReason.NotInGame));
    }

    [Fact]
    public void APokemonLeftWithNoMovesLearnsItsFirstLevelUpMove()
    {
        var gengar = new PK4 { Species = (ushort)Species.Gengar, CurrentLevel = 50, Move1 = (ushort)Move.ShadowClaw, Move2 = (ushort)Move.OminousWind };

        var offered = PlatinumToRuby(gengar).Offers.Single();

        Moves(offered.Arrives).Should().Equal("Hypnosis");
        offered.Changes.Should().Contain(new TransferChange(TransferField.Moves, null, "Hypnosis", TransferChangeReason.Unofficial));
    }

    [Fact]
    public void AnUnofficialTransferReportsTheOriginAndMetDate()
    {
        var gengar = new PK4 { Species = (ushort)Species.Gengar, CurrentLevel = 50, Move1 = (ushort)Move.ShadowBall, MetDate = new DateOnly(2009, 1, 1) };

        var changes = PlatinumToRuby(gengar).Offers.Single().Changes;

        changes.Should().Contain(new TransferChange(TransferField.MetDate, "2009-01-01", null, TransferChangeReason.Unofficial));
    }

    [Fact]
    public void AnUnofficialTransferStillRefusesASpeciesTheDestinationDoesntHave() =>
        PlatinumToRuby(new PK4 { Species = (ushort)Species.Lucario, CurrentLevel = 50, Move1 = (ushort)Move.AuraSphere })
            .Refused.Single().Reason.Should().Be(TransferRefusal.SpeciesNotInGame);

    [Fact]
    public void AnUnofficialTransferRefusesAnEgg() =>
        PlatinumToRuby(new PK4 { Species = (ushort)Species.Gastly, CurrentLevel = 1, Move1 = (ushort)Move.Lick, IsEgg = true })
            .Refused.Single().Reason.Should().Be(TransferRefusal.EggAcrossGenerations);

    [Fact]
    public void AnUnofficialCommitMovesThePokemon()
    {
        var (mine, partner) = PlatinumAndRuby(new PK4 { Species = (ushort)Species.Gengar, CurrentLevel = 50, Move1 = (ushort)Move.ShadowClaw, Move2 = (ushort)Move.ShadowBall });

        var arrived = new Transfer(mine, partner).Commit(Send(Box(0))).Single();

        arrived.Pokemon.Species.Name.Should().Be("Gengar");
        Moves(arrived.Pokemon).Should().Equal("Shadow Ball");
        EntityConverter.AllowIncompatibleConversion.Should().Be(EntityCompatibilitySetting.DisallowIncompatible);
    }

    [Fact]
    public void TheIncompatibleConversionSettingIsRestored()
    {
        var gengar = new PK4 { Species = (ushort)Species.Gengar, CurrentLevel = 50, Move1 = (ushort)Move.ShadowBall };
        var (mine, partner) = PlatinumAndRuby(gengar, gengar.Clone());

        new Transfer(mine, partner).Preview(Send(Box(0)));
        EntityConverter.AllowIncompatibleConversion.Should().Be(EntityCompatibilitySetting.DisallowIncompatible);

        var throwing = () => new Transfer(mine, partner).Commit(Send(Box(0), Box(1), Box(2)));
        throwing.Should().Throw<ArgumentOutOfRangeException>();
        EntityConverter.AllowIncompatibleConversion.Should().Be(EntityCompatibilitySetting.DisallowIncompatible);
    }

    [Fact]
    public void AFileWithNoOfficialRouteArrivesUnofficially()
    {
        var emerald = Blank(GameVersion.E);
        var gengar = new Pokemon(new PK4 { Species = (ushort)Species.Gengar, CurrentLevel = 50, Move1 = (ushort)Move.ShadowClaw, Move2 = (ushort)Move.ShadowBall }, Blank(GameVersion.Pt));

        var import = Transfer.Import(gengar, emerald);

        import.Unofficial.Should().BeTrue();
        import.Arrives.Pkm.Should().BeOfType<PK3>();
        import.Changes.Should().Contain(new TransferChange(TransferField.Moves, "Shadow Claw", null, TransferChangeReason.NotInGame));
        emerald.Trainer.PokemonBox.All.Should().OnlyContain(pokemon => pokemon.IsEmpty);
        EntityConverter.AllowIncompatibleConversion.Should().Be(EntityCompatibilitySetting.DisallowIncompatible);
    }

    [Fact]
    public void AFileAnOfficialConversionTakesIsntUnofficial()
    {
        var emerald = SaveFilePath.Load(SaveFilePath.Emerald);
        var hgss = SaveFilePath.Load(SaveFilePath.HgSs);

        var import = Transfer.Import(emerald.Trainer.Party.Pokemons.First(), hgss);

        import.Unofficial.Should().BeFalse();
        import.Arrives.Pkm.Should().BeOfType<PK4>();
    }

    [Fact]
    public void AFileOfASpeciesTheSaveDoesntHaveIsRefused()
    {
        var lucario = new Pokemon(new PK4 { Species = (ushort)Species.Lucario, CurrentLevel = 50, Move1 = (ushort)Move.AuraSphere }, Blank(GameVersion.Pt));

        var import = () => Transfer.Import(lucario, Blank(GameVersion.E));

        import.Should().Throw<PokemonRefusedException>().Which.Reason.Should().Be(TransferRefusal.SpeciesNotInGame);
        EntityConverter.AllowIncompatibleConversion.Should().Be(EntityCompatibilitySetting.DisallowIncompatible);
    }

    private static TransferPreview PlatinumToRuby(PK4 pokemon)
    {
        var (mine, partner) = PlatinumAndRuby(pokemon);
        return new Transfer(mine, partner).Preview(Send(Box(0)));
    }

    private static (Game Platinum, Game Ruby) PlatinumAndRuby(params PK4[] boxed)
    {
        var platinum = new SAV4Pt();
        for (var i = 0; i < boxed.Length; i++) platinum.SetBoxSlotAtIndex(boxed[i], i, EntityImportSettings.None);
        return (new Game(platinum), Blank(GameVersion.R));
    }

    private static Game Blank(GameVersion version) => new(BlankSaveFile.Get(version));

    private static List<string> Moves(Pokemon pokemon) => pokemon.Moves.Values.Select(slot => slot.Move).Where(move => move.Id != 0).Select(move => move.Name).ToList();

    private static TransferOffer Send(params TransferSlot[] send) => new(send, []);

    private static TransferSlot Box(int index) => new(PokemonSource.Box, index);

    private static TransferSlot Party(int slot) => new(PokemonSource.Party, slot);
}
