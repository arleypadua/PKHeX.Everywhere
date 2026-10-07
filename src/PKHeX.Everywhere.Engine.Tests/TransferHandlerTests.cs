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
[CollectionDefinition(nameof(TransferHandlerTests), DisableParallelization = true)]
public class TransferCollection;

[Collection(nameof(TransferHandlerTests))]
public class TransferHandlerTests
{
    [Fact]
    public void OpenReportsThePartnerTheRoutesAndTheEmptyBoxSlots()
    {
        var session = Loaded(SaveFilePath.FireRed);

        var transfer = Open(session, SaveFilePath.Emerald);

        transfer["partner"]!["version"]!.GetValue<string>().Should().Be("Emerald");
        transfer["partner"]!["fileName"]!.GetValue<string>().Should().Be("partner.sav");
        transfer["routes"]!.ToJsonString().Should().Be("""{"send":"link","receive":"link"}""");
        transfer["room"]!.ToJsonString().Should().Be("""{"mine":63,"partner":7}""");
    }

    [Fact]
    public void GetReturnsTheOpenTransferOrNull()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Value(Dispatch(session, "transfer.get", "[]")).Should().BeNull();

        Open(session, SaveFilePath.Emerald);

        Value(Dispatch(session, "transfer.get", "[]"))!["routes"]!["send"]!.GetValue<string>().Should().Be("link");
    }

    [Fact]
    public void PartnerBoxesListsThePartnersPartyAndBoxes()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Open(session, SaveFilePath.Emerald);

        var theirs = Value(Dispatch(session, "transfer.partnerBoxes", "[]"))!.AsArray();

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
    public void YellowToCrystalIsATimeCapsuleTransfer()
    {
        var session = Loaded(SaveFilePath.Yellow);
        var transfer = Open(session, SaveFilePath.Crystal);

        transfer["routes"]!.ToJsonString().Should().Be("""{"send":"timeCapsule","receive":"timeCapsule"}""");
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
        var transfer = Open(session, SaveFilePath.HgSs);

        transfer["routes"]!.ToJsonString().Should().Be("""{"send":"palPark","receive":null}""");
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

        offer["saveChanges"]!.ToJsonString().Should().Be("""[{"kind":"pokedexCaught","save":"receiver","label":"Bulbasaur"}]""");
    }

    [Fact]
    public void HeartGoldToEmeraldHasNoRoute()
    {
        var session = Loaded(SaveFilePath.HgSs);
        var transfer = Open(session, SaveFilePath.Emerald);

        transfer["routes"]!.ToJsonString().Should().Be("""{"send":null,"receive":"palPark"}""");
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

        var done = Value(Dispatch(session, "transfer.commit", Args(Send(PokemonHandle.InBox(0, 0)))))!;

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
        Value(Dispatch(session, "transfer.partnerBoxes", "[]"))!.AsArray().Should().Contain(p => p!["at"]!.ToJsonString() == arrived["at"]!.ToJsonString());
    }

    [Fact]
    public void CommitThroughPalParkArrivesLegalAndCaught()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Open(session, SaveFilePath.HgSs);

        var done = Value(Dispatch(session, "transfer.commit", Args(Send(PokemonHandle.InBox(0, 5)))))!;

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

        var done = Value(Dispatch(session, "transfer.commit", Args(Send(PokemonHandle.InBox(0, 0)))))!;

        Game.LoadFrom(Bytes(done["save"]!), "yellow.sav").Trainer.PokemonBox.Boxed().Should().NotContain(p => p.Pokemon.Species.Name == "Bulbasaur");
        Game.LoadFrom(Bytes(done["partner"]!), "crystal.sav").Trainer.PokemonBox.Boxed().Should().Contain(p => p.Pokemon.Species.Name == "Bulbasaur");
    }

    [Fact]
    public void CommitReceivesFromThePartnerIntoTheLoadedSave()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Open(session, SaveFilePath.FireRed);

        var done = Value(Dispatch(session, "transfer.commit", Args(Receive(PokemonHandle.InBox(0, 1)))))!;

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

        var done = Value(Dispatch(session, "transfer.commit", Args(Send(PokemonHandle.Party(0)))))!;

        Value(Dispatch(session, "party.get", "[]"))!.AsArray().Select(p => p!["species"]!.GetValue<string>()).Should().Equal("Wurmple", "Wingull");
        Game.LoadFrom(Bytes(done["save"]!), "emerald.sav").Trainer.Party.Pokemons.Select(p => p.Species.Name).Should().Equal("Wurmple", "Wingull");
    }

    [Fact]
    public void CommitKeepsADraftOfABoxSlotTheTransferLeavesAlone()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Open(session, SaveFilePath.Emerald);
        Value(Dispatch(session, "pokemon.edit", Args(PokemonHandle.InBox(0, 1))));

        Value(Dispatch(session, "transfer.commit", Args(Send(PokemonHandle.InBox(0, 0)))));

        session.Draft.Should().NotBeNull();
    }

    [Fact]
    public void CommitDropsADraftOfAPartyMemberThatMovesUp()
    {
        var session = Loaded(SaveFilePath.Emerald);
        Open(session, SaveFilePath.FireRed);
        Value(Dispatch(session, "pokemon.edit", Args(PokemonHandle.Party(1))));

        Value(Dispatch(session, "transfer.commit", Args(Send(PokemonHandle.Party(0)))));

        session.Draft.Should().BeNull();
    }

    [Fact]
    public void CommitFailsWithTransferRefusedAndWritesNothing()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Open(session, SaveFilePath.Emerald);
        var before = Dispatch(session, "box.get", "[]");
        var theirsBefore = Dispatch(session, "transfer.partnerBoxes", "[]");

        Error(Dispatch(session, "transfer.commit", Args(Send(PokemonHandle.InBox(0, 0), PokemonHandle.Party(0))))).Should().Be("transfer-refused");

        Dispatch(session, "box.get", "[]").Should().Be(before);
        Dispatch(session, "transfer.partnerBoxes", "[]").Should().Be(theirsBefore);
    }

    [Fact]
    public void CloseDropsThePartner()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Open(session, SaveFilePath.Emerald);

        Value(Dispatch(session, "transfer.close", "[]"));

        Error(Dispatch(session, "transfer.partnerBoxes", "[]")).Should().Be("no-transfer");
    }

    [Fact]
    public void PreviewFailsWithNoTransferWhenNoneIsOpen() =>
        Error(Dispatch(Loaded(SaveFilePath.FireRed), "transfer.preview", Args(Send(PokemonHandle.InBox(0, 0))))).Should().Be("no-transfer");

    [Fact]
    public void PreviewFailsWithNotFoundForAnEmptySlot()
    {
        var session = Loaded(SaveFilePath.FireRed);
        Open(session, SaveFilePath.Emerald);

        Error(Dispatch(session, "transfer.preview", Args(Send(PokemonHandle.Party(3))))).Should().Be("not-found");
    }

    [Fact]
    public void KadabraTradedOverALinkArrivesAsAlakazam()
    {
        var session = LoadedWith(SaveFilePath.Emerald, pk => pk.Species = (ushort)Species.Kadabra);
        Open(session, SaveFilePath.FireRed);

        var offer = Preview(session, Send(PokemonHandle.InBox(0, 0)))["offers"]![0]!;

        offer["arrives"]!["species"]!.GetValue<string>().Should().Be("Alakazam");
        offer["arrives"]!["nickname"]!.GetValue<string>().Should().Be("ALAKAZAM");
        offer["changes"]!.AsArray().Select(c => c!.ToJsonString()).Should().Contain(
            """{"field":"species","before":"Kadabra","after":"Alakazam","reason":"tradeEvolution"}""",
            """{"field":"nickname","before":"KADABRA","after":"ALAKAZAM","reason":"tradeEvolution"}""");
    }

    [Fact]
    public void OnixHoldingAMetalCoatArrivesInGen2AsSteelixWithoutTheItem()
    {
        var session = LoadedWith(SaveFilePath.Crystal, pk => (pk.Species, pk.HeldItem) = ((ushort)Species.Onix, Gen2MetalCoat));
        Open(session, SaveFilePath.Crystal);

        var done = Value(Dispatch(session, "transfer.commit", Args(Send(PokemonHandle.InBox(0, 0)))))!;

        var partner = Game.LoadFrom(Bytes(done["partner"]!), "crystal.sav");
        var landed = partner.Trainer.PokemonBox.All[Index(partner, done["arrived"]![0]!["at"]!)];
        landed.Species.Name.Should().Be("Steelix");
        landed.Pkm.HeldItem.Should().Be(0);
    }

    [Fact]
    public void TheItemATradeEvolutionUsesUpIsListed()
    {
        var session = LoadedWith(SaveFilePath.Crystal, pk => (pk.Species, pk.HeldItem) = ((ushort)Species.Onix, Gen2MetalCoat));
        Open(session, SaveFilePath.Crystal);

        var changes = Preview(session, Send(PokemonHandle.InBox(0, 0)))["offers"]![0]!["changes"]!.AsArray().Select(c => c!.ToJsonString());

        changes.Should().Contain("""{"field":"heldItem","before":"Metal Coat","after":null,"reason":"itemUsed"}""");
    }

    [Theory]
    [InlineData(SaveFilePath.Crystal, Species.Kadabra, "Kadabra")]
    [InlineData(SaveFilePath.Crystal, Species.Machoke, "Machoke")]
    [InlineData(SaveFilePath.Emerald, Species.Machoke, "Machoke")]
    [InlineData(SaveFilePath.HgSs, Species.Machoke, "Machoke")]
    [InlineData(SaveFilePath.HgSs, Species.Kadabra, "Alakazam")]
    public void AnEverstoneStopsATradeEvolutionButKadabrasInGen4(string save, Species species, string arrives)
    {
        var session = LoadedWith(save, pk => (pk.Species, pk.HeldItem) = ((ushort)species, EverstoneIn(pk)));
        Open(session, save);

        Preview(session, Send(PokemonHandle.InBox(0, 0)))["offers"]![0]!["arrives"]!["species"]!.GetValue<string>().Should().Be(arrives);
    }

    [Fact]
    public void AnItemTradeEvolutionDoesntHappenThroughTheTimeCapsule()
    {
        var session = LoadedWith(SaveFilePath.Yellow, pk => (pk.Species, ((PK1)pk).CatchRate) = ((ushort)Species.Onix, Gen2MetalCoat));
        Open(session, SaveFilePath.Crystal);

        var offer = Preview(session, Send(PokemonHandle.InBox(0, 0)))["offers"]![0]!;

        offer["arrives"]!["species"]!.GetValue<string>().Should().Be("Onix");
        offer["changes"]!.AsArray().Select(c => c!.ToJsonString()).Should().Contain("""{"field":"heldItem","before":null,"after":"Metal Coat","reason":"itemRemapped"}""");
    }

    [Fact]
    public void APlainTradeEvolutionHappensThroughTheTimeCapsule()
    {
        var session = LoadedWith(SaveFilePath.Yellow, pk => pk.Species = (ushort)Species.Haunter);
        Open(session, SaveFilePath.Crystal);

        Preview(session, Send(PokemonHandle.InBox(0, 0)))["offers"]![0]!["arrives"]!["species"]!.GetValue<string>().Should().Be("Gengar");
    }

    [Fact]
    public void AGen3LinkTradeSetsFriendshipTo70()
    {
        var session = LoadedWith(SaveFilePath.Emerald, pk => pk.CurrentFriendship = 255);
        Open(session, SaveFilePath.FireRed);

        var done = Value(Dispatch(session, "transfer.commit", Args(Send(PokemonHandle.InBox(0, 0)))))!;

        var partner = Game.LoadFrom(Bytes(done["partner"]!), "firered.sav");
        partner.Trainer.PokemonBox.All[Index(partner, done["arrived"]![0]!["at"]!)].Friendship.Should().Be(70);
    }

    [Fact]
    public void TheNewFriendshipIsListedAsReceived()
    {
        var session = LoadedWith(SaveFilePath.Emerald, pk => pk.CurrentFriendship = 255);
        Open(session, SaveFilePath.FireRed);

        Preview(session, Send(PokemonHandle.InBox(0, 0)))["offers"]![0]!["changes"]!.ToJsonString()
            .Should().Be("""[{"field":"friendship","before":"255","after":"70","reason":"received"}]""");
    }

    [Fact]
    public void AnEggKeepsItsHatchCounter()
    {
        var session = LoadedWith(SaveFilePath.Emerald, pk => (pk.IsEgg, pk.CurrentFriendship) = (true, 5));
        Open(session, SaveFilePath.FireRed);

        Preview(session, Send(PokemonHandle.InBox(0, 0)))["offers"]![0]!["changes"]!.AsArray().Should().BeEmpty();
    }

    [Fact]
    public void GiratinaOriginLeavingPlatinumArrivesAlteredAndTheOrbGoesToTheSendersBag()
    {
        var session = new Session();
        session.Load(new Game(Platinum(new PK4 { Species = (ushort)Species.Giratina, Form = 1, HeldItem = GriseousOrb, CurrentLevel = 50, Ability = (int)PKHeX.Core.Ability.Levitate })), "platinum.sav");
        Open(session, SaveFilePath.HgSs);

        var offer = Preview(session, Send(PokemonHandle.InBox(0, 0)))["offers"]![0]!;
        var done = Value(Dispatch(session, "transfer.commit", Args(Send(PokemonHandle.InBox(0, 0)))))!;

        offer["arrives"]!["form"]!["name"]!.GetValue<string>().Should().Be("Altered");
        offer["changes"]!.AsArray().Select(c => c!.ToJsonString()).Should().Contain(
            """{"field":"form","before":"Origin","after":"Altered","reason":"formReverted"}""",
            """{"field":"heldItem","before":"Griseous Orb","after":null,"reason":"formReverted"}""",
            """{"field":"ability","before":"Levitate","after":"Pressure","reason":"formReverted"}""");
        offer["saveChanges"]!.AsArray().Select(c => c!.ToJsonString()).Should().Contain("""{"kind":"itemReturned","save":"sender","label":"Griseous Orb"}""");
        var mine = new Game(new SAV4Pt(Bytes(done["save"]!)));
        mine.Trainer.Inventories.InventoryItems.Values.SelectMany(pouch => pouch.Items).Should().Contain(item => item.Id == GriseousOrb && item.Count == 1);
        var partner = Game.LoadFrom(Bytes(done["partner"]!), "partner.dsv");
        partner.Trainer.PokemonBox.All[Index(partner, done["arrived"]![0]!["at"]!)].Form.Form.Name.Should().Be("Altered");
    }

    [Fact]
    public void ReceivingAFatefulArceusInPlatinumStartsTheArceusEvent()
    {
        var session = new Session();
        session.Load(new Game(Platinum()), "platinum.sav");
        Open(session, Edited(SaveFilePath.HgSs, pk => (pk.Species, pk.FatefulEncounter) = ((ushort)Species.Arceus, true)));

        var offer = Preview(session, Receive(PokemonHandle.InBox(0, 0)))["offers"]![0]!;
        var done = Value(Dispatch(session, "transfer.commit", Args(Receive(PokemonHandle.InBox(0, 0)))))!;

        offer["saveChanges"]!.AsArray().Select(c => c!.ToJsonString()).Should().Contain("""{"kind":"eventVar","save":"receiver","label":"Arceus Event Hiker: In Oreburgh Mine"}""");
        new SAV4Pt(Bytes(done["save"]!)).GetWork(86).Should().Be(1);
    }

    private static JsonNode Open(Session session, string partner) =>
        Value(Dispatch(session, "transfer.open", Args(Convert.ToBase64String(File.ReadAllBytes(partner)), "partner.sav", null!)))!;

    private static JsonNode Open(Session session, byte[] partner) =>
        Value(Dispatch(session, "transfer.open", Args(Convert.ToBase64String(partner), "partner.sav", null!)))!;

    private static JsonNode Preview(Session session, object offer) => Value(Dispatch(session, "transfer.preview", Args(offer)))!;

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

    private const byte Gen2MetalCoat = 0x8F;
    private const ushort GriseousOrb = 112;

    private static int EverstoneIn(PKM pk) => pk.Format == 2 ? 0x70 : pk.Format == 3 ? 195 : 229;

    private static Session LoadedWith(string saveFile, Action<PKM> firstBoxPokemon)
    {
        var session = new Session();
        session.Load(Game.LoadFrom(Edited(saveFile, firstBoxPokemon), saveFile), saveFile);
        return session;
    }

    private static byte[] Edited(string saveFile, Action<PKM> firstBoxPokemon)
    {
        var save = SaveUtil.GetSaveFile(File.ReadAllBytes(saveFile))!;
        var pokemon = save.GetBoxSlotAtIndex(0);
        var nicknamed = pokemon.IsNicknamed;
        firstBoxPokemon(pokemon);
        if (!nicknamed) pokemon.ClearNickname();
        save.SetBoxSlotAtIndex(pokemon, 0, EntityImportSettings.None);
        return save.Write().ToArray();
    }

    private static SAV4Pt Platinum(params PK4[] boxed)
    {
        // PKHeX can't write a blank Gen 4 save, which lacks the block footers, but writes a zeroed one.
        var save = new SAV4Pt(new byte[SaveUtil.SIZE_G4RAW]);
        for (var index = 0; index < boxed.Length; index++)
            save.SetBoxSlotAtIndex(boxed[index], index, EntityImportSettings.None);
        return save;
    }

    private static byte[] Bytes(JsonNode exported) => Convert.FromBase64String(exported["bytes"]!.GetValue<string>());
}
