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
    [InlineData(SaveFilePath.FireRed, SaveFilePath.Emerald, new[] { TransferRoute.Link }, new[] { TransferRoute.Link })]
    [InlineData(SaveFilePath.Yellow, SaveFilePath.Crystal, new[] { TransferRoute.TimeCapsule }, new[] { TransferRoute.TimeCapsule })]
    [InlineData(SaveFilePath.Emerald, SaveFilePath.HgSs, new[] { TransferRoute.PalPark }, new[] { TransferRoute.Unofficial })]
    [InlineData(SaveFilePath.Crystal, SaveFilePath.Emerald, new[] { TransferRoute.Unofficial }, new[] { TransferRoute.Unofficial })]
    [InlineData(SaveFilePath.Unbound, SaveFilePath.FireRed, null, null)]
    [InlineData(SaveFilePath.Imperium, SaveFilePath.Imperium, new[] { TransferRoute.Link }, new[] { TransferRoute.Link })]
    [InlineData(SaveFilePath.Unbound, SaveFilePath.Unbound21, new[] { TransferRoute.Link }, new[] { TransferRoute.Link })]
    [InlineData(SaveFilePath.Unbound, SaveFilePath.RadicalRed, null, null)]
    [InlineData(SaveFilePath.Imperium, SaveFilePath.Emerald, null, null)]
    [InlineData(SaveFilePath.EmeraldLegacy, SaveFilePath.Emerald, null, null)]
    [InlineData(SaveFilePath.LetsGoPikachu, SaveFilePath.LetsGoEevee, null, null)]
    public void RoutesFollowTheGames(string mine, string partner, TransferRoute[]? send, TransferRoute[]? receive)
    {
        var transfer = new Transfer(SaveFilePath.Load(mine), SaveFilePath.Load(partner));

        transfer.SendRoute.Should().BeEquivalentTo(send, options => options.WithStrictOrdering());
        transfer.ReceiveRoute.Should().BeEquivalentTo(receive, options => options.WithStrictOrdering());
    }

    [Theory]
    [InlineData(SaveFilePath.Imperium)]
    [InlineData(SaveFilePath.Unbound)]
    public void ASameHackLinkMovesThePokemonAsItLeft(string save)
    {
        var (mine, partner) = (SaveFilePath.Load(save), SaveFilePath.Load(save));
        var (index, leaving) = mine.Trainer.PokemonBox.Boxed().First();
        var bytes = leaving.Pkm.Data.ToArray();
        var transfer = new Transfer(mine, partner);

        var offered = transfer.Preview(Send(Box(index))).Offers.Single();
        var arrived = transfer.Commit(Send(Box(index))).Single();

        offered.Route.Should().Equal(TransferRoute.Link);
        offered.Changes.Should().BeEmpty();
        offered.SaveChanges.Should().BeEmpty();
        mine.Trainer.PokemonBox.All[index].IsEmpty.Should().BeTrue();
        partner.SaveAndReload(reloaded => reloaded.Trainer.PokemonBox.All[arrived.BoxIndex].Pkm.Data.ToArray().Should().Equal(bytes));
    }

    [Theory]
    [InlineData(SaveFilePath.Imperium)]
    [InlineData(SaveFilePath.Unbound)]
    public void ATradeEvolutionDoesntEvolveOverASameHackLink(string save)
    {
        var mine = SaveFilePath.Load(save);
        var (index, haunter) = mine.Trainer.PokemonBox.Boxed().First();
        haunter.Pkm.Species = (ushort)Species.Haunter;
        haunter.Pkm.HeldItem = 0;

        var arrived = new Transfer(mine, SaveFilePath.Load(save)).Commit(Send(Box(index))).Single();

        arrived.Pokemon.Species.Name.Should().Be("Haunter");
    }

    [Theory]
    [InlineData(GameVersion.E, GameVersion.B, new[] { TransferRoute.PalPark, TransferRoute.PokeTransfer })]
    [InlineData(GameVersion.Pt, GameVersion.SN, new[] { TransferRoute.PokeTransfer, TransferRoute.Transporter, TransferRoute.Bank })]
    [InlineData(GameVersion.B, GameVersion.X, new[] { TransferRoute.Transporter, TransferRoute.Bank })]
    [InlineData(GameVersion.X, GameVersion.SN, new[] { TransferRoute.Bank })]
    [InlineData(GameVersion.E, GameVersion.SN, new[] { TransferRoute.PalPark, TransferRoute.PokeTransfer, TransferRoute.Transporter, TransferRoute.Bank })]
    [InlineData(GameVersion.SN, GameVersion.X, new[] { TransferRoute.Unofficial })]
    [InlineData(GameVersion.Pt, GameVersion.R, new[] { TransferRoute.Unofficial })]
    [InlineData(GameVersion.B, GameVersion.Pt, new[] { TransferRoute.Unofficial })]
    [InlineData(GameVersion.C, GameVersion.E, new[] { TransferRoute.Unofficial })]
    [InlineData(GameVersion.RD, GameVersion.SN, new[] { TransferRoute.Transporter, TransferRoute.Bank })]
    [InlineData(GameVersion.C, GameVersion.UM, new[] { TransferRoute.Transporter, TransferRoute.Bank })]
    [InlineData(GameVersion.C, GameVersion.X, new[] { TransferRoute.Unofficial })]
    [InlineData(GameVersion.YW, GameVersion.OR, new[] { TransferRoute.Unofficial })]
    public void OfficialHopsChainForwardAndEveryOtherPairIsUnofficial(GameVersion from, GameVersion to, TransferRoute[] route) =>
        Transfer.RouteBetween(Blank(from), Blank(to)).Should().Equal(route);

    [Theory]
    [InlineData(GameVersion.X, GameVersion.OR)]
    [InlineData(GameVersion.OR, GameVersion.X)]
    [InlineData(GameVersion.SN, GameVersion.US)]
    [InlineData(GameVersion.US, GameVersion.SN)]
    public void Gen6And7SavesTradeByLinkWithinTheirGeneration(GameVersion from, GameVersion to)
    {
        var (mine, partner) = (Boxed(from, Species.Gengar), Blank(to));
        var transfer = new Transfer(mine, partner);

        var arrived = transfer.Commit(Send(Box(0))).Single();

        transfer.SendRoute.Should().Equal(TransferRoute.Link);
        transfer.ReceiveRoute.Should().Equal(TransferRoute.Link);
        arrived.Pokemon.Species.Name.Should().Be("Gengar");
        partner.Trainer.PokemonBox.All[arrived.BoxIndex].Species.Name.Should().Be("Gengar");
        mine.Trainer.PokemonBox.All[0].IsEmpty.Should().BeTrue();
    }

    [Theory]
    [InlineData(GameVersion.OR, GameVersion.X, Species.Pikachu, 1, Move.Thunderbolt, 0, TransferRefusal.SpeciesNotInGame)]
    [InlineData(GameVersion.OR, GameVersion.X, Species.Rayquaza, 0, Move.DragonAscent, 0, TransferRefusal.NotInGame)]
    [InlineData(GameVersion.OR, GameVersion.X, Species.Swampert, 0, Move.Surf, 752, TransferRefusal.NotInGame)]
    [InlineData(GameVersion.US, GameVersion.SN, Species.Poipole, 0, Move.Peck, 0, TransferRefusal.SpeciesNotInGame)]
    [InlineData(GameVersion.US, GameVersion.SN, Species.Lycanroc, 2, Move.Tackle, 0, TransferRefusal.SpeciesNotInGame)]
    [InlineData(GameVersion.US, GameVersion.SN, Species.Necrozma, 0, Move.PhotonGeyser, 0, TransferRefusal.NotInGame)]
    [InlineData(GameVersion.US, GameVersion.SN, Species.Mimikyu, 0, Move.Tackle, 924, TransferRefusal.NotInGame)]
    public void AGameRefusesWhatOnlyItsSequelHas(GameVersion from, GameVersion to, Species species, byte form, Move move, int item, TransferRefusal refusal)
    {
        var transfer = new Transfer(Boxed(from, species, form, move, item), Blank(to));

        transfer.Preview(Send(Box(0))).Refused.Should().Equal(new RefusedPokemon(TransferDirection.Send, Box(0), refusal));
        new Transfer(transfer.Mine, Blank(from)).Preview(Send(Box(0))).Refused.Should().BeEmpty();
    }

    [Fact]
    public void XRefusesARelearnMoveOnlyOmegaRubyHas()
    {
        var omegaRuby = Boxed(GameVersion.OR, Species.Rayquaza, move: Move.Fly, relearn: Move.DragonAscent);

        new Transfer(omegaRuby, Blank(GameVersion.X)).Preview(Send(Box(0))).Refused.Single().Reason.Should().Be(TransferRefusal.NotInGame);
    }

    [Fact]
    public void EachHopFromEmeraldToSunMakesItsOwnChanges()
    {
        var offered = new Transfer(Emerald(Charizard()), Blank(GameVersion.SN)).Preview(Send(Box(0))).Offers.Single();

        offered.Route.Should().Equal(TransferRoute.PalPark, TransferRoute.PokeTransfer, TransferRoute.Transporter, TransferRoute.Bank);
        offered.Arrives.Pkm.Should().BeOfType<PK7>();
        offered.Changes.Should().Contain(new TransferChange(TransferField.Moves, "Fly", null, TransferChangeReason.HmRemoved))
            .And.Contain(new TransferChange(TransferField.MetLocation, "Route 101", "Pal Park", TransferChangeReason.PalPark))
            .And.Contain(new TransferChange(TransferField.MetLocation, "Pal Park", "Poké Transfer", TransferChangeReason.PokeTransfer))
            .And.Contain(new TransferChange(TransferField.HeldItem, "Leftovers", null, TransferChangeReason.Transporter))
            .And.Contain(new TransferChange(TransferField.Nickname, "CHARIZARD", "Charizard", TransferChangeReason.Transporter));
        offered.Changes.Select(change => change.Reason).Should().BeSubsetOf([TransferChangeReason.HmRemoved, TransferChangeReason.PalPark, TransferChangeReason.PokeTransfer, TransferChangeReason.Transporter, TransferChangeReason.Bank]);
    }

    [Theory]
    [InlineData(GameVersion.RD)]
    [InlineData(GameVersion.C)]
    public void AVirtualConsolePokemonReachesSunThroughTransporterAndBankAndIsLegal(GameVersion from)
    {
        var offered = new Transfer(GameBoy(from, Species.Pikachu, Move.ThunderShock), Blank(GameVersion.SN)).Preview(Send(Box(0))).Offers.Single();

        offered.Route.Should().Equal(TransferRoute.Transporter, TransferRoute.Bank);
        offered.Arrives.Pkm.Should().BeOfType<PK7>();
        offered.Changes.Should().NotBeEmpty();
        offered.Changes.Select(change => change.Reason).Should().BeSubsetOf([TransferChangeReason.Transporter, TransferChangeReason.Bank]);
        offered.Legality!.Valid.Should().BeTrue(string.Join("; ", offered.Legality.Messages));
    }

    [Theory]
    [InlineData(GameVersion.RD, Locations.Transfer1)]
    [InlineData(GameVersion.YW, Locations.Transfer1)]
    [InlineData(GameVersion.C, Locations.Transfer2)]
    [InlineData(GameVersion.GD, Locations.Transfer2)]
    public void TransporterConvertsAVirtualConsolePokemonAsTheGameDid(GameVersion from, ushort metLocation)
    {
        var mine = GameBoy(from, Species.Pikachu, Move.ThunderShock, shiny: true);
        var leaving = mine.Trainer.PokemonBox.All[0].Pkm;

        var offered = new Transfer(mine, Blank(GameVersion.SN)).Preview(Send(Box(0))).Offers.Single();
        var arrives = offered.Arrives.Pkm;

        offered.Legality!.Valid.Should().BeTrue(string.Join("; ", offered.Legality.Messages));
        (arrives.Version, arrives.MetLocation, arrives.MetLevel, arrives.Ball).Should().Be((from, metLocation, (byte)leaving.CurrentLevel, (byte)Ball.Poke));
        arrives.Nature.Should().Be(Experience.GetNatureVC(leaving.EXP));
        arrives.AbilityNumber.Should().Be(4);
        arrives.IsShiny.Should().BeTrue();
        arrives.Language.Should().Be((int)LanguageID.English);
        arrives.OriginalTrainerName.Should().Be(leaving.OriginalTrainerName);
        new[] { arrives.IV_HP, arrives.IV_ATK, arrives.IV_DEF, arrives.IV_SPA, arrives.IV_SPD, arrives.IV_SPE }.Count(iv => iv == 31).Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void TransporterGivesMewFiveFlawlessIvsAndAFatefulEncounter()
    {
        var arrives = new Transfer(GameBoy(GameVersion.RD, Species.Mew, Move.Pound), Blank(GameVersion.SN)).Preview(Send(Box(0))).Offers.Single().Arrives.Pkm;

        new[] { arrives.IV_HP, arrives.IV_ATK, arrives.IV_DEF, arrives.IV_SPA, arrives.IV_SPD, arrives.IV_SPE }.Count(iv => iv == 31).Should().BeGreaterThanOrEqualTo(5);
        arrives.FatefulEncounter.Should().BeTrue();
    }

    [Fact]
    public void AVirtualConsolePokemonTakesTheLanguageOfItsSave()
    {
        var japanese = GameBoy(GameVersion.RD, Species.Pikachu, Move.ThunderShock, LanguageID.Japanese);
        var sun = Blank(GameVersion.SN);
        var region = (SAV7)sun.SaveFile;
        (region.ConsoleRegion, region.Country, region.Region) = (0, 1, 0);

        var offered = new Transfer(japanese, sun).Preview(Send(Box(0))).Offers.Single();

        offered.Arrives.Pkm.Language.Should().Be((int)LanguageID.Japanese);
        offered.Legality!.Valid.Should().BeTrue(string.Join("; ", offered.Legality.Messages));
    }

    [Fact]
    public void TransporterRefusesAVirtualConsolePokemonFromAnotherConsoleRegion()
    {
        var japanese = GameBoy(GameVersion.RD, Species.Pikachu, Move.ThunderShock, LanguageID.Japanese);

        new Transfer(japanese, Blank(GameVersion.SN)).Preview(Send(Box(0))).Refused.Single().Reason.Should().Be(TransferRefusal.LanguageMismatch);
    }

    [Fact]
    public void TransporterGivesTheSameVirtualConsolePokemonTheSameBytesEveryTime()
    {
        var crystal = GameBoy(GameVersion.C, Species.Pikachu, Move.ThunderShock);

        var first = new Transfer(crystal, Blank(GameVersion.SN)).Preview(Send(Box(0))).Offers.Single().Arrives;
        var again = new Transfer(crystal, Blank(GameVersion.SN)).Preview(Send(Box(0))).Offers.Single().Arrives;

        again.ToFile().Bytes.Should().Equal(first.ToFile().Bytes);
    }

    [Fact]
    public void TransporterRefusesAGen2PokemonHoldingAnItem()
    {
        var crystal = GameBoy(GameVersion.C, Species.Pikachu, Move.ThunderShock);
        crystal.Trainer.PokemonBox.All[0].Pkm.HeldItem = ItemConverter.GetItemOld2(Leftovers);

        new Transfer(crystal, Blank(GameVersion.SN)).Preview(Send(Box(0))).Refused
            .Should().Equal(new RefusedPokemon(TransferDirection.Send, Box(0), TransferRefusal.HoldingItem));
    }

    [Fact]
    public void ImportingAGen2PokemonHoldingAnItemIntoSunIsRefused()
    {
        var pikachu = Pikachu(0x1234);
        pikachu.HeldItem = ItemConverter.GetItemOld2(Leftovers);

        var import = () => Transfer.Import(new Pokemon(pikachu, Blank(GameVersion.C)), Blank(GameVersion.SN));

        import.Should().Throw<PokemonRefusedException>().Which.Reason.Should().Be(TransferRefusal.HoldingItem);
    }

    [Fact]
    public void TwoPokemonWithTheSameTrainerAndDvsArriveWithDifferentPids()
    {
        var crystal = Crystal(Pikachu(0x1234), Pikachu(0x1234));
        crystal.Trainer.PokemonBox.All[1].Pkm.EXP += 100;

        var offers = new Transfer(crystal, Blank(GameVersion.SN)).Preview(Send(Box(0), Box(1))).Offers;

        offers[1].Arrives.Pkm.PID.Should().NotBe(offers[0].Arrives.Pkm.PID);
    }

    [Fact]
    public void AStadiumSendsToSunUnofficially()
    {
        var (stadium, _) = Stadium(1, Species.Pikachu);

        Transfer.RouteBetween(stadium, Blank(GameVersion.SN)).Should().Equal(TransferRoute.Unofficial);
    }

    [Fact]
    public void TransporterRefusesAGen2Egg()
    {
        var crystal = GameBoy(GameVersion.C, Species.Pichu, Move.ThunderShock);
        crystal.Trainer.PokemonBox.All[0].Pkm.IsEgg = true;

        new Transfer(crystal, Blank(GameVersion.SN)).Preview(Send(Box(0))).Refused.Single().Reason.Should().Be(TransferRefusal.EggAcrossGenerations);
    }

    [Fact]
    public void BankReturnsTheHeldItemToTheSendersBagAndRevertsTheFormItGave()
    {
        var x = Boxed(GameVersion.X, Species.Giratina, form: 1, move: Move.ShadowForce, item: TransferEffects.GriseousOrb);
        var transfer = new Transfer(x, Blank(GameVersion.SN));

        var offered = transfer.Preview(Send(Box(0))).Offers.Single();
        var arrived = transfer.Commit(Send(Box(0))).Single().Pokemon;

        offered.Changes.Should().Contain(new TransferChange(TransferField.HeldItem, "Griseous Orb", null, TransferChangeReason.Bank))
            .And.Contain(new TransferChange(TransferField.Form, "Origin", "Altered", TransferChangeReason.Bank));
        offered.SaveChanges.Should().Contain(new TransferSaveChange(TransferSaveChangeKind.ItemReturned, TransferSide.Sender, "Griseous Orb"));
        (arrived.Pkm.HeldItem, arrived.Pkm.Form).Should().Be((0, (byte)0));
        x.Trainer.Inventories.InventoryItems.Values.SelectMany(pouch => pouch.Items).Should().Contain(item => item.Id == TransferEffects.GriseousOrb && item.Count == 1);
    }

    [Fact]
    public void TransporterReturnsTheHeldItemToTheGen5Bag()
    {
        var black = Boxed(GameVersion.B, Species.Gengar, item: Leftovers);
        var transfer = new Transfer(black, Blank(GameVersion.X));

        var offered = transfer.Preview(Send(Box(0))).Offers.Single();
        transfer.Commit(Send(Box(0)));

        offered.Changes.Should().Contain(new TransferChange(TransferField.HeldItem, "Leftovers", null, TransferChangeReason.Transporter));
        offered.SaveChanges.Should().Contain(new TransferSaveChange(TransferSaveChangeKind.ItemReturned, TransferSide.Sender, "Leftovers"));
        black.Trainer.Inventories.InventoryItems.Values.SelectMany(pouch => pouch.Items).Should().Contain(item => item.Id == Leftovers && item.Count == 1);
    }

    [Theory]
    [InlineData(GameVersion.B2, GameVersion.X, Species.Kyurem, 1, Move.FusionFlare, TransferRefusal.Fused)]
    [InlineData(GameVersion.X, GameVersion.SN, Species.Kyurem, 2, Move.FusionBolt, TransferRefusal.Fused)]
    [InlineData(GameVersion.OR, GameVersion.SN, Species.Pikachu, 1, Move.Thunderbolt, TransferRefusal.SpeciesNotInGame)]
    [InlineData(GameVersion.Pt, GameVersion.B, Species.Pichu, 1, Move.ThunderShock, TransferRefusal.SpeciesNotInGame)]
    public void PreviewRefusesWhatTransporterAndBankRefused(GameVersion from, GameVersion to, Species species, byte form, Move move, TransferRefusal refusal) =>
        new Transfer(Boxed(from, species, form, move), Blank(to)).Preview(Send(Box(0))).Refused
            .Should().Equal(new RefusedPokemon(TransferDirection.Send, Box(0), refusal));

    [Theory]
    [InlineData(GameVersion.B, GameVersion.SN)]
    [InlineData(GameVersion.X, GameVersion.SN)]
    public void TransporterAndBankRefuseEggs(GameVersion from, GameVersion to)
    {
        var mine = Boxed(from, Species.Gastly, move: Move.Lick);
        mine.Trainer.PokemonBox.All[0].Pkm.IsEgg = true;

        new Transfer(mine, Blank(to)).Preview(Send(Box(0))).Refused.Single().Reason.Should().Be(TransferRefusal.EggAcrossGenerations);
    }

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
        offered.Legality!.Valid.Should().BeTrue();
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

    public static TheoryData<GameVersion, GameVersion> Conversions()
    {
        GameVersion[] games = [GameVersion.RD, GameVersion.C, GameVersion.E, GameVersion.Pt, GameVersion.B];
        var conversions = new TheoryData<GameVersion, GameVersion>();
        foreach (var from in games)
        foreach (var to in games.Where(to => to != from))
            conversions.Add(from, to);
        return conversions;
    }

    [Theory]
    [MemberData(nameof(Conversions))]
    public void ConvertingTheSameFileTwiceGivesTheSameBytes(GameVersion from, GameVersion to)
    {
        var transfer = new Transfer(Blank(from), Blank(to));
        var file = Transfer.Import(new Pokemon(Venusaur(), Blank(GameVersion.B)), transfer.Mine).Arrives.ToFile().Bytes;

        var first = transfer.Convert(Pokemon.ReadFile(file, transfer.Mine.SaveFile.Generation), TransferSave.Partner).Arrives;
        var again = transfer.Convert(Pokemon.ReadFile(file, transfer.Mine.SaveFile.Generation), TransferSave.Partner).Arrives;

        first.Pkm.GetType().Should().Be(transfer.Partner.SaveFile.PKMType);
        again.ToFile().Bytes.Should().Equal(first.ToFile().Bytes);
    }

    [Fact]
    public void ConvertingToThePartnerGivesWhatAnUnofficialCommitPlaces()
    {
        var (platinum, ruby) = PlatinumAndRuby(new PK4 { Species = (ushort)Species.Gengar, CurrentLevel = 50, Move1 = (ushort)Move.ShadowClaw, Move2 = (ushort)Move.ShadowBall });
        var transfer = new Transfer(platinum, ruby);

        var converted = transfer.Convert(Pokemon.ReadFile(platinum.Trainer.PokemonBox.All[0].ToFile().Bytes, 4), TransferSave.Partner);
        var placed = transfer.Commit(Send(Box(0))).Single().Pokemon;

        converted.Arrives.Pkm.Should().BeOfType<PK3>();
        Moves(converted.Arrives).Should().Equal("Shadow Ball");
        converted.Arrives.ToFile().Bytes.Should().Equal(placed.ToFile().Bytes);
        converted.Details().Legality.Should().BeEquivalentTo(Transfer.LegalityIn(ruby, converted.Arrives));
    }

    [Fact]
    public void ConvertingWritesNothing()
    {
        var (platinum, ruby) = PlatinumAndRuby(Gengar());
        var transfer = new Transfer(platinum, ruby);

        transfer.Convert(Pokemon.ReadFile(platinum.Trainer.PokemonBox.All[0].ToFile().Bytes, 4), TransferSave.Partner);

        ruby.Trainer.PokemonBox.All.Should().OnlyContain(pokemon => pokemon.IsEmpty);
    }

    [Fact]
    public void AGen5FileIsntTakenForABattleRevolutionOne()
    {
        var gengar = new Pokemon(new PK5 { Species = (ushort)Species.Gengar, CurrentLevel = 50, Move1 = (ushort)Move.ShadowBall }, Blank(GameVersion.B));
        var stored = new byte[gengar.Pkm.SIZE_STORED];
        gengar.Pkm.WriteDecryptedDataStored(stored);

        Pokemon.ReadFile(stored, 5).Pkm.Should().BeOfType<PK5>();
    }

    [Theory]
    [InlineData(4)]
    [InlineData(0)]
    public void BytesThatArentAPokemonOfTheGenerationAreUnreadable(int generation)
    {
        var read = () => Pokemon.ReadFile(new Pokemon(Gengar(), Blank(GameVersion.Pt)).ToFile().Bytes[..80], generation);

        read.Should().Throw<UnreadablePokemonException>();
    }

    [Fact]
    public void AnArrivalLandsAsGivenWithItsPatch()
    {
        var black = Black(new PK5 { Species = (ushort)Species.Gengar, CurrentLevel = 50, Move1 = (ushort)Move.ShadowClaw, Move2 = (ushort)Move.ShadowBall, Ball = (byte)Ball.Dusk, MetDate = new DateOnly(2011, 3, 6) });
        var (ruby, platinum) = RubyWithGengar();
        var transfer = new Transfer(ruby, platinum);
        var bytes = new Transfer(black, platinum).Convert(Pokemon.ReadFile(black.Trainer.PokemonBox.All[0].ToFile().Bytes, 5), TransferSave.Partner).Arrives.ToFile().Bytes;
        var offer = new TransferOffer([Box(0)], [], [new ArrivalOverride(TransferDirection.Send, Box(0), bytes, new PokemonPatch(Level: 60, Nickname: "Spooky"))]);

        var offered = transfer.Preview(offer).Offers.Single();
        var placed = transfer.Commit(offer).Single().Pokemon;

        Moves(offered.Arrives).Should().Equal("Shadow Claw", "Shadow Ball");
        offered.Arrives.Ball.Name.Should().Be("Dusk Ball");
        offered.Legality.Should().BeEquivalentTo(Transfer.LegalityIn(platinum, offered.Arrives));
        offered.Changes.Should().Contain(new TransferChange(TransferField.Ball, "Poké Ball", "Dusk Ball", TransferChangeReason.PalPark));
        (placed.Level, placed.Nickname, placed.Ball.Name, placed.Pkm.MetDate).Should().Be((60, "Spooky", "Dusk Ball", new DateOnly(2011, 3, 6)));
        Moves(placed).Should().Equal("Shadow Claw", "Shadow Ball");
    }

    [Fact]
    public void AnArrivalForAPokemonNotOfferedIsRejected()
    {
        var (ruby, platinum) = RubyWithGengar();
        var bytes = new Pokemon(Gengar(), platinum).ToFile().Bytes;

        var preview = () => new Transfer(ruby, platinum).Preview(new TransferOffer([Box(0)], [], [new ArrivalOverride(TransferDirection.Send, Box(1), bytes, new PokemonPatch())]));

        preview.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AnArrivalNotInTheDestinationsFormatIsUnreadable()
    {
        var (ruby, platinum) = RubyWithGengar();
        var bytes = ruby.Trainer.PokemonBox.All[0].ToFile().Bytes;

        var preview = () => new Transfer(ruby, platinum).Preview(new TransferOffer([Box(0)], [], [new ArrivalOverride(TransferDirection.Send, Box(0), bytes, new PokemonPatch())]));

        preview.Should().Throw<UnreadablePokemonException>();
    }

    [Fact]
    public void AnArrivalsPatchFailsAsAnUpdateDoes()
    {
        var (ruby, platinum) = RubyWithGengar();
        var bytes = new Pokemon(Gengar(), platinum).ToFile().Bytes;

        var preview = () => new Transfer(ruby, platinum).Preview(new TransferOffer([Box(0)], [], [new ArrivalOverride(TransferDirection.Send, Box(0), bytes, new PokemonPatch(Level: 101))]));

        preview.Should().Throw<InvalidPatchException>();
    }

    [Theory]
    [InlineData(Species.Venusaur, 1)]
    [InlineData(Species.Gengar, 92)]
    public void TheEvolutionFamilyIsItsFirstSpecies(Species species, int family) =>
        new Pokemon(new PK4 { Species = (ushort)species, CurrentLevel = 50 }, Blank(GameVersion.Pt)).Details().EvolutionFamily.Should().Be(family);

    [Fact]
    public void APikachuIsOfPichusFamilyEvenInGen1() =>
        new Pokemon(new PK1 { Species = (ushort)Species.Pikachu, CurrentLevel = 20 }, Blank(GameVersion.RD)).EvolutionFamily.Should().Be((int)Species.Pichu);

    [Fact]
    public void APokemonMovedToGen1KeepsItsTrainerAndDefaultName()
    {
        var arrived = new Transfer(Black(Venusaur()), Blank(GameVersion.RD)).Commit(Send(Box(0))).Single().Pokemon;

        (arrived.Pkm.OriginalTrainerName, arrived.Pkm.Nickname, arrived.Pkm.Language).Should().Be(("Trainer", "VENUSAUR", (int)LanguageID.English));
    }

    [Fact]
    public void APokemonMovedUpFromGen2KeepsItsLanguageGenderAndShininessAndLosesItsStatExperience()
    {
        var (red, crystal) = (Blank(GameVersion.RD), Blank(GameVersion.C));
        var inRed = new Transfer(Black(Venusaur()), red).Commit(Send(Box(0))).Single();
        var inCrystal = new Transfer(red, crystal).Commit(Send(Box(inRed.BoxIndex))).Single();

        var offered = new Transfer(crystal, Blank(GameVersion.E)).Preview(Send(Box(inCrystal.BoxIndex))).Offers.Single();

        var arrives = offered.Arrives.Pkm;
        arrives.Language.Should().Be((int)LanguageID.English);
        arrives.EVTotal.Should().Be(0);
        arrives.PID.Should().NotBe(0);
        (arrives.Gender, arrives.IsShiny).Should().Be((inCrystal.Pokemon.Pkm.Gender, inCrystal.Pokemon.Pkm.IsShiny));
        offered.Legality!.Messages.Should().NotContain(message => message.Contains("Language") || message.Contains("EV") || message.Contains("Encryption"));
    }

    [Fact]
    public void AGen2PokemonWhoseLanguageCantBeGuessedTakesTheDestinationsLanguage()
    {
        var emerald = Blank(GameVersion.E);
        emerald.SaveFile.Language = (int)LanguageID.French;

        var arrives = new Transfer(Crystal(Pikachu(0xFAAA, "SPARKY")), emerald).Preview(Send(Box(0))).Offers.Single().Arrives.Pkm;

        (arrives.Language, arrives.Nickname).Should().Be(((int)LanguageID.French, "SPARKY"));
    }

    [Fact]
    public void AGen2PokemonGetsTheSamePidEveryTimeAndKeepsItsShininess()
    {
        var crystal = Crystal(Pikachu(0xAAAA), Pikachu(0x1234));

        var first = new Transfer(crystal, Blank(GameVersion.E)).Preview(Send(Box(0), Box(1))).Offers;
        var again = new Transfer(crystal, Blank(GameVersion.E)).Preview(Send(Box(0))).Offers.Single();

        again.Arrives.Pkm.PID.Should().Be(first[0].Arrives.Pkm.PID);
        first[0].Arrives.Pkm.IsShiny.Should().BeTrue();
        first[0].Arrives.Pkm.PID.Should().NotBe(first[1].Arrives.Pkm.PID);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void SendingFromTheMiddleOfAStadiumBoxKeepsThePokemonAfterIt(int generation)
    {
        var (mine, load) = Stadium(generation, Species.Bulbasaur, Species.Charmander, Species.Squirtle);
        new Transfer(mine, Blank(generation == 1 ? GameVersion.RD : GameVersion.C)).Commit(Send(Box(1)));

        var reloaded = new Game(load(mine.ToByteArray()));

        BoxSpecies(reloaded).Should().Equal("Bulbasaur", "Squirtle", null);
        BoxSpecies(mine).Should().Equal("Bulbasaur", "Squirtle", null);
    }

    [Theory]
    [InlineData(GameVersion.C, TransferRoute.TimeCapsule)]
    [InlineData(GameVersion.GD, TransferRoute.TimeCapsule)]
    [InlineData(GameVersion.YW, TransferRoute.TimeCapsule)]
    [InlineData(GameVersion.RD, TransferRoute.TimeCapsule)]
    [InlineData(GameVersion.E, TransferRoute.Unofficial)]
    public void Stadium2MovesPokemonToAndFromTheGameBoyGames(GameVersion partner, TransferRoute route)
    {
        var (stadium, _) = Stadium(2);
        var transfer = new Transfer(stadium, Blank(partner));

        transfer.SendRoute.Should().Equal(route);
        transfer.ReceiveRoute.Should().Equal(route);
    }

    [Fact]
    public void APokemonMovedFromCrystalToStadium2AndBackIsUnchanged()
    {
        var (stadium, _) = Stadium(2);
        var pikachu = Pikachu(0xABCD);
        (pikachu.Move2, pikachu.Move3) = ((ushort)Move.Growl, (ushort)Move.ThunderWave);
        var crystal = Crystal(pikachu);
        var sent = crystal.Trainer.PokemonBox.All[0].Pkm;

        var there = new Transfer(crystal, stadium).Commit(Send(Box(0))).Single();
        var back = new Transfer(stadium, crystal).Commit(Send(Box(there.BoxIndex))).Single().Pokemon.Pkm;

        (back.Species, back.CurrentLevel, back.Move1, back.Move2, back.Move3, back.Move4, ((PK2)back).DV16)
            .Should().Be((sent.Species, sent.CurrentLevel, sent.Move1, sent.Move2, sent.Move3, sent.Move4, (ushort)0xABCD));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void StoringInAStadiumIsntATrade(int generation)
    {
        var (stadium, _) = Stadium(generation, Species.Haunter);
        var partner = Blank(generation == 1 ? GameVersion.RD : GameVersion.C);

        new Transfer(stadium, partner).Preview(Send(Box(0))).Offers.Single().Arrives.Species.Name.Should().Be("Haunter");
        new Transfer(partner, stadium).Preview(new TransferOffer([], [Box(0)])).Offers.Single().Arrives.Species.Name.Should().Be("Haunter");
    }

    private static List<string?> BoxSpecies(Game game) => game.Trainer.PokemonBox.All.Take(3).Select(p => p.IsEmpty ? null : p.Species.Name).ToList();

    private static (Game Game, Func<Memory<byte>, SAV_STADIUM> Load) Stadium(int generation, params Species[] boxed)
    {
        Func<Memory<byte>, SAV_STADIUM> load = generation == 1
            ? data => new SAV1Stadium(data, japanese: false)
            : data => new SAV2Stadium(data, japanese: false);
        // A blank Stadium save has no box headers until it's loaded from bytes once.
        var save = load(new byte[generation == 1 ? SaveUtil.SIZE_G1STAD : SaveUtil.SIZE_G2STAD]);
        for (var i = 0; i < boxed.Length; i++)
        {
            var pokemon = save.BlankPKM;
            pokemon.Species = (ushort)boxed[i];
            pokemon.CurrentLevel = 10;
            pokemon.Move1 = (ushort)Move.Tackle;
            save.SetBoxSlotAtIndex(pokemon, i, EntityImportSettings.None);
        }
        return (new Game(save), load);
    }

    private const ushort Leftovers = 234;
    private const ushort HoennRoute101 = 16;

    private static PK3 Charizard()
    {
        var charizard = new PK3
        {
            Species = (ushort)Species.Charizard,
            CurrentLevel = 50,
            Move1 = (ushort)Move.Flamethrower,
            Move2 = (ushort)Move.Fly,
            HeldItem = ItemConverter.GetItemOld3(Leftovers),
            OriginalTrainerFriendship = 255,
            Language = (int)LanguageID.English,
            OriginalTrainerName = "Trainer",
            TID16 = 12345,
            SID16 = 54321,
            PID = 0x12345678,
            Version = GameVersion.E,
            MetLocation = HoennRoute101,
            MetLevel = 5,
            Ball = (byte)Ball.Poke,
        };
        charizard.ClearNickname();
        return charizard;
    }

    private static Game Emerald(PK3 boxed)
    {
        var emerald = BlankSaveFile.Get(GameVersion.E);
        emerald.SetBoxSlotAtIndex(boxed, 0, EntityImportSettings.None);
        return new Game(emerald);
    }

    private static PK5 Venusaur()
    {
        var venusaur = new PK5
        {
            Species = (ushort)Species.Venusaur,
            CurrentLevel = 50,
            Move1 = (ushort)Move.VineWhip,
            Language = (int)LanguageID.English,
            OriginalTrainerName = "Trainer",
            TID16 = 12345,
            SID16 = 54321,
            PID = 0x12345678,
            IV_HP = 31, IV_ATK = 31, IV_DEF = 31, IV_SPA = 31, IV_SPD = 31, IV_SPE = 31,
            EV_HP = 75, EV_SPA = 255, EV_SPE = 180,
        };
        venusaur.ClearNickname();
        return venusaur;
    }

    private static PK2 Pikachu(ushort dvs, string? nickname = null)
    {
        var pikachu = new PK2 { Species = (ushort)Species.Pikachu, CurrentLevel = 20, Move1 = (ushort)Move.ThunderShock, TID16 = 12345, OriginalTrainerName = "GOLD", DV16 = dvs };
        if (nickname is null) pikachu.SetNotNicknamed((int)LanguageID.English);
        else pikachu.Nickname = nickname;
        return pikachu;
    }

    private static Game Crystal(params PK2[] boxed)
    {
        var crystal = BlankSaveFile.Get(GameVersion.C);
        for (var i = 0; i < boxed.Length; i++) crystal.SetBoxSlotAtIndex(boxed[i], i, EntityImportSettings.None);
        return new Game(crystal);
    }

    private const ushort PlatinumRoute209 = 24;

    private static PK4 Gengar() => new()
    {
        Species = (ushort)Species.Gengar,
        CurrentLevel = 50,
        Move1 = (ushort)Move.ShadowBall,
        PID = 0x12345678,
        TID16 = 12345,
        SID16 = 54321,
        Ball = (byte)Ball.Dusk,
        Version = GameVersion.Pt,
        MetLocation = PlatinumRoute209,
        MetLevel = 30,
        MetDate = new DateOnly(2009, 1, 31),
    };

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

    private static (Game Ruby, Game Platinum) RubyWithGengar()
    {
        var ruby = BlankSaveFile.Get(GameVersion.R);
        ruby.SetBoxSlotAtIndex(new PK3 { Species = (ushort)Species.Gengar, CurrentLevel = 50, Move1 = (ushort)Move.ShadowBall, Ball = (byte)Ball.Poke, Language = (int)LanguageID.English }, 0, EntityImportSettings.None);
        return (new Game(ruby), Blank(GameVersion.Pt));
    }

    private static Game Black(PK5 boxed)
    {
        var black = BlankSaveFile.Get(GameVersion.B);
        black.SetBoxSlotAtIndex(boxed, 0, EntityImportSettings.None);
        return new Game(black);
    }

    private static Game GameBoy(GameVersion version, Species species, Move move, LanguageID language = LanguageID.English, bool shiny = false)
    {
        var save = BlankSaveFile.Get(version, language == LanguageID.Japanese ? "サトシ" : "ASH", language);
        var pokemon = save.BlankPKM;
        pokemon.Species = (ushort)species;
        pokemon.CurrentLevel = 30;
        pokemon.Move1 = (ushort)move;
        pokemon.Move1_PP = pokemon.GetMovePP(pokemon.Move1, 0);
        pokemon.OriginalTrainerName = save.OT;
        pokemon.TID16 = save.TID16;
        var gb = (GBPKM)pokemon;
        gb.DV16 = shiny ? (ushort)0xFAAA : (ushort)0x1234;
        gb.SetNotNicknamed((int)language);
        pokemon.ResetPartyStats();
        save.SetBoxSlotAtIndex(pokemon, 0, EntityImportSettings.None);
        return new Game(save);
    }

    private static Game Blank(GameVersion version) => new(BlankSaveFile.Get(version));

    private static Game Boxed(GameVersion version, Species species, byte form = 0, Move move = Move.ShadowBall, int item = 0, Move relearn = Move.None)
    {
        var save = BlankSaveFile.Get(version);
        var pokemon = save.BlankPKM;
        pokemon.Species = (ushort)species;
        pokemon.Form = form;
        pokemon.CurrentLevel = 50;
        pokemon.Move1 = (ushort)move;
        pokemon.RelearnMove1 = (ushort)relearn;
        pokemon.HeldItem = item;
        pokemon.Language = (int)LanguageID.English;
        save.SetBoxSlotAtIndex(pokemon, 0, EntityImportSettings.None);
        return new Game(save);
    }

    private static List<string> Moves(Pokemon pokemon) => pokemon.Moves.Values.Select(slot => slot.Move).Where(move => move.Id != 0).Select(move => move.Name).ToList();

    private static TransferOffer Send(params TransferSlot[] send) => new(send, []);

    private static TransferSlot Box(int index) => new(PokemonSource.Box, index);

    private static TransferSlot Party(int slot) => new(PokemonSource.Party, slot);
}
