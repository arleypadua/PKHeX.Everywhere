using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Everywhere.RomHacks.Cfru;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;
using PKHeX.Facade;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class UnboundSaveTests
{
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.Unbound);

    private static Session LoadedUnbound(byte[]? bytes = null)
    {
        var session = new Session();
        Value(Dispatch(session, "game.load", Args(Convert.ToBase64String(bytes ?? Fixture), "unbound.sav", null!)));
        return session;
    }

    private static byte[] Exported(Session session) =>
        Convert.FromBase64String(Value(Dispatch(session, "game.export", "[]"))!["bytes"]!.GetValue<string>());

    private static JsonNode Details(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.details", Args(at)))!;

    private static string[] MoveNames(JsonNode details) =>
        details["moves"]!.AsArray().Select(move => move!["name"]!.GetValue<string>()).ToArray();

    [Fact]
    public void LoadsAsUnboundAndReadsTheTrainer()
    {
        var session = LoadedUnbound();

        session.Game!.SaveFile.Should().BeOfType<UnboundSave>();
        session.Game.SaveFile.ChecksumsValid.Should().BeTrue();
        var trainer = Value(Dispatch(session, "trainer.get", "[]"))!;
        trainer["name"]!.GetValue<string>().Should().Be("Kadhem");
        trainer["money"]!.GetValue<uint>().Should().Be(1811990);
    }

    [Fact]
    public void ListsTheParty()
    {
        var party = Value(Dispatch(LoadedUnbound(), "party.get", "[]"))!.AsArray();

        party.Select(p => (p!["species"]!.GetValue<string>(), p["level"]!.GetValue<int>()))
            .Should().Equal(("Latias", 50));
    }

    [Fact]
    public void ListsEveryPokemonInAll25Boxes()
    {
        var session = LoadedUnbound();

        var boxed = Value(Dispatch(session, "box.get", "[]"))!.AsArray();

        session.Game!.SaveFile.BoxCount.Should().Be(25);
        boxed.GroupBy(p => p!["at"]!["box"]!.GetValue<int>())
            .ToDictionary(box => box.Key + 1, box => box.Count())
            .Should().Equal(new Dictionary<int, int>
            {
                [1] = 30, [2] = 30, [3] = 30, [4] = 30, [5] = 30, [6] = 30, [7] = 30, [8] = 26, [9] = 30, [10] = 30,
                [11] = 25, [12] = 30, [13] = 25, [14] = 30, [15] = 30, [16] = 30, [17] = 29, [18] = 30, [19] = 30,
                [20] = 30, [21] = 30, [22] = 27, [23] = 30, [24] = 20,
            });
    }

    [Theory]
    [InlineData(0, 0, "Venusaur", 50, new[] { "Giga Drain", "Sludge Bomb", "Leech Seed", "Synthesis" })]
    [InlineData(0, 8, "Raticate", 50, new[] { "Super Fang", "Final Gambit", "Toxic", "Scary Face" })]
    [InlineData(18, 29, "Vikavolt", 50, new[] { "Zap Cannon", "Guillotine", "Charge", "Sticky Web" })]
    [InlineData(19, 0, "Crabominable", 50, new[] { "Ice Hammer", "Close Combat", "Earthquake", "Stone Edge" })]
    [InlineData(22, 0, "Thievul", 100, new[] { "Dark Pulse", "Hidden Power", "Burning Jealousy", "Knock Off" })]
    [InlineData(23, 0, "Dracovish", 100, new[] { "Fishious Rend", "Outrage", "Crunch", "Psychic Fangs" })]
    public void OpensTheDetailsOfABoxPokemon(int box, int slot, string species, int level, string[] moves)
    {
        var details = Details(LoadedUnbound(), PokemonHandle.InBox(box, slot));

        Facade.Repositories.SpeciesRepository.Find(details["species"]!.GetValue<ushort>())!.Name.Should().Be(species);
        details["level"]!.GetValue<int>().Should().Be(level);
        MoveNames(details).Should().Equal(moves);
    }

    [Fact]
    public void OpensTheDetailsOfAPartyPokemon()
    {
        var details = Details(LoadedUnbound(), PokemonHandle.Party(0));

        details["species"]!.GetValue<int>().Should().Be((int)Core.Species.Latias);
        details["level"]!.GetValue<int>().Should().Be(50);
        MoveNames(details).Should().Equal("Mist Ball", "Light Screen", "Reflect", "Roost");
    }

    [Fact]
    public void AMoveSlotHoldingAnUnmappedMoveIsUnknown()
    {
        const ushort unmappedMove = 90;
        var session = LoadedUnbound(FixtureWith(save =>
        {
            var latias = (CfruPokemon)save.GetPartySlotAtIndex(0);
            latias.SetMoveIndex(3, unmappedMove);
            save.SetPartySlotAtIndex(latias, 0);
        }));

        var moves = Details(session, PokemonHandle.Party(0))["moves"]!.AsArray();

        moves.Select(move => move!["isUnknown"]!.GetValue<bool>()).Should().Equal(false, false, false, true);
        moves[3]!["name"]!.GetValue<string>().Should().Be($"Unknown move #{unmappedMove}");
    }

    [Fact]
    public void ExportingWithoutEditsKeepsEveryByte() =>
        Exported(LoadedUnbound()).Should().Equal(Fixture);

    [Fact]
    public void AnExportedEditKeepsTheRtcTrailerAndChangesOnlyItsSector()
    {
        var trailer = Enumerable.Range(1, 0x10).Select(b => (byte)b).ToArray();
        var withTrailer = Fixture.Concat(trailer).ToArray();
        var session = LoadedUnbound(withTrailer);

        Value(Dispatch(session, "trainer.setMoney", Args(1234)));
        var exported = Exported(session);

        exported.Should().HaveCount(withTrailer.Length);
        exported[Fixture.Length..].Should().Equal(trailer);
        Enumerable.Range(0, exported.Length).Where(offset => exported[offset] != withTrailer[offset])
            .Select(offset => offset / 0x1000).Distinct().Should().ContainSingle();
        Value(Dispatch(LoadedUnbound(exported), "trainer.get", "[]"))!["money"]!.GetValue<uint>().Should().Be(1234);
    }

    [Theory]
    [MemberData(nameof(EditedPokemon))]
    public void EditedMovesIvsAndHeldItemSurviveExportAndReload(PokemonHandle at)
    {
        var session = LoadedUnbound();
        var patch = new
        {
            heldItem = 234,
            moves = new[] { (int)Core.Move.Thunderbolt, (int)Core.Move.Surf, (int)Core.Move.IceBeam, (int)Core.Move.Psychic },
            ivs = new { health = 31, attack = 0, defense = 31, specialAttack = 31, specialDefense = 31, speed = 30 },
        };

        Value(Dispatch(session, "pokemon.update", JsonSerializer.Serialize(new object[] { Arg(at), patch })));

        var reloadedSession = LoadedUnbound(Exported(session));
        reloadedSession.Game!.SaveFile.ChecksumsValid.Should().BeTrue();
        var reloaded = Details(reloadedSession, at);
        reloaded["heldItem"]!.GetValue<int>().Should().Be(234);
        MoveNames(reloaded).Should().Equal("Thunderbolt", "Surf", "Ice Beam", "Psychic");
        reloaded["ivs"]!.ToJsonString().Should().Be("""{"health":31,"attack":0,"defense":31,"specialAttack":31,"specialDefense":31,"speed":30}""");
    }

    public static TheoryData<PokemonHandle> EditedPokemon => new() { PokemonHandle.Party(0), PokemonHandle.InBox(23, 0) };

    [Fact]
    public void AnEditLeavesTheOtherPokemonAsTheyWere()
    {
        var session = LoadedUnbound();
        var before = Value(Dispatch(session, "box.get", "[]"))!.ToJsonString();

        Value(Dispatch(session, "pokemon.setLevel", Args(PokemonHandle.InBox(0, 0), 60)));

        var after = Value(Dispatch(LoadedUnbound(Exported(session)), "box.get", "[]"))!.AsArray();
        after.Where(p => p!["at"]!["box"]!.GetValue<int>() != 0 || p["at"]!["slot"]!.GetValue<int>() != 0)
            .Select(p => p!.ToJsonString())
            .Should().Equal(JsonNode.Parse(before)!.AsArray().Skip(1).Select(p => p!.ToJsonString()));
    }

    [Fact]
    public void FormatsListTheRegisteredSaveFormats() =>
        Value(Dispatch(new Session(), "game.formats", "[]"))!.AsArray()
            .Select(format => (format!["id"]!.GetValue<string>(), format["name"]!.GetValue<string>()))
            .Should().Contain(("unbound", "Pokémon Unbound"));

    [Fact]
    public void LoadsWithTheChosenFormat()
    {
        var session = new Session();

        Value(Dispatch(session, "game.load", Args(Convert.ToBase64String(Fixture), "unbound.sav", "unbound")));

        session.Game!.SaveFile.Should().BeOfType<UnboundSave>();
        session.FileName.Should().Be("unbound.sav");
    }

    [Fact]
    public void LoadingWithAnUnknownFormatFailsWithNotFound()
    {
        var session = new Session();

        Error(Dispatch(session, "game.load", Args(Convert.ToBase64String(Fixture), "unbound.sav", "nope"))).Should().Be("not-found");

        session.Game.Should().BeNull();
    }

    [Fact]
    public void LoadingASaveTheChosenFormatCannotReadFailsWithInvalidSave() =>
        Error(Dispatch(new Session(), "game.load", Args(Convert.ToBase64String(File.ReadAllBytes(SaveFilePath.Emerald)), "emerald.sav", "unbound")))
            .Should().Be("invalid-save");

    [Fact]
    public void TheSummaryNamesTheFormatAndListsNoCapabilities()
    {
        var summary = Value(Dispatch(LoadedUnbound(), "game.get", "[]"))!;

        summary["format"]!["id"]!.GetValue<string>().Should().Be("unbound");
        summary["format"]!["name"]!.GetValue<string>().Should().Be("Pokémon Unbound");
        summary["format"]!["baseGameId"]!.GetValue<int>().Should().Be((int)Core.GameVersion.FR);
        summary["capabilities"]!.AsArray().Should().BeEmpty();
    }

    [Fact]
    public void TheVersionAndOverviewCarryTheFormatId()
    {
        var session = LoadedUnbound();
        var exported = new List<GameExported>();
        session.Published += engineEvent =>
        {
            if (engineEvent is GameExported e) exported.Add(e);
        };

        Value(Dispatch(session, "game.version", "[]"))!["formatId"]!.GetValue<string>().Should().Be("unbound");
        Exported(session);
        exported.Should().ContainSingle().Which.Game.FormatId.Should().Be("unbound");
    }

    [Fact]
    public void DetailsLeaveLegalityOut() =>
        Details(LoadedUnbound(), PokemonHandle.Party(0))["legality"].Should().BeNull();

    [Theory]
    [MemberData(nameof(CallsBehindCapabilities))]
    public void CallsBehindACapabilityFailWithNotSupportedWithoutChangingTheSave(string call, string args)
    {
        var session = LoadedUnbound();

        Error(Dispatch(session, call, args)).Should().Be("not-supported");

        Exported(session).Should().Equal(Fixture);
    }

    public static TheoryData<string, string> CallsBehindCapabilities => new()
    {
        { "pokemon.showdown", Args(PokemonHandle.Party(0)) },
        { "party.showdown", "[]" },
        { "box.showdown", "[]" },
        { "encounters.versions", "[]" },
        { "encounters.search", Args((int)Core.GameVersion.FR, (int)Core.Species.Latias) },
        { "box.addEncounter", Args(0) },
        { "events.get", "[]" },
        { "events.flag", Args(0) },
        { "events.setFlag", Args(0, true) },
        { "events.setWork", Args(0, 1) },
        { "events.giveTickets", Args(false) },
    };

    [Fact]
    public void PlugInHooksDontRun()
    {
        var session = LoadedUnbound();
        var host = new PlugInHost(session);
        var ran = PlugInRuns(session);
        host.Register(PlugIn("PKHeX.Everywhere.Engine.Tests.PlugIn"));

        Value(Dispatch(session, "pokemon.setLevel", Args(PokemonHandle.Party(0), 60)));

        ran.Should().BeEmpty();
        Details(session, PokemonHandle.Party(0))["nickname"]!.GetValue<string>().Should().NotBe("Changed");
    }

    [Fact]
    public void PlugInActionsAndPagesAreOff()
    {
        var session = LoadedUnbound();
        var host = new PlugInHost(session);
        var ran = PlugInRuns(session);
        host.Register(PlugIn("PKHeX.Everywhere.Engine.Tests.PlugIn"));
        host.Register(PlugIn("PKHeX.Web.Plugins.AutoLegality"));
        Value(Dispatch(session, "plugins.actions", """["quick", null]"""))!.AsArray().Should().BeEmpty();
        Value(Dispatch(session, "plugins.actions", Args("pokemon", PokemonHandle.Party(0))))!.AsArray().Should().BeEmpty();
        Value(Dispatch(session, "plugins.pages", "[]"))!.AsArray().Should().BeEmpty();
        Error(Dispatch(session, "plugins.run", """["PKHeX.Everywhere.Engine.Tests.PlugIn.Greet", null]""")).Should().Be("not-supported");
        Error(Dispatch(session, "plugins.run", Args("PKHeX.Web.Plugins.AutoLegality.MakeLegalOnClick", PokemonHandle.Party(0))))
            .Should().Be("not-supported");
        Error(Dispatch(session, "plugins.pageModule", """["PKHeX.Everywhere.Engine.Tests.PlugIn", "hello"]""")).Should().Be("not-supported");

        ran.Should().BeEmpty();
        Exported(session).Should().Equal(Fixture);
    }

    [Fact]
    public void VanillaFireRedStillLoadsAsFireRed()
    {
        var session = new Session();

        Value(Dispatch(session, "game.load", Args(Convert.ToBase64String(File.ReadAllBytes(SaveFilePath.FireRed)), "firered.sav", null!)));

        session.Game!.SaveFile.Should().BeOfType<Core.SAV3FRLG>();
        Value(Dispatch(session, "game.version", "[]"))!["generation"]!.GetValue<string>().Should().Be("Gen3");
    }

    private static byte[] PlugIn(string id) => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "plugins", $"{id}.dll"));

    private const int UnknownSlot = (22 * 30) + 18;
    private const ushort ShadowWarrior = 706;
    private const int ShadowWarriorId = 0xF000 + ShadowWarrior;
    private const int ZygardeCellId = 0xF000 + 835;
    private const int ZygardeCoreId = 0xF000 + 836;
    private static readonly PokemonHandle UnknownSpecies = PokemonHandle.InBox(22, 18);
    private static readonly PokemonHandle HoldsUnknownItem = PokemonHandle.InBox(19, 27);

    private static int Item(string name) => Facade.Repositories.ItemRepository.GetItemByName(name)!.Id;

    private static int[] Ids(JsonNode? choices) => choices!.AsArray().Select(choice => choice!["id"]!.GetValue<int>()).ToArray();

    private static string Update(Session session, PokemonHandle at, object patch) =>
        Dispatch(session, "pokemon.update", JsonSerializer.Serialize(new object[] { Arg(at), patch }));

    [Fact]
    public void TheSpeciesCatalogListsOnlyUnboundSpecies()
    {
        var species = Ids(Value(Dispatch(LoadedUnbound(), "species.list", "[]")));

        species.Where(id => id >= 1 && id <= (int)Core.Species.Enamorus).Should().HaveCount(905);
        species.Except(Enumerable.Range(1, (int)Core.Species.Enamorus)).Should().BeEquivalentTo([ShadowWarriorId, ZygardeCellId, ZygardeCoreId]);
    }

    [Fact]
    public void TheSpeciesCatalogNamesUnboundsHackSpecies() =>
        Value(Dispatch(LoadedUnbound(), "species.list", "[]"))!.AsArray()
            .Where(species => species!["id"]!.GetValue<int>() > (int)Core.Species.Enamorus)
            .Select(species => (species!["id"]!.GetValue<int>(), species["name"]!.GetValue<string>()))
            .Should().BeEquivalentTo([(ShadowWarriorId, "Shadow Warrior"), (ZygardeCellId, "Zygarde Cell"), (ZygardeCoreId, "Zygarde Core")]);

    [Fact]
    public void TheMoveCatalogListsOnlyCfruMoves()
    {
        var moves = Ids(Value(Dispatch(LoadedUnbound(), "game.moves", "[]")));

        moves.Should().HaveCount(815)
            .And.Contain((int)Core.Move.HiddenPower)
            .And.NotContain([(int)Core.Move.Fissure, (int)Core.Move.TeraBlast]);
    }

    [Fact]
    public void TheHeldItemCatalogListsOnlyUnboundItems()
    {
        var items = Ids(Value(Dispatch(LoadedUnbound(), "game.heldItems", "[]")));

        items.Should().Contain([0, Item("Leftovers"), Item("Light Clay"), Item("Latiasite")])
            .And.NotContain([Item("Ability Patch"), Item("Booster Energy")]);
    }

    [Theory]
    [MemberData(nameof(EditedPokemon))]
    public void PokemonOptionsListOnlyUnboundValues(PokemonHandle at)
    {
        var session = LoadedUnbound();
        var options = Value(Dispatch(session, "pokemon.options", Args(at)))!;

        Ids(options["moves"]).Should().BeSubsetOf(Ids(Value(Dispatch(session, "game.moves", "[]"))));
        Ids(options["species"]).Should().BeSubsetOf(Ids(Value(Dispatch(session, "species.list", "[]"))));
    }

    [Fact]
    public void HeldItemsTranslateToModernIds() =>
        Details(LoadedUnbound(), PokemonHandle.Party(0))["heldItem"]!.GetValue<int>().Should().Be(Item("Light Clay"));

    [Theory]
    [InlineData("species", (int)Core.Species.Sprigatito)]
    [InlineData("heldItem", 1606)] // Ability Patch
    public void SettingAValueOutsideUnboundsTablesIsRejected(string field, int value)
    {
        var session = LoadedUnbound();

        Error(Update(session, PokemonHandle.Party(0), new Dictionary<string, object> { [field] = value })).Should().Be("invalid-patch");

        Exported(session).Should().Equal(Fixture);
    }

    [Fact]
    public void SettingAMoveOutsideCfrusTableIsRejected()
    {
        var session = LoadedUnbound();

        Error(Update(session, PokemonHandle.Party(0), new { moves = new[] { (int)Core.Move.Fissure, 0, 0, 0 } })).Should().Be("invalid-patch");

        Exported(session).Should().Equal(Fixture);
    }

    private static void PutShadowWarriorInTheUnknownSlot(UnboundSave save)
    {
        var pokemon = (CfruPokemon)save.GetBoxSlotAtIndex(UnknownSlot);
        pokemon.SpeciesIndex = ShadowWarrior;
        save.SetBoxSlotAtIndex(pokemon, UnknownSlot);
    }

    private static byte[] WithUnknownSpecies() => FixtureWith(PutShadowWarriorInTheUnknownSlot);

    private static JsonNode Listed(Session session, PokemonHandle at) => Value(Dispatch(session, "box.get", "[]"))!.AsArray()
        .Single(p => p!["at"]!["box"]!.GetValue<int>() == at.Box && p["at"]!["slot"]!.GetValue<int>() == at.Slot)!;

    [Fact]
    public void AnUnknownSpeciesIsListedWithNoSpeciesIdAndIsNotEditable()
    {
        var session = LoadedUnbound(WithUnknownSpecies());

        var unknown = Listed(session, UnknownSpecies);
        unknown["species"]!.GetValue<string>().Should().Be("Shadow Warrior");
        unknown["speciesId"].Should().BeNull();
        unknown["isUnknown"]!.GetValue<bool>().Should().BeTrue();
        unknown["editable"]!.GetValue<bool>().Should().BeFalse();

        var known = Listed(session, PokemonHandle.InBox(0, 0));
        known["speciesId"]!.GetValue<int>().Should().BePositive();
        known["isUnknown"]!.GetValue<bool>().Should().BeFalse();
        known["editable"]!.GetValue<bool>().Should().BeTrue();
    }

    [Fact]
    public void AnUnknownSpeciesShowsItsDetailsButCantBeEdited()
    {
        var bytes = WithUnknownSpecies();
        var session = LoadedUnbound(bytes);

        var details = Details(session, UnknownSpecies);
        details["species"].Should().BeNull();
        details["isUnknown"]!.GetValue<bool>().Should().BeTrue();
        details["editable"]!.GetValue<bool>().Should().BeFalse();
        Details(session, PokemonHandle.Party(0))["editable"]!.GetValue<bool>().Should().BeTrue();
        Ids(Value(Dispatch(session, "pokemon.options", Args(UnknownSpecies)))!["heldItems"]).Should().Contain(details["heldItem"]!.GetValue<int>());

        Error(Update(session, UnknownSpecies, new { nickname = "Renamed" })).Should().Be("unknown-species");
        Error(Dispatch(session, "pokemon.setLevel", Args(UnknownSpecies, 60))).Should().Be("unknown-species");
        Error(Dispatch(session, "pokemon.edit", Args(UnknownSpecies))).Should().Be("unknown-species");
        Error(Dispatch(session, "pokemon.clone", Args(UnknownSpecies))).Should().Be("unknown-species");

        Exported(session).Should().Equal(bytes);
    }

    [Fact]
    public void AShadowWarriorsDetailsShowItsOwnTypesAbilityAndGender()
    {
        var details = Details(LoadedUnbound(WithUnknownSpecies()), UnknownSpecies);

        details["types"]!.AsArray().Select(type => type!.GetValue<int>()).Should().Equal((int)Core.MoveType.Ghost, (int)Core.MoveType.Dark);
        details["ability"]!.GetValue<int>().Should().BeOneOf((int)Core.Ability.ToughClaws, (int)Core.Ability.WonderGuard);
        details["gender"]!.GetValue<string>().Should().Be("genderless");
    }

    [Fact]
    public void AnUnknownSpeciesExportsWithItsBytes()
    {
        var bytes = WithUnknownSpecies();
        var session = LoadedUnbound(bytes);

        var exported = Convert.FromBase64String(Value(Dispatch(session, "pokemon.export", Args(UnknownSpecies)))!["bytes"]!.GetValue<string>());

        exported.Should().Equal(new UnboundSave(bytes).GetBoxSlotAtIndex(UnknownSlot).Data.ToArray());
    }

    // The Engine has no command to release a Pokémon, so it's released in the save before it loads, as the game would.
    [Fact]
    public void AnUnknownSpeciesReleasedFromItsSlotLeavesItEmpty()
    {
        var released = FixtureWith(save =>
        {
            PutShadowWarriorInTheUnknownSlot(save);
            save.SetBoxSlotAtIndex(save.BlankPKM, UnknownSlot);
        });
        var session = LoadedUnbound(released);

        Error(Dispatch(session, "pokemon.get", Args(UnknownSpecies))).Should().Be("not-found");
        Value(Dispatch(session, "box.get", "[]"))!.AsArray()
            .Should().NotContain(p => p!["isUnknown"]!.GetValue<bool>());
        Exported(session).Should().Equal(released);
    }

    private static byte[] FixtureWith(Action<UnboundSave> change)
    {
        var save = new UnboundSave(Fixture.ToArray());
        change(save);
        return save.Write().ToArray();
    }

    [Fact]
    public void AnUnknownSpeciesMovedToAnotherBoxSlotSurvivesExport()
    {
        var to = new UnboundSave(Fixture.ToArray()).NextOpenBoxSlot();
        byte[]? moved = null;
        var session = LoadedUnbound(FixtureWith(save =>
        {
            PutShadowWarriorInTheUnknownSlot(save);
            moved = save.GetBoxSlotAtIndex(UnknownSlot).Data.ToArray();
            save.SetBoxSlotAtIndex(save.GetBoxSlotAtIndex(UnknownSlot), to);
            save.SetBoxSlotAtIndex(save.BlankPKM, UnknownSlot);
        }));

        var reloaded = LoadedUnbound(Exported(session));

        reloaded.Game!.SaveFile.GetBoxSlotAtIndex(to).Data.ToArray().Should().Equal(moved);
        Value(Dispatch(reloaded, "box.get", "[]"))!.AsArray()
            .Where(p => p!["species"]!.GetValue<string>() == "Shadow Warrior")
            .Select(p => (p!["at"]!["box"]!.GetValue<int>() * 30) + p["at"]!["slot"]!.GetValue<int>())
            .Should().Equal(to);
    }

    [Fact]
    public void AnUnknownSpeciesInThePartySurvivesAnEditToAnotherMember()
    {
        var session = LoadedUnbound(FixtureWith(save =>
        {
            PutShadowWarriorInTheUnknownSlot(save);
            save.SetPartySlotAtIndex(save.GetBoxSlotAtIndex(UnknownSlot), 1);
        }));
        var before = session.Game!.SaveFile.GetPartySlotAtIndex(1).Data.ToArray();

        Value(Dispatch(session, "pokemon.setLevel", Args(PokemonHandle.Party(0), 60)));

        var reloaded = LoadedUnbound(Exported(session));
        Value(Dispatch(reloaded, "party.get", "[]"))!.AsArray().Select(p => p!["species"]!.GetValue<string>())
            .Should().Equal("Latias", "Shadow Warrior");
        reloaded.Game!.SaveFile.GetPartySlotAtIndex(1).Data.ToArray().Should().Equal(before);
    }

    [Fact]
    public void AnUnknownHeldItemShowsAsUnknown()
    {
        var session = LoadedUnbound();
        var details = Details(session, HoldsUnknownItem);
        var heldItem = details["heldItem"]!.GetValue<int>();

        details["heldItemIsUnknown"]!.GetValue<bool>().Should().BeTrue();
        Value(Dispatch(session, "pokemon.options", Args(HoldsUnknownItem)))!["heldItems"]!.AsArray()
            .Where(item => item!["isUnknown"]!.GetValue<bool>())
            .Select(item => (item!["id"]!.GetValue<int>(), item["name"]!.GetValue<string>()))
            .Should().Equal((heldItem, "Unknown item #640"));
    }

    [Fact]
    public void AnUnknownHeldItemSurvivesAnEditToAnotherField()
    {
        var session = LoadedUnbound();
        var heldItem = Details(session, HoldsUnknownItem)["heldItem"]!.GetValue<int>();

        Value(Update(session, HoldsUnknownItem, new { ivs = new { attack = 0 } }));

        var reloaded = Details(LoadedUnbound(Exported(session)), HoldsUnknownItem);
        reloaded["ivs"]!["attack"]!.GetValue<int>().Should().Be(0);
        reloaded["heldItem"]!.GetValue<int>().Should().Be(heldItem);
        reloaded["heldItemIsUnknown"]!.GetValue<bool>().Should().BeTrue();
    }

    private static JsonNode Pouch(Session session, string name) => Value(Dispatch(session, "inventory.get", "[]"))!.AsArray()
        .Single(pouch => pouch!["name"]!.GetValue<string>() == name)!;

    private static (int Id, string Name, int Count)[] Owned(JsonNode pouch) => pouch["items"]!.AsArray()
        .Select(item => (item!["id"]!.GetValue<int>(), item["name"]!.GetValue<string>(), item["count"]!.GetValue<int>()))
        .ToArray();

    private static (int Id, string Name, int Count)[] Unknown(JsonNode pouch) => pouch["items"]!.AsArray()
        .Where(item => item!["isUnknown"]!.GetValue<bool>())
        .Select(item => (item!["id"]!.GetValue<int>(), item["name"]!.GetValue<string>(), item["count"]!.GetValue<int>()))
        .ToArray();

    private static string SetItem(Session session, string pouch, int itemId, int count) =>
        Dispatch(session, "inventory.setItem", Args(new ItemHandle(pouch, itemId), count));

    private static int[] ChangedOffsets(byte[] exported) =>
        Enumerable.Range(0, exported.Length).Where(offset => exported[offset] != Fixture[offset]).ToArray();

    [Fact]
    public void TheBagListsTheMainBallTmAndBerryPockets()
    {
        var pouches = Value(Dispatch(LoadedUnbound(), "inventory.get", "[]"))!.AsArray();

        pouches.Select(pouch => (pouch!["name"]!.GetValue<string>(), pouch["items"]!.AsArray().Count))
            .Should().Equal(("Balls", 16), ("Berries", 66), ("Items", 276), ("TMHMs", 128));
    }

    [Theory]
    [InlineData("Items", "Max Repel", 18)]
    [InlineData("Items", "Rare Candy", 73)]
    [InlineData("Balls", "Premier Ball", 166)]
    [InlineData("Balls", "Dream Ball", 2)]
    [InlineData("TMHMs", "TM01", 1)]
    [InlineData("TMHMs", "HM01", 1)]
    [InlineData("Berries", "Oran Berry", 11)]
    [InlineData("Berries", "Enigma Berry", 1)]
    public void TheBagTranslatesItemsToModernIds(string pouch, string item, int count) =>
        Owned(Pouch(LoadedUnbound(), pouch)).Should().ContainSingle(owned => owned.Id == Item(item) && owned.Count == count);

    [Theory]
    [InlineData("Items", 79, 92)]
    [InlineData("TMHMs", 444, 1)]
    [InlineData("Berries", 174, 4)]
    public void AnUnknownBagItemIsListedWithTheOtherItems(string pouch, int index, int count) =>
        Owned(Pouch(LoadedUnbound(), pouch)).Should().ContainSingle(owned => owned.Name == $"Unknown item #{index}")
            .Which.Count.Should().Be(count);

    [Fact]
    public void OwnedItemsSayWhetherTheyAreUnknownAndUnknownOnesHaveDistinctIds()
    {
        var pouch = Pouch(LoadedUnbound(), "Items");
        var unknown = Unknown(pouch);

        unknown.Should().HaveCount(8).And.OnlyContain(item => item.Name.StartsWith("Unknown item #"));
        unknown.Select(item => item.Id).Should().OnlyHaveUniqueItems().And.NotContain(0);
        Owned(pouch).Except(unknown).Should().OnlyContain(item => !item.Name.StartsWith("Unknown item #"));
    }

    [Fact]
    public void AnUnknownBagItemCantBeSetInAnotherPouch()
    {
        var session = LoadedUnbound();
        Error(SetItem(session, "Balls", Unknown(Pouch(session, "Items"))[0].Id, 1)).Should().Be("bad-arguments");
    }

    [Fact]
    public void TmsHoldOneOfEach() =>
        Pouch(LoadedUnbound(), "TMHMs")["items"]!.AsArray().Select(item => item!["maxCount"]!.GetValue<int>())
            .Should().OnlyContain(maxCount => maxCount == 1);

    [Fact]
    public void EveryItemInThePocketsCanBeSetAgain()
    {
        var session = LoadedUnbound();

        foreach (var pouch in new[] { "Items", "Balls", "TMHMs", "Berries" })
        foreach (var (id, _, count) in Owned(Pouch(session, pouch)))
            Value(SetItem(session, pouch, id, count));

        Exported(session).Should().Equal(Fixture);
    }

    // The fixture owns every TM, so the one added back is one removed first.
    [Theory]
    [InlineData("Items", 5)]
    [InlineData("Balls", 5)]
    [InlineData("TMHMs", 1)]
    [InlineData("Berries", 5)]
    public void AddingChangingAndRemovingAnItemSurvivesExportAndReload(string pouch, int changeTo)
    {
        var session = LoadedUnbound();
        var before = Owned(Pouch(session, pouch));
        var owned = before.Select(item => item.Id).ToArray();
        var (changed, removed, freed) = (owned[0], owned[1], owned[2]);

        Value(SetItem(session, pouch, changed, changeTo));
        Value(SetItem(session, pouch, removed, 0));
        Value(SetItem(session, pouch, freed, 0));
        var added = Pouch(session, pouch)["addable"]!.AsArray().Select(item => item!["id"]!.GetValue<int>()).First(id => id != removed);
        Value(SetItem(session, pouch, added, 1));

        var reloaded = LoadedUnbound(Exported(session));
        reloaded.Game!.SaveFile.ChecksumsValid.Should().BeTrue();
        var expected = before
            .Where(item => item.Id != removed && item.Id != freed)
            .Select(item => item.Id == changed ? item with { Count = changeTo } : item)
            .Append((added, reloaded.Game.ItemRepository.GetGameItem((ushort)added).Name, 1));
        Owned(Pouch(reloaded, pouch)).Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("Items")]
    [InlineData("Balls")]
    [InlineData("TMHMs")]
    [InlineData("Berries")]
    public void RemovingAnItemChangesOnlyItsSlot(string pouch)
    {
        var session = LoadedUnbound();

        Value(SetItem(session, pouch, Owned(Pouch(session, pouch)).First().Id, 0));

        var changed = ChangedOffsets(Exported(session));
        changed.Should().NotBeEmpty();
        (changed.Max() - changed.Min()).Should().BeLessThan(4);
    }

    [Theory]
    [InlineData("Items")]
    [InlineData("Balls")]
    [InlineData("Berries")]
    public void AddingAnItemChangesOnlyTheSlotItFills(string pouch)
    {
        var session = LoadedUnbound();

        Value(SetItem(session, pouch, Pouch(session, pouch)["addable"]!.AsArray().First()!["id"]!.GetValue<int>(), 1));

        var changed = ChangedOffsets(Exported(session));
        changed.Should().NotBeEmpty();
        (changed.Max() - changed.Min()).Should().BeLessThan(4);
    }

    [Fact]
    public void UnknownBagItemsSurviveAnEditInTheirPocket()
    {
        var session = LoadedUnbound();
        var unknown = Unknown(Pouch(session, "Items"));
        unknown.Should().HaveCount(8);

        Value(SetItem(session, "Items", Item("Max Repel"), 0));

        Unknown(Pouch(LoadedUnbound(Exported(session)), "Items")).Should().Equal(unknown);
    }

    // The key items pocket isn't listed yet, so its bytes are checked where they follow the main pocket in sector 30.
    private static readonly Range KeyItems = 0x1E1F0..0x1E31C;

    [Fact]
    public void TheMainPocketHolds450EntriesAndKeepsTheOtherPockets()
    {
        var session = LoadedUnbound();
        var others = new[] { "Balls", "TMHMs", "Berries" }.Select(pouch => Owned(Pouch(session, pouch))).ToArray();
        var full = FixtureWith(save =>
        {
            var bag = save.Inventory;
            foreach (var slot in bag.Pouches[0].Items.Where(slot => slot.Index == 0)) (slot.Index, slot.Count) = (Item("Potion"), 1);
            bag.CopyTo(save);
        });

        var exported = Exported(LoadedUnbound(full));
        var reloaded = LoadedUnbound(exported);

        Owned(Pouch(reloaded, "Items")).Should().HaveCount(450);
        exported[KeyItems].Should().Equal(Fixture[KeyItems]);
        new[] { "Balls", "TMHMs", "Berries" }.Select(pouch => Owned(Pouch(reloaded, pouch))).Should().BeEquivalentTo(others);
        Error(SetItem(reloaded, "Items", Pouch(reloaded, "Items")["addable"]!.AsArray().First()!["id"]!.GetValue<int>(), 1))
            .Should().Be("pouch-full");
    }

    private static object Arg(PokemonHandle at) =>
        new { source = at.Source.ToString().ToLowerInvariant(), slot = at.Slot, box = at.Box };
}
