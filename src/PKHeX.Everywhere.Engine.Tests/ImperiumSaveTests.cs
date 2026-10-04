using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.RomHacks.Expansion.Imperium;
using PKHeX.Facade.Tests.Base;
using static System.Buffers.Binary.BinaryPrimitives;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class ImperiumSaveTests
{
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.Imperium);

    private const ushort MegaLuxray = 1489;
    private const int Luxrite = 975;

    public static TheoryData<string, byte[]> Saves => new()
    {
        { "as found", Fixture },
        { "readable as Emerald", SaveFilePath.ImperiumReadableAsEmerald() },
    };

    private static Session Loaded(byte[]? bytes = null)
    {
        var session = new Session();
        Value(Dispatch(session, "game.load", Args(Convert.ToBase64String(bytes ?? Fixture), "imperium.sav", null!)));
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

    private static byte[] FixtureWith(Action<ImperiumSave> change)
    {
        var save = new ImperiumSave(Fixture.ToArray());
        change(save);
        return save.Write().ToArray();
    }

    [Theory]
    [MemberData(nameof(Saves))]
    public void LoadsAsImperiumWithoutAFormat(string _, byte[] save)
    {
        var session = Loaded(save);

        session.Game!.SaveFile.Should().BeOfType<ImperiumSave>();
        session.Game.SaveFile.ChecksumsValid.Should().BeTrue();
        Value(Dispatch(session, "game.get", "[]"))!["format"]!["id"]!.GetValue<string>().Should().Be("emerald-imperium");
    }

    [Fact]
    public void ListsTheParty() =>
        Value(Dispatch(Loaded(), "party.get", "[]"))!.AsArray()
            .Select(p => (p!["species"]!.GetValue<string>(), p["level"]!.GetValue<int>()))
            .Should().Equal(("Emboar", 56), ("Marowak", 56), ("Toxapex", 56), ("Kingambit", 56), ("Bronzong", 56), ("Slowbro", 56));

    [Fact]
    public void ListsThe14Boxes()
    {
        var session = Loaded();

        var boxed = Value(Dispatch(session, "box.get", "[]"))!.AsArray();

        session.Game!.SaveFile.BoxCount.Should().Be(14);
        boxed.GroupBy(p => p!["at"]!["box"]!.GetValue<int>()).Select(box => (box.Key, box.Count()))
            .Should().Equal((0, 24), (13, 3));
    }

    [Theory]
    [InlineData(-1, 1, Species.Marowak, 1, "Marowak", "Thick Club", new[] { "Flame Wheel", "Shadow Bone", "Bonemerang", "Fling" })]
    [InlineData(-1, 3, Species.Kingambit, 0, "Kingambit", "Eviolite", new[] { "Iron Head", "Sucker Punch", "Kowtow Cleave", "Brick Break" })]
    [InlineData(0, 8, Species.Rapidash, 1, "Rapidash", null, new[] { "Heal Pulse", "Will-O-Wisp", "Covet", "Fire Lash" })]
    [InlineData(0, 12, Species.Meowscarada, 0, "Meowscarada", null, new[] { "Night Slash", "Tail Whip", "Leafage", "Bite" })]
    [InlineData(0, 19, Species.Aegislash, 0, "Aegislash", null, new[] { "King’s Shield", "Night Slash", "Smart Strike", "Retaliate" })]
    [InlineData(13, 2, Species.Delibird, 0, "Delibird", null, new[] { "Present", "(None)", "(None)", "(None)" })]
    public void ReadsThePokemon(int box, int slot, Species species, int form, string nickname, string? heldItem, string[] moves)
    {
        var details = Details(Loaded(), box < 0 ? PokemonHandle.Party(slot) : PokemonHandle.InBox(box, slot));

        details["species"]!.GetValue<int>().Should().Be((int)species);
        details["form"]!.GetValue<int>().Should().Be(form);
        details["nickname"]!.GetValue<string>().Should().Be(nickname);
        details["heldItem"]!.GetValue<int>().Should().Be(heldItem is null ? 0 : Item(heldItem));
        MoveNames(details).Should().Equal(moves);
    }

    [Fact]
    public void AnItemOnlyImperiumHasIsUnknown()
    {
        var details = Details(Loaded(), PokemonHandle.InBox(0, 1));

        details["heldItem"]!.GetValue<int>().Should().Be(0xF000 + Luxrite);
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

        Value(Update(session, at, new { level = 70, heldItem = Item("Leftovers") }));

        var exported = Exported(session);
        var owned = new HashSet<int>(Enumerable.Range(0, 28).Select(sector => (sector * 0x1000) + 0xFF6).SelectMany(checksum => new[] { checksum, checksum + 1 }));
        var slot = SectorOf(sectorId) + offset;
        owned.UnionWith(Enumerable.Range(slot, size));
        Enumerable.Range(0, Fixture.Length).Where(index => exported[index] != Fixture[index])
            .Should().NotBeEmpty().And.OnlyContain(index => owned.Contains(index));
    }

    // Party slot 0 sits at SaveBlock1 0x238. Box 1 slot 9 is the storage's 4 + 8 * 80, which stays inside storage sector 17.
    public static TheoryData<PokemonHandle, int, int, int> EditedSlots => new()
    {
        { PokemonHandle.Party(0), 1, 0x238, 100 },
        { PokemonHandle.InBox(0, 8), 17, 4 + (8 * 80), 80 },
    };

    private static int SectorOf(int id) =>
        Enumerable.Range(0, 28).Single(sector => ReadUInt16LittleEndian(Fixture.AsSpan((sector * 0x1000) + 0xFF4)) == id) * 0x1000;

    [Fact]
    public void AFormOnlyImperiumHasShowsByNameAndCantBeEdited()
    {
        var bytes = FixtureWith(save =>
        {
            var pokemon = (ImperiumPokemon)save.GetBoxSlotAtIndex(0);
            pokemon.SpeciesIndex = MegaLuxray;
            save.SetBoxSlotAtIndex(pokemon, 0);
        });
        var session = Loaded(bytes);
        var at = PokemonHandle.InBox(0, 0);

        Value(Dispatch(session, "box.get", "[]"))!.AsArray()
            .Single(p => p!["at"]!["box"]!.GetValue<int>() == 0 && p["at"]!["slot"]!.GetValue<int>() == 0)!["species"]!
            .GetValue<string>().Should().Be("Mega Luxray");
        Error(Update(session, at, new { nickname = "Renamed" })).Should().Be("unknown-species");
        Error(Dispatch(session, "pokemon.setLevel", Args(at, 60))).Should().Be("unknown-species");

        Exported(session).Should().Equal(bytes);
    }

    [Fact]
    public void TheFormatIsListed() =>
        Value(Dispatch(new Session(), "game.formats", "[]"))!.AsArray()
            .Select(format => format!["id"]!.GetValue<string>()).Should().Contain("emerald-imperium");
}
