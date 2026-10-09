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
        offered.Legality.Messages.Should().NotContain(message => message.Contains("Language") || message.Contains("EV") || message.Contains("Encryption"));
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

        (transfer.SendRoute, transfer.ReceiveRoute).Should().Be((route, route));
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

    private static Game Blank(GameVersion version) => new(BlankSaveFile.Get(version));

    private static List<string> Moves(Pokemon pokemon) => pokemon.Moves.Values.Select(slot => slot.Move).Where(move => move.Id != 0).Select(move => move.Name).ToList();

    private static TransferOffer Send(params TransferSlot[] send) => new(send, []);

    private static TransferSlot Box(int index) => new(PokemonSource.Box, index);

    private static TransferSlot Party(int slot) => new(PokemonSource.Party, slot);
}
