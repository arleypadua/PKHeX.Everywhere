using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class MysteryGiftAlbumTests
{
    public static TheoryData<GameVersion, int> Albums => new()
    {
        { GameVersion.D, 3 },
        { GameVersion.Pt, 3 },
        { GameVersion.HG, 3 },
        { GameVersion.W, 12 },
        { GameVersion.B2, 12 },
    };

    [Theory]
    [MemberData(nameof(Albums))]
    public void ABlankSaveHasAnEmptyAlbum(GameVersion version, int slots)
    {
        var album = Blank(version).MysteryGifts!;

        album.SlotCount.Should().Be(slots);
        album.Slots.Should().HaveCount(slots).And.AllSatisfy(card => card.Should().BeNull());
    }

    [Theory]
    [InlineData(GameVersion.E)]
    [InlineData(GameVersion.C)]
    [InlineData(GameVersion.X)]
    public void OnlyGen4And5SavesHaveAnAlbum(GameVersion version) =>
        Blank(version).MysteryGifts.Should().BeNull();

    [Theory]
    [InlineData(GameVersion.D, 1 << (int)GameVersion.D | 1 << (int)GameVersion.P)]
    [InlineData(GameVersion.Pt, 1 << (int)GameVersion.Pt)]
    [InlineData(GameVersion.HG, 1 << (int)GameVersion.HG | 1 << (int)GameVersion.SS)]
    public void Gen4EventsAreTheCardsTheGameCanReceive(GameVersion version, int compatible)
    {
        var events = Blank(version).MysteryGifts!.Events;

        events.Should().NotBeEmpty();
        events.Should().AllSatisfy(e => (((PCD)e.Card.Gift).CardCompatibility & compatible).Should().NotBe(0));
        events.Select(e => e.Id).Should().OnlyHaveUniqueItems().And.AllSatisfy(id => id.Should().StartWith("pcd-"));
    }

    [Theory]
    [InlineData(GameVersion.W)]
    [InlineData(GameVersion.B2)]
    public void Gen5EventsAreTheCardsTheGameCanReceive(GameVersion version)
    {
        var events = Blank(version).MysteryGifts!.Events;

        events.Should().NotBeEmpty();
        events.Should().AllSatisfy(e => ((PGF)e.Card.Gift).CanBeReceivedByVersion(version).Should().BeTrue());
        events.Select(e => e.Id).Should().OnlyHaveUniqueItems().And.AllSatisfy(id => id.Should().StartWith("pgf-"));
    }

    [Fact]
    public void EventIdsComeFromTheCardsBytes()
    {
        var events = Blank(GameVersion.W).MysteryGifts!.Events;

        events.Single(e => e.Card is { CardId: 2046, Title: "Get the Liberty Pass!" }).Id.Should().Be("pgf-2046-ENG-c62055f4");
    }

    [Fact]
    public void AnEventShowsItsPokemonOrItem()
    {
        var events = Blank(GameVersion.Pt).MysteryGifts!.Events;

        var key = events.Single(e => e.Card is { CardId: 17, Language: null, Title: "Unlock the Secret!" }).Card;
        key.Kind.Should().Be(WonderCardKind.Item);
        key.Item!.Name.Should().Be(ItemRepository.GetItem(467).Name);
        key.Species.Should().BeNull();
        key.Known.Should().BeTrue();

        var pokemon = events.First(e => e.Card.Kind == WonderCardKind.Pokemon).Card;
        pokemon.Species.Should().NotBeNull();
        pokemon.Item.Should().BeNull();
        pokemon.Language.Should().NotBeNull();
    }

    [Theory]
    [InlineData(GameVersion.D)]
    [InlineData(GameVersion.Pt)]
    [InlineData(GameVersion.HG)]
    public void AGen4InsertWaitsForTheDeliveryPerson(GameVersion version)
    {
        var game = Blank(version);
        var card = FirstPokemonEvent(game);

        game.MysteryGifts!.Insert(card, 0).Should().Be(0);

        var save = (SAV4)game.SaveFile;
        var album = game.MysteryGifts;
        album.Slots[0]!.Title.Should().Be(card.Title);
        album.IsReceived(card.CardId).Should().BeTrue();
        Queued(save).Should().ContainSingle().Which.Slot.Should().Be(save is SAV4HGSS ? (byte)0 : (byte)1);
        save.Mystery.IsDeliveryManActive.Should().BeTrue();
        album.Unlocked.Should().BeFalse();
    }

    // PKHeX can't write a blank Generation 4 save, so the round trip uses the HeartGold and SoulSilver fixture.
    [Fact]
    public void AGen4InsertSurvivesReloading()
    {
        var game = Game.LoadFrom(SaveFilePath.HgSs);
        var card = FirstPokemonEvent(game);

        game.MysteryGifts!.Insert(card, 0);

        game.SaveAndReload(reloaded =>
        {
            var save = (SAV4)reloaded.SaveFile;
            reloaded.MysteryGifts!.Slots[0]!.Title.Should().Be(card.Title);
            reloaded.MysteryGifts.IsReceived(card.CardId).Should().BeTrue();
            Queued(save).Should().ContainSingle(gift => gift.Slot == 0).Which.Species.Should().Be(card.Gift.Species);
            save.Mystery.IsDeliveryManActive.Should().BeTrue();
        });
    }

    [Fact]
    public void ADiamondOrPearlInsertMarksBothSlotsActive()
    {
        var game = Blank(GameVersion.D);

        game.MysteryGifts!.Insert(FirstPokemonEvent(game), 0);

        var block = (MysteryBlock4DP)((SAV4)game.SaveFile).Mystery;
        block.GetMysteryGiftReceivedSentinel(0).Should().NotBe(0u);
        block.GetMysteryGiftReceivedSentinel(8).Should().NotBe(0u);
        block.GetMysteryGiftReceivedSentinel(9).Should().Be(0u);
    }

    [Theory]
    [InlineData(GameVersion.Pt)]
    [InlineData(GameVersion.B2)]
    public void AnInsertPastTheFilledSlotsLandsInTheFirstEmptyOne(GameVersion version)
    {
        var game = Blank(version);

        game.MysteryGifts!.Insert(FirstPokemonEvent(game), 2).Should().Be(0);

        game.MysteryGifts.Slots[0].Should().NotBeNull();
    }

    [Fact]
    public void AGen4InsertReplacesTheCardAndItsWaitingGift()
    {
        var game = Blank(GameVersion.Pt);
        var album = game.MysteryGifts!;
        var events = album.Events.Where(e => e.Card.Kind == WonderCardKind.Pokemon).Take(2).Select(e => e.Card).ToList();

        album.Insert(events[0], 0);
        album.Insert(events[1], 0);

        album.Slots[0]!.Title.Should().Be(events[1].Title);
        Queued((SAV4)game.SaveFile).Should().ContainSingle().Which.Species.Should().Be(events[1].Gift.Species);
    }

    [Fact]
    public void RemovingAGen4CardMovesTheRestUpAndDropsItsWaitingGift()
    {
        var game = Blank(GameVersion.HG);
        var album = game.MysteryGifts!;
        var events = album.Events.Where(e => e.Card.Kind == WonderCardKind.Pokemon).Take(2).Select(e => e.Card).ToList();
        album.Insert(events[0], 0);
        album.Insert(events[1], 1);

        album.Remove(0);

        var save = (SAV4)game.SaveFile;
        album.Slots.Select(card => card?.Title).Should().Equal(events[1].Title, null, null);
        Queued(save).Should().ContainSingle().Which.Slot.Should().Be(0);
        save.Mystery.IsDeliveryManActive.Should().BeTrue();
    }

    [Fact]
    public void RemovingTheLastWaitingGiftSendsTheDeliveryPersonAway()
    {
        var game = Blank(GameVersion.Pt);
        game.MysteryGifts!.Insert(FirstPokemonEvent(game), 0);

        game.MysteryGifts.Remove(0);

        ((SAV4)game.SaveFile).Mystery.IsDeliveryManActive.Should().BeFalse();
        Queued((SAV4)game.SaveFile).Should().BeEmpty();
    }

    [Theory]
    [InlineData(GameVersion.W)]
    [InlineData(GameVersion.B2)]
    public void AGen5InsertIsAnUncollectedCardAfterReloading(GameVersion version)
    {
        var game = Blank(version);
        var card = FirstPokemonEvent(game);

        game.MysteryGifts!.Insert(card, 0);

        game.SaveAndReload(reloaded =>
        {
            var album = reloaded.MysteryGifts!;
            album.Slots[0]!.Title.Should().Be(card.Title);
            album.Slots[0]!.Gift.GiftUsed.Should().BeFalse();
            album.IsReceived(card.CardId).Should().BeTrue();
        });
    }

    [Fact]
    public void RemovingAGen5CardMovesTheRestUp()
    {
        var game = Blank(GameVersion.B2);
        var album = game.MysteryGifts!;
        var events = album.Events.Take(3).Select(e => e.Card).ToList();
        for (var slot = 0; slot < 3; slot++) album.Insert(events[slot], slot);

        album.Remove(1);

        game.SaveAndReload(reloaded => reloaded.MysteryGifts!.Slots.Take(3).Select(card => card?.Title).Should().Equal(events[0].Title, events[2].Title, null));
    }

    [Fact]
    public void Gen4UnlockIsTheMenuFlag()
    {
        var game = Blank(GameVersion.Pt);

        game.MysteryGifts!.SetUnlocked(true);

        ((SAV4)game.SaveFile).IsMysteryGiftUnlocked.Should().BeTrue();
        game.MysteryGifts.Unlocked.Should().BeTrue();
    }

    [Fact]
    public void Gen5HasNoUnlock()
    {
        var album = Blank(GameVersion.B2).MysteryGifts!;

        album.Unlocked.Should().BeNull();
        album.Invoking(a => a.SetUnlocked(true)).Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void AReceivedMarkCanBeCleared()
    {
        var game = Blank(GameVersion.Pt);
        var card = FirstPokemonEvent(game);
        game.MysteryGifts!.Insert(card, 0);

        game.MysteryGifts.SetReceived(card.CardId, false);

        game.MysteryGifts.IsReceived(card.CardId).Should().BeFalse();
        game.MysteryGifts.Events.Where(e => e.Card.CardId == card.CardId).Should().AllSatisfy(e => e.Received.Should().BeFalse());
    }

    [Theory]
    [InlineData(GameVersion.Pt)]
    [InlineData(GameVersion.B2)]
    public void AFileOfADatabaseCardIsKnown(GameVersion version)
    {
        var album = Blank(version).MysteryGifts!;
        var card = album.Events[0].Card;

        var read = album.ReadFile(card.Gift.Write().ToArray());

        read.Title.Should().Be(card.Title);
        read.Known.Should().BeTrue();
    }

    [Fact]
    public void AnUnknownCardStillReadsAndInserts()
    {
        var game = Blank(GameVersion.B2);
        var pgf = (PGF)game.MysteryGifts!.Events[0].Card.Gift.Clone();
        pgf.CardTitle = "Homemade";

        var read = game.MysteryGifts.ReadFile(pgf.Data.ToArray());
        game.MysteryGifts.Insert(read, 0);

        read.Known.Should().BeFalse();
        game.MysteryGifts.Slots[0]!.Title.Should().Be("Homemade");
    }

    [Fact]
    public void APgtFileGetsABlankCard()
    {
        var game = Blank(GameVersion.HG);
        var pcd = (PCD)FirstPokemonEvent(game).Gift;

        var read = game.MysteryGifts!.ReadFile(pcd.Gift.Data.ToArray());
        game.MysteryGifts.Insert(read, 0);

        read.Kind.Should().Be(WonderCardKind.Pokemon);
        read.Species!.Id.Should().Be(pcd.Species);
        Queued((SAV4)game.SaveFile).Should().ContainSingle().Which.Slot.Should().Be(0);
    }

    [Fact]
    public void AFileForAnotherGenerationIsRefused()
    {
        var gen5 = Blank(GameVersion.B2).MysteryGifts!.Events[0].Card.Gift.Data.ToArray();

        Blank(GameVersion.Pt).MysteryGifts!.Invoking(album => album.ReadFile(gen5))
            .Should().Throw<WonderCardRefusedException>().Which.Reason.Should().Be(WonderCardRefusal.WrongGeneration);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(PGF.Size)]
    public void BytesThatArentACardAreRefused(int length) =>
        Blank(GameVersion.B2).MysteryGifts!.Invoking(album => album.ReadFile(new byte[length]))
            .Should().Throw<WonderCardRefusedException>().Which.Reason.Should().Be(WonderCardRefusal.NotACard);

    [Fact]
    public void SlotsOutsideTheAlbumAreOutOfRange()
    {
        var game = Blank(GameVersion.Pt);

        game.MysteryGifts!.Invoking(album => album.Insert(FirstPokemonEvent(game), 3)).Should().Throw<ArgumentOutOfRangeException>();
        game.MysteryGifts.Invoking(album => album.Remove(-1)).Should().Throw<ArgumentOutOfRangeException>();
        game.MysteryGifts.Invoking(album => album.SetReceived(2047, true)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void FindEventOnlyFindsEventsTheGameCanReceive()
    {
        var hgss = Blank(GameVersion.HG).MysteryGifts!.Events.Select(e => e.Id).ToHashSet();
        var platinumOnly = Blank(GameVersion.Pt).MysteryGifts!.Events.First(e => !hgss.Contains(e.Id));

        Blank(GameVersion.Pt).MysteryGifts!.FindEvent(platinumOnly.Id).Should().NotBeNull();
        Blank(GameVersion.HG).MysteryGifts!.FindEvent(platinumOnly.Id).Should().BeNull();
    }

    private static WonderCard FirstPokemonEvent(Game game) =>
        game.MysteryGifts!.Events.First(e => e.Card is { Kind: WonderCardKind.Pokemon, CardId: > 0 }).Card;

    private static List<PGT> Queued(SAV4 save) =>
        Enumerable.Range(0, 8).Select(save.Mystery.GetMysteryGiftPGT).Where(gift => !gift.IsEmpty).ToList();

    private static Game Blank(GameVersion version) =>
        Game.EmptyOf(GameVersionRepository.Instance.FindBlank((int)version)!);
}
