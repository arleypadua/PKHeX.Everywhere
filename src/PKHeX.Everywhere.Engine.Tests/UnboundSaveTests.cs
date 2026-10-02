using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;
using PKHeX.Facade;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class UnboundSaveTests
{
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.Unbound);

    static UnboundSaveTests() => SaveFormats.Register(new UnboundFormat());

    private static Session LoadedUnbound(byte[]? bytes = null)
    {
        var session = new Session();
        Value(Dispatch(session, "game.load", Args(Convert.ToBase64String(bytes ?? Fixture), "unbound.sav")));
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
    public void ListsEveryKnownPokemonInAll25Boxes()
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
                [20] = 30, [21] = 30, [22] = 27, [23] = 28, [24] = 20,
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
    public void ExportingWithoutEditsKeepsEveryByte() =>
        Exported(LoadedUnbound()).Should().Equal(Fixture);

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
    public void LegalityReportsInvalidWithoutChangingTheSave()
    {
        var session = LoadedUnbound();

        Details(session, PokemonHandle.Party(0))["legality"]!["valid"]!.GetValue<bool>().Should().BeFalse();

        Exported(session).Should().Equal(Fixture);
    }

    [Fact]
    public void ShowdownExportsThePokemon() =>
        Value(Dispatch(LoadedUnbound(), "pokemon.showdown", Args(PokemonHandle.Party(0))))!.GetValue<string>()
            .Should().StartWith("Latias (F) @").And.EndWith("- Mist Ball\n- Light Screen\n- Reflect\n- Roost");

    [Fact]
    public void EncountersFailWithoutChangingTheSave()
    {
        var session = LoadedUnbound();

        Error(Dispatch(session, "encounters.search", Args((int)Core.GameVersion.FR, (int)Core.Species.Latias))).Should().Be("bad-arguments");
        Value(Dispatch(session, "encounters.search", Args((int)Core.GameVersion.SL, (int)Core.Species.Latias)))!.AsArray().Should().NotBeEmpty();
        Error(Dispatch(session, "box.addEncounter", Args(0))).Should().Be("unexpected");

        Exported(session).Should().Equal(Fixture);
    }

    [Fact]
    public void VanillaFireRedStillLoadsAsFireRed()
    {
        var session = new Session();

        Value(Dispatch(session, "game.load", Args(Convert.ToBase64String(File.ReadAllBytes(SaveFilePath.FireRed)), "firered.sav")));

        session.Game!.SaveFile.Should().BeOfType<Core.SAV3FRLG>();
        Value(Dispatch(session, "game.version", "[]"))!["generation"]!.GetValue<string>().Should().Be("Gen3");
    }

    private static object Arg(PokemonHandle at) =>
        new { source = at.Source.ToString().ToLowerInvariant(), slot = at.Slot, box = at.Box };
}
