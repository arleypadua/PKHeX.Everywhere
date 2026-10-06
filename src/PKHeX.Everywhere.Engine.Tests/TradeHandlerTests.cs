using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

// A preview judges legality under the destination's ParseSettings, which are global, so no other test runs meanwhile.
[CollectionDefinition(nameof(TradeHandlerTests), DisableParallelization = true)]
public class TradeCollection;

[Collection(nameof(TradeHandlerTests))]
public class TradeHandlerTests
{
    [Fact]
    public void OpenReportsThePartnerTheRoutesAndTheEmptyBoxSlots()
    {
        var session = Loaded(SaveFilePath.FireRed);

        var trade = Open(session, SaveFilePath.Emerald);

        trade["partner"]!["version"]!.GetValue<string>().Should().Be("Emerald");
        trade["partner"]!["fileName"]!.GetValue<string>().Should().Be("partner.sav");
        trade["routes"]!.ToJsonString().Should().Be("""{"send":"link","receive":"link"}""");
        trade["room"]!.ToJsonString().Should().Be("""{"mine":63,"partner":7}""");
    }

    [Fact]
    public void GetReturnsTheOpenTradeOrNull()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Value(Dispatch(session, "trade.get", "[]")).Should().BeNull();

        Open(session, SaveFilePath.Emerald);

        Value(Dispatch(session, "trade.get", "[]"))!["routes"]!["send"]!.GetValue<string>().Should().Be("link");
    }

    [Fact]
    public void PartnerBoxesListsThePartnersPartyAndBoxes()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Open(session, SaveFilePath.Emerald);

        var theirs = Value(Dispatch(session, "trade.partnerBoxes", "[]"))!.AsArray();

        theirs.Select(p => p!["species"]!.GetValue<string>()).Should().StartWith(["Torchic", "Wurmple", "Wingull", "Bulbasaur"]);
        theirs[0]!["at"]!["source"]!.GetValue<string>().Should().Be("party");
        theirs[3]!["at"]!.ToJsonString().Should().Be("""{"source":"box","slot":0,"box":0}""");
    }

    [Fact]
    public void FireRedToEmeraldIsALinkTradeThatChangesNothingButPlacement()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Open(session, SaveFilePath.Emerald);

        var preview = Preview(session, Send(PokemonHandle.InBox(0, 0)));

        preview["refused"]!.AsArray().Should().BeEmpty();
        var offer = preview["offers"]![0]!;
        offer["direction"]!.GetValue<string>().Should().Be("send");
        offer["from"]!["species"]!.GetValue<string>().Should().Be("Charizard");
        offer["from"]!["at"]!.ToJsonString().Should().Be("""{"source":"box","slot":0,"box":0}""");
        offer["arrives"]!["species"]!.GetValue<string>().Should().Be("Charizard");
        offer["arrives"]!["id"]!.GetValue<string>().Should().Be(offer["from"]!["id"]!.GetValue<string>());
        offer["arrives"]!["at"]!.ToJsonString().Should().Be(FirstEmptyBoxSlot(SaveFilePath.Emerald));
        offer["changes"]!.AsArray().Should().BeEmpty();
        offer["legality"]!["valid"]!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public void YellowToCrystalIsATimeCapsuleTrade()
    {
        var session = Loaded(SaveFilePath.Yellow);
        var trade = Open(session, SaveFilePath.Crystal);

        trade["routes"]!.ToJsonString().Should().Be("""{"send":"timeCapsule","receive":"timeCapsule"}""");
        var offer = Preview(session, Send(PokemonHandle.InBox(0, 0)))["offers"]![0]!;
        offer["arrives"]!["species"]!.GetValue<string>().Should().Be("Bulbasaur");
        offer["changes"]!.AsArray().Select(c => c!["reason"]!.GetValue<string>()).Should().OnlyContain(r => r == "timeCapsule" || r == "itemRemapped");
        offer["legality"]!["valid"]!.GetValue<bool>().Should().BeTrue(offer["legality"]!.ToJsonString());
    }

    [Fact]
    public void CrystalToYellowRefusesAGen2Species()
    {
        var session = Loaded(SaveFilePath.Crystal);
        Open(session, SaveFilePath.Yellow);

        var preview = Preview(session, Send(PokemonHandle.InBox(0, 0), PokemonHandle.InBox(0, 1)));

        preview["offers"]!.AsArray().Select(o => o!["from"]!["species"]!.GetValue<string>()).Should().Equal("Scyther");
        preview["refused"]!.ToJsonString().Should().Be("""[{"direction":"send","at":{"source":"box","slot":1,"box":0},"reason":"speciesNotInGame"}]""");
    }

    [Fact]
    public void AJapaneseGen2SaveRefusesAnInternationalGen1Pokemon()
    {
        var session = new Session();
        session.Load(new Game(new SAV2(LanguageID.Japanese, GameVersion.C)), "japanese.sav");
        Open(session, SaveFilePath.Yellow);

        var preview = Preview(session, Receive(PokemonHandle.InBox(0, 0)));

        preview["refused"]![0]!["reason"]!.GetValue<string>().Should().Be("languageMismatch");
    }

    [Fact]
    public void EmeraldToHeartGoldIsPalParkWhichRemovesHmMoves()
    {
        var session = Loaded(SaveFilePath.Emerald);
        var trade = Open(session, SaveFilePath.HgSs);

        trade["routes"]!.ToJsonString().Should().Be("""{"send":"palPark","receive":null}""");
        var offer = Preview(session, Send(PokemonHandle.InBox(0, 5)))["offers"]![0]!;
        offer["from"]!["species"]!.GetValue<string>().Should().Be("Charizard");
        var changes = offer["changes"]!.AsArray().Select(c => c!.ToJsonString()).ToList();
        changes.Should().Contain("""{"field":"moves","before":"Fly","after":null,"reason":"hmRemoved"}""");
        changes.Should().Contain(c => c.StartsWith("""{"field":"metLocation",""") && c.EndsWith(""","after":"Pal Park","reason":"palPark"}"""));
    }

    [Fact]
    public void PalParkRegistersEachNewSpeciesInThePokedex()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Open(session, SaveFilePath.HgSs);

        var offer = Preview(session, Send(PokemonHandle.InBox(0, 0)))["offers"]![0]!;

        offer["saveChanges"]!.ToJsonString().Should().Be("""[{"kind":"pokedexCaught","species":"Bulbasaur"}]""");
    }

    [Fact]
    public void HeartGoldToEmeraldHasNoRoute()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var trade = Open(session, SaveFilePath.Emerald);

        trade["routes"]!.ToJsonString().Should().Be("""{"send":null,"receive":"palPark"}""");
        Preview(session, Send(PokemonHandle.InBox(0, 0)))["refused"]![0]!["reason"]!.GetValue<string>().Should().Be("noRoute");
    }

    [Fact]
    public void AGen3EggCantMoveToGen4()
    {
        var session = new Session();
        session.Load(Game.LoadFrom(EditedEmerald.WithFirstBoxPokemon(pk => pk.IsEgg = true), "emerald.sav"), "emerald.sav");
        Open(session, SaveFilePath.HgSs);

        Preview(session, Send(PokemonHandle.InBox(0, 0)))["refused"]![0]!["reason"]!.GetValue<string>().Should().Be("eggAcrossGenerations");
    }

    [Fact]
    public void TheSendingPartyKeepsOneNonEggPokemon()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Open(session, SaveFilePath.FireRed);

        var preview = Preview(session, Send(PokemonHandle.Party(0), PokemonHandle.Party(1), PokemonHandle.Party(2)));

        preview["offers"]!.AsArray().Select(o => o!["from"]!["species"]!.GetValue<string>()).Should().Equal("Torchic", "Wurmple");
        preview["refused"]!.ToJsonString().Should().Be("""[{"direction":"send","at":{"source":"party","slot":2,"box":null},"reason":"lastPartyMember"}]""");
    }

    [Fact]
    public void AnOfferBiggerThanTheEmptySlotsIsRefused()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Open(session, SaveFilePath.Emerald);

        var preview = Preview(session, Send(Enumerable.Range(0, 8).Select(slot => PokemonHandle.InBox(0, slot)).ToArray()));

        preview["offers"]!.AsArray().Should().HaveCount(7);
        preview["offers"]!.AsArray().Select(o => o!["arrives"]!["at"]!.ToJsonString()).Should().OnlyHaveUniqueItems();
        preview["refused"]!.ToJsonString().Should().Be("""[{"direction":"send","at":{"source":"box","slot":7,"box":0},"reason":"noRoom"}]""");
    }

    [Fact]
    public void ARomHackSaveHasNoRoutes()
    {
        var session = Loaded(SaveFilePath.Unbound);

        Open(session, SaveFilePath.FireRed)["routes"]!.ToJsonString().Should().Be("""{"send":null,"receive":null}""");
    }

    [Fact]
    public void CommitMovesThePokemonFromOneSaveToTheOther()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Open(session, SaveFilePath.Emerald);
        var charizard = session.Game!.Trainer.PokemonBox.All[0];

        var done = Value(Dispatch(session, "trade.commit", Args(Send(PokemonHandle.InBox(0, 0)))))!;

        var arrived = done["arrived"]![0]!;
        arrived["direction"]!.GetValue<string>().Should().Be("send");
        arrived["at"]!.ToJsonString().Should().Be(FirstEmptyBoxSlot(SaveFilePath.Emerald));
        var mine = Game.LoadFrom(Bytes(done["save"]!), "firered.sav");
        mine.Trainer.PokemonBox.All[0].IsEmpty.Should().BeTrue();
        var partner = Game.LoadFrom(Bytes(done["partner"]!), "partner.sav");
        var landed = partner.Trainer.PokemonBox.All[Index(partner, arrived["at"]!)];
        landed.Pkm.Data.ToArray().Should().Equal(charizard.Pkm.Data.ToArray());
        landed.Legality().Valid.Should().BeTrue();
        Value(Dispatch(session, "box.get", "[]"))!.AsArray().Should().NotContain(p => p!["at"]!.ToJsonString() == """{"source":"box","slot":0,"box":0}""");
        Value(Dispatch(session, "trade.partnerBoxes", "[]"))!.AsArray().Should().Contain(p => p!["at"]!.ToJsonString() == arrived["at"]!.ToJsonString());
    }

    [Fact]
    public void CommitThroughPalParkArrivesLegalAndCaught()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Open(session, SaveFilePath.HgSs);

        var done = Value(Dispatch(session, "trade.commit", Args(Send(PokemonHandle.InBox(0, 5)))))!;

        var partner = Game.LoadFrom(Bytes(done["partner"]!), "partner.dsv");
        var landed = partner.Trainer.PokemonBox.All[Index(partner, done["arrived"]![0]!["at"]!)];
        landed.Species.Name.Should().Be("Charizard");
        landed.MetConditions.Location.Id.Should().Be(Locations.Transfer3);
        landed.Legality().Valid.Should().BeTrue();
        partner.SaveFile.GetCaught((ushort)Species.Charizard).Should().BeTrue();
        Game.LoadFrom(Bytes(done["save"]!), "emerald.sav").Trainer.PokemonBox.All[5].IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void CommitThroughTheTimeCapsuleLoadsBack()
    {
        var session = Loaded(SaveFilePath.Yellow);
        Open(session, SaveFilePath.Crystal);

        var done = Value(Dispatch(session, "trade.commit", Args(Send(PokemonHandle.InBox(0, 0)))))!;

        Game.LoadFrom(Bytes(done["save"]!), "yellow.sav").Trainer.PokemonBox.Boxed().Should().NotContain(p => p.Pokemon.Species.Name == "Bulbasaur");
        Game.LoadFrom(Bytes(done["partner"]!), "crystal.sav").Trainer.PokemonBox.Boxed().Should().Contain(p => p.Pokemon.Species.Name == "Bulbasaur");
    }

    [Fact]
    public void CommitReceivesFromThePartnerIntoTheLoadedSave()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Open(session, SaveFilePath.FireRed);

        var done = Value(Dispatch(session, "trade.commit", Args(Receive(PokemonHandle.InBox(0, 1)))))!;

        var arrived = done["arrived"]![0]!;
        arrived["direction"]!.GetValue<string>().Should().Be("receive");
        Value(Dispatch(session, "pokemon.get", Args(arrived["at"]!)))!["species"]!.GetValue<string>().Should().Be("Pikachu");
        Game.LoadFrom(Bytes(done["partner"]!), "firered.sav").Trainer.PokemonBox.All[1].IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void CommitTakesAPartyMemberOutOfTheParty()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Open(session, SaveFilePath.FireRed);

        var done = Value(Dispatch(session, "trade.commit", Args(Send(PokemonHandle.Party(0)))))!;

        Value(Dispatch(session, "party.get", "[]"))!.AsArray().Select(p => p!["species"]!.GetValue<string>()).Should().Equal("Wurmple", "Wingull");
        Game.LoadFrom(Bytes(done["save"]!), "emerald.sav").Trainer.Party.Pokemons.Select(p => p.Species.Name).Should().Equal("Wurmple", "Wingull");
    }

    [Fact]
    public void CommitKeepsADraftOfABoxSlotTheTradeLeavesAlone()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Open(session, SaveFilePath.Emerald);
        Value(Dispatch(session, "pokemon.edit", Args(PokemonHandle.InBox(0, 1))));

        Value(Dispatch(session, "trade.commit", Args(Send(PokemonHandle.InBox(0, 0)))));

        session.Draft.Should().NotBeNull();
    }

    [Fact]
    public void CommitDropsADraftOfAPartyMemberThatMovesUp()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Open(session, SaveFilePath.FireRed);
        Value(Dispatch(session, "pokemon.edit", Args(PokemonHandle.Party(1))));

        Value(Dispatch(session, "trade.commit", Args(Send(PokemonHandle.Party(0)))));

        session.Draft.Should().BeNull();
    }

    [Fact]
    public void CommitFailsWithTradeRefusedAndWritesNothing()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Open(session, SaveFilePath.Emerald);
        var before = Dispatch(session, "box.get", "[]");
        var theirsBefore = Dispatch(session, "trade.partnerBoxes", "[]");

        Error(Dispatch(session, "trade.commit", Args(Send(PokemonHandle.InBox(0, 0), PokemonHandle.Party(0))))).Should().Be("trade-refused");

        Dispatch(session, "box.get", "[]").Should().Be(before);
        Dispatch(session, "trade.partnerBoxes", "[]").Should().Be(theirsBefore);
    }

    [Fact]
    public void CloseDropsThePartner()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Open(session, SaveFilePath.Emerald);

        Value(Dispatch(session, "trade.close", "[]"));

        Error(Dispatch(session, "trade.partnerBoxes", "[]")).Should().Be("no-trade");
    }

    [Fact]
    public void PreviewFailsWithNoTradeWhenNoneIsOpen() =>
        Error(Dispatch(Loaded(SaveFilePath.FireRed), "trade.preview", Args(Send(PokemonHandle.InBox(0, 0))))).Should().Be("no-trade");

    [Fact]
    public void PreviewFailsWithNotFoundForAnEmptySlot()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Open(session, SaveFilePath.Emerald);

        Error(Dispatch(session, "trade.preview", Args(Send(PokemonHandle.Party(3))))).Should().Be("not-found");
    }

    private static JsonNode Open(Session session, string partner) =>
        Value(Dispatch(session, "trade.open", Args(Convert.ToBase64String(File.ReadAllBytes(partner)), "partner.sav", null!)))!;

    private static JsonNode Preview(Session session, object offer) => Value(Dispatch(session, "trade.preview", Args(offer)))!;

    private static object Send(params PokemonHandle[] send) => new { send = send.Select(Handle), receive = Array.Empty<object>() };

    private static object Receive(params PokemonHandle[] receive) => new { send = Array.Empty<object>(), receive = receive.Select(Handle) };

    private static object Handle(PokemonHandle at) => new { source = at.Source.ToString().ToLowerInvariant(), slot = at.Slot, box = at.Box };

    private static string FirstEmptyBoxSlot(string saveFile)
    {
        var save = SaveFilePath.Load(saveFile).SaveFile;
        var index = save.NextOpenBoxSlot();
        return $$"""{"source":"box","slot":{{index % save.BoxSlotCount}},"box":{{index / save.BoxSlotCount}}}""";
    }

    private static int Index(Game game, JsonNode at) =>
        at["box"]!.GetValue<int>() * game.SaveFile.BoxSlotCount + at["slot"]!.GetValue<int>();

    private static byte[] Bytes(JsonNode exported) => Convert.FromBase64String(exported["bytes"]!.GetValue<string>());
}
