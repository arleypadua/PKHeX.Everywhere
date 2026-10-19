using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.RomHacks.Expansion.HeartAndSoul;
using PKHeX.Facade.Tests.Base;
using static System.Buffers.Binary.BinaryPrimitives;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class HeartAndSoulSaveTests
{
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.HeartAndSoul);

    // The fixture's first slot has counter 100 and its second 101.
    private const int NewerSlot = 14;

    private static Session Loaded(byte[]? bytes = null)
    {
        var session = new Session();
        Value(Dispatch(session, "game.load", Args(Convert.ToBase64String(bytes ?? Fixture), "heartandsoul.sav", null!)));
        return session;
    }

    private static byte[] Exported(Session session) =>
        Convert.FromBase64String(Value(Dispatch(session, "game.export", "[]"))!["bytes"]!.GetValue<string>());

    private static JsonNode Details(Session session, PokemonHandle at) => Value(Dispatch(session, "pokemon.details", Args(at)))!;

    private static string Update(Session session, PokemonHandle at, object patch) =>
        Dispatch(session, "pokemon.update", Args(at, patch));

    private static string[] MoveNames(JsonNode details) =>
        details["moves"]!.AsArray().Select(move => move!["name"]!.GetValue<string>()).ToArray();

    private static int Item(string name) => Facade.Repositories.ItemRepository.GetItemByName(name)!.Id;

    [Fact]
    public void LoadsAsHeartAndSoul()
    {
        var session = Loaded();

        session.Game!.SaveFile.Should().BeOfType<HeartAndSoulSave>();
        session.Game.SaveFile.ChecksumsValid.Should().BeTrue();
        Value(Dispatch(session, "game.get", "[]"))!["format"]!["id"]!.GetValue<string>().Should().Be("heart-and-soul");
    }

    [Fact]
    public void ListsTheParty() =>
        Value(Dispatch(Loaded(), "party.get", "[]"))!.AsArray()
            .Select(p => (p!["species"]!.GetValue<string>(), p["level"]!.GetValue<int>()))
            .Should().Equal(("Gastly", 31), ("Snubbull", 31), ("Paras", 31), ("Munchlax", 31), ("Hoothoot", 31), ("Poliwag", 31));

    [Fact]
    public void ListsThe14Boxes()
    {
        var session = Loaded();

        var boxed = Value(Dispatch(session, "box.get", "[]"))!.AsArray();

        session.Game!.SaveFile.BoxCount.Should().Be(14);
        boxed.GroupBy(p => p!["at"]!["box"]!.GetValue<int>()).Select(box => (box.Key, box.Count()))
            .Should().Equal((0, 25), (13, 2));
    }

    [Theory]
    [InlineData(-1, 0, Species.Gastly, "Gaster", new[] { "Hypnosis", "Thunderbolt", "Curse", "Shadow Ball" })]
    [InlineData(0, 0, Species.Goldeen, "Goldster", new[] { "Flail", "Water Pulse", "Aqua Ring", "Horn Attack" })]
    [InlineData(0, 12, Species.Vulpix, "Vulster", new[] { "Baby-Doll Eyes", "Quick Attack", "Confuse Ray", "Fire Spin" })]
    [InlineData(13, 1, Species.Mareep, "Marster", new[] { "Charge", "Growl", "Thunder Wave", "Thunder Shock" })]
    public void ReadsThePokemon(int box, int slot, Species species, string nickname, string[] moves)
    {
        var details = Details(Loaded(), box < 0 ? PokemonHandle.Party(slot) : PokemonHandle.InBox(box, slot));

        details["species"]!.GetValue<int>().Should().Be((int)species);
        details["nickname"]!.GetValue<string>().Should().Be(nickname);
        details["heldItem"]!.GetValue<int>().Should().Be(0);
        MoveNames(details).Should().Equal(moves);
    }

    [Fact]
    public void ExportingWithoutEditsKeepsEveryByte() =>
        Exported(Loaded()).Should().Equal(Fixture);

    [Theory]
    [MemberData(nameof(EditedPokemon))]
    public void EditsSurviveExportAndReload(PokemonHandle at)
    {
        var session = Loaded();
        var patch = new
        {
            level = 70,
            moves = new[] { (int)Move.Thunderbolt, (int)Move.Surf, (int)Move.IceBeam, (int)Move.Psychic },
            heldItem = Item("Leftovers"),
        };

        Value(Update(session, at, patch));

        var reloaded = Loaded(Exported(session));
        reloaded.Game!.SaveFile.ChecksumsValid.Should().BeTrue();
        var details = Details(reloaded, at);
        details["level"]!.GetValue<int>().Should().Be(70);
        MoveNames(details).Should().Equal("Thunderbolt", "Surf", "Ice Beam", "Psychic");
        details["heldItem"]!.GetValue<int>().Should().Be(Item("Leftovers"));
    }

    public static TheoryData<PokemonHandle> EditedPokemon => new() { PokemonHandle.Party(0), PokemonHandle.InBox(0, 8) };

    [Theory]
    [MemberData(nameof(EditedSlots))]
    public void AnEditChangesNoByteOutsideThePokemonsSlotAndTheChecksums(PokemonHandle at, int sectorId, int offset, int size)
    {
        var session = Loaded();

        Value(Update(session, at, new { level = 70, moves = new[] { (int)Move.Surf, 0, 0, 0 }, heldItem = Item("Leftovers") }));

        var exported = Exported(session);
        var owned = new HashSet<int>(Enumerable.Range(NewerSlot, 14).Select(sector => (sector * 0x1000) + 0xFF6).SelectMany(checksum => new[] { checksum, checksum + 1 }));
        var slot = SectorOf(sectorId) + offset;
        owned.UnionWith(Enumerable.Range(slot, size));
        Enumerable.Range(0, Fixture.Length).Where(index => exported[index] != Fixture[index])
            .Should().NotBeEmpty().And.OnlyContain(index => owned.Contains(index));
    }

    // Party slot 0 sits at SaveBlock1 0x23C. Box 1 slot 9 is the storage's 4 + 8 * 80, which stays inside storage sector 5.
    public static TheoryData<PokemonHandle, int, int, int> EditedSlots => new()
    {
        { PokemonHandle.Party(0), 1, 0x23C, 100 },
        { PokemonHandle.InBox(0, 8), 5, 4 + (8 * 80), 80 },
    };

    private static int SectorOf(int id) =>
        Enumerable.Range(NewerSlot, 14).Single(sector => ReadUInt16LittleEndian(Fixture.AsSpan((sector * 0x1000) + 0xFF4)) == id) * 0x1000;

    [Fact]
    public void ShowsTheTrainer()
    {
        var trainer = Value(Dispatch(Loaded(), "trainer.get", "[]"))!;

        trainer["name"]!.GetValue<string>().Should().Be("Pukster");
        trainer["gender"]!.GetValue<string>().Should().Be("male");
        trainer["id"]!.GetValue<string>().Should().Be("18233/57075");
        trainer["money"]!.GetValue<uint>().Should().Be(1026739);
    }

    [Fact]
    public void AnEditedTrainerSurvivesExportAndChangesNoOtherByte()
    {
        var session = Loaded();

        Value(Dispatch(session, "trainer.setName", Args("Gold")));
        Value(Dispatch(session, "trainer.setGender", Args("female")));
        Value(Dispatch(session, "trainer.setMoney", Args(424242)));

        var exported = Exported(session);
        var money = SectorOf(1) + 0x494;
        var key = ReadUInt32LittleEndian(Fixture.AsSpan(SectorOf(0) + 0xAC));
        ReadUInt32LittleEndian(exported.AsSpan(money)).Should().Be(424242 ^ key);
        var owned = new HashSet<int>(Enumerable.Range(NewerSlot, 14).Select(sector => (sector * 0x1000) + 0xFF6).SelectMany(checksum => new[] { checksum, checksum + 1 }));
        owned.UnionWith(Enumerable.Range(SectorOf(0), 9));
        owned.UnionWith(Enumerable.Range(money, 4));
        Enumerable.Range(0, Fixture.Length).Where(index => exported[index] != Fixture[index])
            .Should().NotBeEmpty().And.OnlyContain(index => owned.Contains(index));

        var reloaded = Loaded(exported);
        reloaded.Game!.SaveFile.ChecksumsValid.Should().BeTrue();
        var trainer = Value(Dispatch(reloaded, "trainer.get", "[]"))!;
        trainer["name"]!.GetValue<string>().Should().Be("Gold");
        trainer["gender"]!.GetValue<string>().Should().Be("female");
        trainer["money"]!.GetValue<uint>().Should().Be(424242);
    }

    [Fact]
    public void TheFormatIsListed() =>
        Value(Dispatch(new Session(), "game.formats", "[]"))!.AsArray()
            .Select(format => format!["id"]!.GetValue<string>()).Should().Contain("heart-and-soul");
}
