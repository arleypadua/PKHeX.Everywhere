using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.RomHacks.Cfru;
using PKHeX.Everywhere.RomHacks.Cfru.RadicalRed;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;
using PKHeX.Facade;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class RadicalRedSaveTests
{
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.RadicalRed);

    private static string Load(Session session, byte[] bytes, string? formatId) =>
        Dispatch(session, "game.load", Args(Convert.ToBase64String(bytes), "radicalred.sav", formatId!));

    private static Session LoadedRadicalRed(byte[]? bytes = null)
    {
        var session = new Session();
        Value(Load(session, bytes ?? Fixture, "radicalred"));
        return session;
    }

    private static byte[] Exported(Session session) =>
        Convert.FromBase64String(Value(Dispatch(session, "game.export", "[]"))!["bytes"]!.GetValue<string>());

    private static JsonNode Details(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.details", Args(at)))!;

    private static string Update(Session session, PokemonHandle at, object patch) =>
        Dispatch(session, "pokemon.update", Args(at, patch));

    private static byte[] FixtureWith(Action<RadicalRedSave> change)
    {
        var save = new RadicalRedSave(Fixture.ToArray());
        change(save);
        return save.Write().ToArray();
    }

    [Fact]
    public void LoadingWithoutAFormatRequiresAChoiceAndListsRadicalRed()
    {
        var session = new Session();

        var envelope = JsonNode.Parse(Load(session, Fixture, null))!;

        Error(envelope.ToJsonString()).Should().Be("format-choice-required");
        envelope["error"]!["candidates"]!.AsArray()
            .Select(candidate => (candidate!["id"]!.GetValue<string>(), candidate["name"]!.GetValue<string>()))
            .Should().Equal(("radicalred", "Pokémon Radical Red"));
        session.Game.Should().BeNull();
    }

    [Fact]
    public void LoadsAsRadicalRedWhenChosen()
    {
        var session = LoadedRadicalRed();

        session.Game!.SaveFile.Should().BeOfType<RadicalRedSave>();
        session.Game.SaveFile.ChecksumsValid.Should().BeTrue();
        Value(Dispatch(session, "game.get", "[]"))!["format"]!["id"]!.GetValue<string>().Should().Be("radicalred");
    }

    [Fact]
    public void ListsTheParty()
    {
        var party = Value(Dispatch(LoadedRadicalRed(), "party.get", "[]"))!.AsArray();

        party.Select(p => (p!["species"]!.GetValue<string>(), p["level"]!.GetValue<int>()))
            .Should().Equal(("Tyranitar", 100), ("Excadrill", 100), ("Dragonite", 100), ("Slowbro", 100), ("Garchomp", 100));
    }

    [Fact]
    public void ListsTheBoxesIncludingBox25()
    {
        var session = LoadedRadicalRed();

        var boxed = Value(Dispatch(session, "box.get", "[]"))!.AsArray();

        session.Game!.SaveFile.BoxCount.Should().Be(25);
        boxed.Where(p => p!["at"]!["box"]!.GetValue<int>() == 24).Should().HaveCount(20);
        boxed.Select(p => p!["species"]!.GetValue<string>()).Should().NotContain(species => species.StartsWith("Unknown"));
    }

    [Fact]
    public void ReadsTheTrainer()
    {
        var trainer = Value(Dispatch(LoadedRadicalRed(), "trainer.get", "[]"))!;

        trainer["name"]!.GetValue<string>().Should().Be("Radical");
        trainer["money"]!.GetValue<uint>().Should().Be(1003293);
    }

    private static int Item(string name) => Facade.Repositories.ItemRepository.GetItemByName(name)!.Id;

    [Fact]
    public void HeldItemsTranslateWithRadicalRedsTable()
    {
        var session = LoadedRadicalRed();

        Enumerable.Range(0, 5).Select(slot => Details(session, PokemonHandle.Party(slot))["heldItem"]!.GetValue<int>())
            .Should().Equal(Item("Smooth Rock"), Item("Choice Band"), Item("Leftovers"), Item("Leftovers"), Item("Leftovers"));
    }

    [Theory]
    [InlineData("Dream Ball", 50)]
    [InlineData("Beast Ball", 100)]
    [InlineData("Quick Ball", 17)]
    public void TheBagTranslatesItemsWithRadicalRedsTable(string item, int count) =>
        Value(Dispatch(LoadedRadicalRed(), "inventory.get", "[]"))!.AsArray()
            .Single(pouch => pouch!["name"]!.GetValue<string>() == "Balls")!["items"]!.AsArray()
            .Should().ContainSingle(owned => owned!["id"]!.GetValue<int>() == Item(item) && owned["count"]!.GetValue<int>() == count);

    [Fact]
    public void TheShinyGarchompInThePartyIsShiny() =>
        Value(Dispatch(LoadedRadicalRed(), "party.get", "[]"))!.AsArray()
            .Select(p => (p!["species"]!.GetValue<string>(), p["isShiny"]!.GetValue<bool>()))
            .Should().Equal(("Tyranitar", false), ("Excadrill", false), ("Dragonite", false), ("Slowbro", false), ("Garchomp", true));

    [Fact]
    public void ChoosingPKHeXLoadsItAsFireRed()
    {
        var session = new Session();

        Value(Load(session, Fixture, "pkhex"));

        session.Game!.SaveFile.Should().BeOfType<Core.SAV3FRLG>();
        session.Game.Format.Should().BeNull();
    }

    [Fact]
    public void VanillaFireRedStillLoadsAsFireRedWithoutAChoice()
    {
        var session = new Session();

        Value(Load(session, File.ReadAllBytes(SaveFilePath.FireRed), null));

        session.Game!.SaveFile.Should().BeOfType<Core.SAV3FRLG>();
    }

    [Fact]
    public void UnboundStillLoadsAsUnboundWithoutAChoice()
    {
        var session = new Session();

        Value(Load(session, File.ReadAllBytes(SaveFilePath.Unbound), null));

        session.Game!.SaveFile.Should().BeOfType<UnboundSave>();
    }

    [Fact]
    public void ExportingWithoutEditsKeepsEveryByte() =>
        Exported(LoadedRadicalRed()).Should().Equal(Fixture);

    [Theory]
    [MemberData(nameof(EditedPokemon))]
    public void EditsSurviveExportAndReload(PokemonHandle at)
    {
        var session = LoadedRadicalRed();
        var patch = new
        {
            nickname = "Edited",
            moves = new[] { (int)Core.Move.Thunderbolt, (int)Core.Move.Surf, (int)Core.Move.IceBeam, (int)Core.Move.Psychic },
            ivs = new { health = 31, attack = 0, defense = 31, specialAttack = 31, specialDefense = 31, speed = 30 },
        };

        Value(Update(session, at, patch));
        Value(Dispatch(session, "trainer.setMoney", Args(1234)));

        var reloadedSession = LoadedRadicalRed(Exported(session));
        reloadedSession.Game!.SaveFile.ChecksumsValid.Should().BeTrue();
        var reloaded = Details(reloadedSession, at);
        reloaded["nickname"]!.GetValue<string>().Should().Be("Edited");
        reloaded["moves"]!.AsArray().Select(move => move!["name"]!.GetValue<string>())
            .Should().Equal("Thunderbolt", "Surf", "Ice Beam", "Psychic");
        reloaded["ivs"]!.ToJsonString().Should().Be("""{"health":31,"attack":0,"defense":31,"specialAttack":31,"specialDefense":31,"speed":30}""");
        Value(Dispatch(reloadedSession, "trainer.get", "[]"))!["money"]!.GetValue<uint>().Should().Be(1234);
    }

    public static TheoryData<PokemonHandle> EditedPokemon => new() { PokemonHandle.Party(0), PokemonHandle.InBox(24, 0) };

    private const ushort Chillet = 1375;

    [Fact]
    public void AFakeSpeciesShowsAsUnknownAndCantBeEdited()
    {
        var bytes = FixtureWith(save =>
        {
            var pokemon = (CfruPokemon)save.GetBoxSlotAtIndex(24 * 30);
            pokemon.SpeciesIndex = Chillet;
            save.SetBoxSlotAtIndex(pokemon, 24 * 30);
        });
        var session = LoadedRadicalRed(bytes);
        var at = PokemonHandle.InBox(24, 0);

        Value(Dispatch(session, "box.get", "[]"))!.AsArray()
            .Single(p => p!["at"]!["box"]!.GetValue<int>() == 24 && p["at"]!["slot"]!.GetValue<int>() == 0)!["species"]!
            .GetValue<string>().Should().Be($"Unknown (#{Chillet})");
        Error(Update(session, at, new { nickname = "Renamed" })).Should().Be("unknown-species");
        Error(Dispatch(session, "pokemon.setLevel", Args(at, 60))).Should().Be("unknown-species");

        Exported(session).Should().Equal(bytes);
    }

    // A shiny xor of 8 to 15 is shiny at 1 in 4096 odds but not at 1 in 8192.
    [Theory]
    [InlineData(7, true, true)]
    [InlineData(8, false, true)]
    [InlineData(15, false, true)]
    [InlineData(16, false, false)]
    public void ShininessUsesEachHacksOdds(ushort xor, bool radicalRed, bool unbound)
    {
        IsShiny(new RadicalRedPokemon(), xor).Should().Be(radicalRed);
        IsShiny(new UnboundPokemon(), xor).Should().Be(unbound);
    }

    private static bool IsShiny(CfruPokemon pokemon, ushort xor)
    {
        pokemon.TID16 = 12345;
        pokemon.SID16 = 54321;
        pokemon.PID = ((uint)(pokemon.TID16 ^ pokemon.SID16 ^ xor) << 16) | 0;
        return pokemon.IsShiny;
    }

    [Fact]
    public void ShininessInTheDetailsUsesRadicalRedsOdds()
    {
        var at = PokemonHandle.Party(0);
        var session = LoadedRadicalRed(FixtureWith(save =>
        {
            var pokemon = save.GetPartySlotAtIndex(0);
            pokemon.PID = ((uint)(pokemon.TID16 ^ pokemon.SID16 ^ 8) << 16) | 0;
            save.SetPartySlotAtIndex(pokemon, 0);
        }));

        Details(session, at)["isShiny"]!.GetValue<bool>().Should().BeFalse();

        Value(Update(session, at, new { isShiny = true }));

        var shiny = (CfruPokemon)LoadedRadicalRed(Exported(session)).Game!.SaveFile.GetPartySlotAtIndex(0);
        shiny.ShinyXor.Should().BeLessThan(8);
    }
}
