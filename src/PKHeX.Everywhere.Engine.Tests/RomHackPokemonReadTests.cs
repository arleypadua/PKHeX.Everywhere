using System.Text.Json.Nodes;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.RomHacks.Cfru.Unbound;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class RomHackPokemonReadTests
{
    private const ushort ShadowWarrior = 706;
    private const int ShadowWarriorId = 0xF000 + ShadowWarrior;
    private const int ShadowWarriorSlot = (22 * 30) + 18;

    public static TheoryData<string, string, int> RomHacks => new()
    {
        { SaveFilePath.Unbound, "unbound", (int)GameVersion.FR },
        { SaveFilePath.RadicalRed, "radicalred", (int)GameVersion.FR },
        { SaveFilePath.Imperium, "emerald-imperium", (int)GameVersion.E },
    };

    [Theory]
    [MemberData(nameof(RomHacks))]
    public void ReadsEachPartyPokemonAsItsSaveDetailsIt(string path, string formatId, int version)
    {
        var session = new Session();
        Value(Dispatch(session, "game.load", Args(Convert.ToBase64String(File.ReadAllBytes(path)), "hack.sav", formatId)));
        var party = session.Game!.SaveFile.PartyData;

        party.Should().NotBeEmpty();
        for (var slot = 0; slot < party.Count; slot++)
        {
            var details = Value(Dispatch(session, "pokemon.details", Args(PokemonHandle.Party(slot))))!;

            Read(PartyBytes(party[slot]), version, formatId).ToJsonString().Should().Be(details.ToJsonString());
        }
    }

    [Fact]
    public void ReadsAShadowWarriorWithItsHackIndexAndNamesIt()
    {
        var save = new UnboundSave(File.ReadAllBytes(SaveFilePath.UnboundUnknownSpecies));
        save.SetPartySlotAtIndex(save.GetBoxSlotAtIndex(ShadowWarriorSlot), save.PartyCount);

        var read = Read(PartyBytes(save.GetPartySlotAtIndex(save.PartyCount - 1)), (int)GameVersion.FR, "unbound");

        read["speciesIndex"]!.GetValue<int>().Should().Be(ShadowWarrior);
        read["species"]!.GetValue<int>().Should().Be(ShadowWarriorId);
        Names(new { speciesIds = new[] { ShadowWarriorId }, itemIds = Array.Empty<int>(), formatId = "unbound" })["species"]!.ToJsonString()
            .Should().Be($$"""[{"id":{{ShadowWarriorId}},"name":"Shadow Warrior"}]""");
    }

    [Fact]
    public void NamesWithoutAFormatLeaveOutAHackSpecies() =>
        Names(new { speciesIds = new[] { ShadowWarriorId }, itemIds = Array.Empty<int>() })["species"]!.AsArray().Should().BeEmpty();

    [Fact]
    public void NamesAHackItemThroughItsFormat()
    {
        var session = new Session();
        Value(Dispatch(session, "game.load", Args(Convert.ToBase64String(File.ReadAllBytes(SaveFilePath.Unbound)), "unbound.sav", "unbound")));
        var heldItem = session.Game!.Trainer.PokemonBox.All[(19 * 30) + 27].HeldItem;

        Names(new { speciesIds = Array.Empty<int>(), itemIds = new[] { (int)heldItem.Id }, formatId = "unbound" })["items"]!.ToJsonString()
            .Should().Be($$"""[{"id":{{heldItem.Id}},"name":"{{heldItem.Name}}"}]""");
        heldItem.IsUnknown.Should().BeTrue();
    }

    [Theory]
    [InlineData(100, (int)GameVersion.E, "unbound", "bad-arguments")]
    [InlineData(100, (int)GameVersion.FRLG, "unbound", "bad-arguments")]
    [InlineData(80, (int)GameVersion.FR, "unbound", "bad-arguments")]
    [InlineData(100, (int)GameVersion.FR, "unbound", "bad-checksum")]
    [InlineData(100, (int)GameVersion.E, "emerald-imperium", "bad-checksum")]
    [InlineData(100, (int)GameVersion.FR, "no-such-format", "not-found")]
    public void ReadFailsWithTheCodeOfTheProblem(int length, int version, string formatId, string code) =>
        Error(Dispatch(new Session(), "pokemon.read", Args(Convert.ToBase64String(new byte[length]), version, formatId))).Should().Be(code);

    [Fact]
    public void NamesFailWithAnUnknownFormat() =>
        Error(Dispatch(new Session(), "catalog.names", Args(new { speciesIds = new[] { 25 }, itemIds = Array.Empty<int>(), formatId = "no-such-format" })))
            .Should().Be("not-found");

    private static string PartyBytes(PKM pokemon)
    {
        var bytes = new byte[pokemon.SIZE_PARTY];
        pokemon.WriteEncryptedDataParty(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static JsonNode Read(string bytes, int version, string formatId) =>
        Value(Dispatch(new Session(), "pokemon.read", Args(bytes, version, formatId)))!;

    private static JsonNode Names(object request) => Value(Dispatch(new Session(), "catalog.names", Args(request)))!;
}
