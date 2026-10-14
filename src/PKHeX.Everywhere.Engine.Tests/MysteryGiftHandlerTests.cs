using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class MysteryGiftHandlerTests
{
    private const string HgSs = "hgss";

    public static TheoryData<string> Saves => [nameof(GameVersion.D), nameof(GameVersion.Pt), HgSs, nameof(GameVersion.B2)];

    // PKHeX can't write a blank Generation 4 save, so these are the saves that survive an export.
    public static TheoryData<string> ExportableSaves => [HgSs, nameof(GameVersion.B2)];

    [Theory]
    [MemberData(nameof(Saves))]
    public void GameGetReportsTheAlbum(string save) =>
        Value(Dispatch(Open(save), "game.get", "[]"))!["hasMysteryGift"]!.GetValue<bool>().Should().BeTrue();

    [Theory]
    [InlineData(nameof(GameVersion.D), 3, false)]
    [InlineData(nameof(GameVersion.Pt), 3, false)]
    [InlineData(nameof(GameVersion.B2), 12, null)]
    public void StatusListsEveryEmptySlotOfABlankSave(string save, int slots, bool? unlocked)
    {
        var status = Value(Dispatch(Open(save), "mysteryGift.status", "[]"))!;

        ((bool?)status["unlocked"]?.GetValue<bool>()).Should().Be(unlocked);
        status["slots"]!.AsArray().Select(s => s!["index"]!.GetValue<int>()).Should().Equal(Enumerable.Range(0, slots));
        status["slots"]!.AsArray().Should().AllSatisfy(s => s!["card"].Should().BeNull());
    }

    [Theory]
    [MemberData(nameof(Saves))]
    public void EventsListWhatTheGameCanReceive(string save)
    {
        var events = Events(Open(save));

        events.Should().NotBeEmpty();
        events.Select(e => e["id"]!.GetValue<string>()).Should().OnlyHaveUniqueItems();
        events.Select(e => e["kind"]!.GetValue<string>()).Distinct().Should().BeSubsetOf(["pokemon", "item", "other"]);
        events.Should().Contain(e => e["kind"]!.GetValue<string>() == "pokemon" && e["species"]!["name"] != null);
    }

    [Theory]
    [MemberData(nameof(Saves))]
    public void PreviewEventShowsThePokemonOrTheItem(string save)
    {
        var session = Open(save);
        var pokemon = FirstEvent(session, "pokemon");

        var pokemonPreview = Value(Dispatch(session, "mysteryGift.previewEvent", Args(Id(pokemon))))!;

        pokemonPreview["title"]!.GetValue<string>().Should().Be(pokemon["title"]!.GetValue<string>());
        pokemonPreview["pokemon"]!["speciesId"]!.GetValue<int>().Should().Be(pokemon["species"]!["id"]!.GetValue<int>());
        pokemonPreview["known"]!.GetValue<bool>().Should().BeTrue();
        pokemonPreview["item"].Should().BeNull();

        // Black 2 and White 2 can't receive any item card PKHeX has.
        if (Events(session).FirstOrDefault(e => e["kind"]!.GetValue<string>() == "item") is not { } item) return;
        var itemPreview = Value(Dispatch(session, "mysteryGift.previewEvent", Args(Id(item))))!;
        itemPreview["pokemon"].Should().BeNull();
        itemPreview["item"]!.ToJsonString().Should().Be(item["item"]!.ToJsonString());
    }

    [Theory]
    [MemberData(nameof(Saves))]
    public void PreviewFileReadsACardOfTheSavesGeneration(string save)
    {
        var session = Open(save);
        var (bytes, title) = CardFile(session.Game!.SaveFile);

        var preview = Value(Dispatch(session, "mysteryGift.previewFile", Args(Convert.ToBase64String(bytes))))!;

        preview["title"]!.GetValue<string>().Should().Be(title);
        preview["known"]!.GetValue<bool>().Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(Saves))]
    public void PreviewFileRefusesCardsOfAnotherGenerationAndBytesThatArentACard(string save)
    {
        var session = Open(save);
        var other = session.Game!.SaveFile.Generation == 4 ? EncounterEvent.MGDB_G5[0].Data.ToArray() : EncounterEvent.MGDB_G4[1].Data.ToArray();

        Error(Dispatch(session, "mysteryGift.previewFile", Args(Convert.ToBase64String(other)))).Should().Be("wrong-generation");
        Error(Dispatch(session, "mysteryGift.previewFile", Args(Convert.ToBase64String(new byte[12])))).Should().Be("unparseable");
        Error(Dispatch(session, "mysteryGift.insertFile", Args(Convert.ToBase64String(other), 0))).Should().Be("wrong-generation");
    }

    [Theory]
    [MemberData(nameof(Saves))]
    public void InsertEventPutsTheCardInTheAlbumAndMarksItReceived(string save)
    {
        var session = Open(save);
        var pokemon = FirstEvent(session, "pokemon");

        Value(Dispatch(session, "mysteryGift.insertEvent", Args(Id(pokemon), 0)))!.GetValue<int>().Should().Be(0);

        Slot(session, 0)!["title"]!.GetValue<string>().Should().Be(pokemon["title"]!.GetValue<string>());
        Slot(session, 0)!["known"]!.GetValue<bool>().Should().BeTrue();
        Events(session).Single(e => Id(e) == Id(pokemon))["received"]!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public void AnInsertedGen5CardKeepsItsLanguage()
    {
        var session = Open(nameof(GameVersion.B2));
        var meloetta = Events(session).First(e => Id(e).StartsWith("pgf-129-ENG-"));

        Value(Dispatch(session, "mysteryGift.insertEvent", Args(Id(meloetta), 0)));

        Slot(session, 0)!["language"]!.GetValue<string>().Should().Be("ENG");
    }

    [Theory]
    [MemberData(nameof(ExportableSaves))]
    public void AnInsertedCardSurvivesExportAndLoad(string save)
    {
        var session = Open(save);
        var pokemon = FirstEvent(session, "pokemon");

        Value(Dispatch(session, "mysteryGift.insertEvent", Args(Id(pokemon), 0)));

        var reloaded = ExportedAndLoaded(session);
        Slot(reloaded, 0)!["title"]!.GetValue<string>().Should().Be(pokemon["title"]!.GetValue<string>());
        Slot(reloaded, 0)!["species"]!.ToJsonString().Should().Be(pokemon["species"]!.ToJsonString());
    }

    [Theory]
    [MemberData(nameof(Saves))]
    public void InsertFilePutsTheCardInTheAlbum(string save)
    {
        var session = Open(save);
        var (bytes, title) = CardFile(session.Game!.SaveFile);

        Value(Dispatch(session, "mysteryGift.insertFile", Args(Convert.ToBase64String(bytes), 0)))!.GetValue<int>().Should().Be(0);

        Slot(session, 0)!["title"]!.GetValue<string>().Should().Be(title);
    }

    [Theory]
    [MemberData(nameof(Saves))]
    public void InsertPastTheFilledSlotsLandsInTheFirstEmptyOne(string save)
    {
        var session = Open(save);
        Value(Dispatch(session, "mysteryGift.remove", Args(0)));
        Value(Dispatch(session, "mysteryGift.remove", Args(0)));
        Value(Dispatch(session, "mysteryGift.remove", Args(0)));

        Value(Dispatch(session, "mysteryGift.insertEvent", Args(Id(FirstEvent(session, "pokemon")), 2)))!.GetValue<int>().Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(Saves))]
    public void RemoveEmptiesTheSlotAndMovesTheRestUp(string save)
    {
        var session = Open(save);
        var events = Events(session).Where(e => e["kind"]!.GetValue<string>() == "pokemon").Take(2).ToList();
        Value(Dispatch(session, "mysteryGift.insertEvent", Args(Id(events[0]), 0)));
        Value(Dispatch(session, "mysteryGift.insertEvent", Args(Id(events[1]), 1)));

        Value(Dispatch(session, "mysteryGift.remove", Args(0)));

        Slot(session, 0)!["title"]!.GetValue<string>().Should().Be(events[1]["title"]!.GetValue<string>());
    }

    [Theory]
    [InlineData(nameof(GameVersion.D))]
    [InlineData(nameof(GameVersion.Pt))]
    [InlineData(HgSs)]
    public void SetUnlockedShowsMysteryGiftInAGen4Menu(string save)
    {
        var session = Open(save);

        Value(Dispatch(session, "mysteryGift.setUnlocked", Args(true)));
        Value(Dispatch(session, "mysteryGift.status", "[]"))!["unlocked"]!.GetValue<bool>().Should().BeTrue();

        Value(Dispatch(session, "mysteryGift.setUnlocked", Args(false)));
        Value(Dispatch(session, "mysteryGift.status", "[]"))!["unlocked"]!.GetValue<bool>().Should().BeFalse();
    }

    [Fact]
    public void Gen5HasNoUnlock() =>
        Error(Dispatch(Open(nameof(GameVersion.B2)), "mysteryGift.setUnlocked", Args(true))).Should().Be("not-supported");

    [Theory]
    [MemberData(nameof(Saves))]
    public void SetReceivedClearsTheMark(string save)
    {
        var session = Open(save);
        var pokemon = FirstEvent(session, "pokemon");
        var cardId = pokemon["cardId"]!.GetValue<int>();
        Value(Dispatch(session, "mysteryGift.insertEvent", Args(Id(pokemon), 0)));

        Value(Dispatch(session, "mysteryGift.setReceived", Args(cardId, false)));

        Events(session).Where(e => e["cardId"]!.GetValue<int>() == cardId).Should().AllSatisfy(e => e["received"]!.GetValue<bool>().Should().BeFalse());
    }

    [Theory]
    [MemberData(nameof(Saves))]
    public void UnknownEventsAndSlotsFail(string save)
    {
        var session = Open(save);
        var slots = Value(Dispatch(session, "mysteryGift.status", "[]"))!["slots"]!.AsArray().Count;

        Error(Dispatch(session, "mysteryGift.previewEvent", Args("pcd-0-nope"))).Should().Be("not-found");
        Error(Dispatch(session, "mysteryGift.insertEvent", Args("pcd-0-nope", 0))).Should().Be("not-found");
        Error(Dispatch(session, "mysteryGift.insertEvent", Args(Id(FirstEvent(session, "pokemon")), slots))).Should().Be("out-of-range");
        Error(Dispatch(session, "mysteryGift.remove", Args(-1))).Should().Be("out-of-range");
        Error(Dispatch(session, "mysteryGift.setReceived", Args(4096, true))).Should().Be("out-of-range");
    }

    [Fact]
    public void ASaveWithoutAnAlbumFailsWithNotSupported()
    {
        var session = Loaded(SaveFilePath.Emerald);

        Value(Dispatch(session, "game.get", "[]"))!["hasMysteryGift"]!.GetValue<bool>().Should().BeFalse();
        Error(Dispatch(session, "mysteryGift.status", "[]")).Should().Be("not-supported");
        Error(Dispatch(session, "mysteryGift.events", "[]")).Should().Be("not-supported");
        Error(Dispatch(session, "mysteryGift.insertEvent", Args("pcd-0-nope", 0))).Should().Be("not-supported");
        Error(Dispatch(session, "mysteryGift.setUnlocked", Args(true))).Should().Be("not-supported");
    }

    [Fact]
    public void StatusFailsWithNoSaveWithoutALoadedSave() =>
        Error(Dispatch(new Session(), "mysteryGift.status", "[]")).Should().Be("no-save");

    [Fact]
    public void InsertingReportsTheMysteryGiftTopic()
    {
        var session = Open(nameof(GameVersion.Pt));
        var reported = new List<string>();
        session.Changed += reported.AddRange;

        Value(Dispatch(session, "mysteryGift.insertEvent", Args(Id(FirstEvent(session, "item")), 0)));

        reported.Should().Equal(Topics.MysteryGift);
    }

    private static Session Open(string save)
    {
        if (save == HgSs) return Loaded(SaveFilePath.HgSs);

        var session = new Session();
        Value(Dispatch(session, "game.loadBlank", Args((int)Enum.Parse<GameVersion>(save))));
        return session;
    }

    private static List<JsonNode> Events(Session session) => Value(Dispatch(session, "mysteryGift.events", "[]"))!.AsArray().Select(e => e!).ToList();

    private static JsonNode FirstEvent(Session session, string kind) =>
        Events(session).First(e => e["kind"]!.GetValue<string>() == kind && e["cardId"]!.GetValue<int>() > 0);

    private static string Id(JsonNode gift) => gift["id"]!.GetValue<string>();

    private static JsonNode? Slot(Session session, int index) => Value(Dispatch(session, "mysteryGift.status", "[]"))!["slots"]![index]!["card"];

    private static (byte[] Bytes, string Title) CardFile(SaveFile save)
    {
        DataMysteryGift card = save.Generation == 4
            ? EncounterEvent.MGDB_G4.First(c => c.GiftType == GiftType4.Item && (c.CardCompatibility & (1 << (int)GameVersion.Pt | 1 << (int)GameVersion.HG | 1 << (int)GameVersion.D)) != 0)
            : EncounterEvent.MGDB_G5.First(c => c.IsEntity);
        return (card.Data.ToArray(), card.CardTitle.Trim());
    }

    private static Session ExportedAndLoaded(Session session)
    {
        var bytes = Value(Dispatch(session, "game.export", "[]"))!["bytes"]!.GetValue<string>();
        var reloaded = new Session();
        Value(Dispatch(reloaded, "game.load", Args(bytes, "reloaded.sav", null!)));
        return reloaded;
    }
}
