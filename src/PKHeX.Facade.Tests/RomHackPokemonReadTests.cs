using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Tests;

public class RomHackPokemonReadTests
{
    [Theory]
    [InlineData(SaveFilePath.Unbound, "unbound")]
    [InlineData(SaveFilePath.RadicalRed, "radicalred")]
    [InlineData(SaveFilePath.Imperium, "emerald-imperium")]
    public void ReadsEachPartyPokemonAsItsSaveDetailsIt(string saveFile, string formatId)
    {
        var format = SaveFormats.Find(formatId)!;
        var party = Game.LoadFrom(File.ReadAllBytes(saveFile), saveFile, format).Trainer.Party.Pokemons;

        party.Should().NotBeEmpty().And.AllSatisfy(pokemon =>
            Pokemon.Read(PartyBytes(pokemon.Pkm), (int)format.BaseGame, format).Should().BeEquivalentTo(pokemon.Details()));
    }

    [Theory]
    [InlineData(SaveFilePath.Unbound, "unbound")]
    [InlineData(SaveFilePath.Imperium, "emerald-imperium")]
    public void ReadsAPartyPokemonAtTheLevelItsGameStores(string saveFile, string formatId)
    {
        var format = SaveFormats.Find(formatId)!;
        var pkm = Game.LoadFrom(File.ReadAllBytes(saveFile), saveFile, format).Trainer.Party.Pokemons[0].Pkm;
        var stored = (byte)((pkm.CurrentLevel % 100) + 1);
        pkm.Stat_Level = stored;

        Pokemon.Read(PartyBytes(pkm), (int)format.BaseGame, format).Level.Should().Be(stored);
    }

    [Fact]
    public void RejectsACorruptedImperiumByte()
    {
        var format = SaveFormats.Find("emerald-imperium")!;
        var bytes = PartyBytes(Game.LoadFrom(SaveFilePath.Imperium).Trainer.Party.Pokemons[0].Pkm);
        bytes[0x30] ^= 0xFF;

        Failure(() => Pokemon.Read(bytes, (int)GameVersion.E, format)).Should().Be(UnreadableReason.BadChecksum);
    }

    [Theory]
    [InlineData("unbound", GameVersion.FR)]
    [InlineData("radicalred", GameVersion.FR)]
    [InlineData("emerald-imperium", GameVersion.E)]
    public void RejectsAllZeroBytes(string formatId, GameVersion version) =>
        Failure(() => Pokemon.Read(new byte[100], (int)version, SaveFormats.Find(formatId)!)).Should().Be(UnreadableReason.BadChecksum);

    [Theory]
    [InlineData(GameVersion.E)]
    [InlineData(GameVersion.LG)]
    [InlineData(GameVersion.FRLG)]
    public void RejectsAVersionOtherThanTheBaseGame(GameVersion version) =>
        Failure(() => Pokemon.Read(new byte[100], (int)version, SaveFormats.Find("unbound")!)).Should().Be(UnreadableReason.NotTheBaseGame);

    [Theory]
    [InlineData(80)]
    [InlineData(58)]
    [InlineData(0)]
    public void RejectsBytesOutsideThePartyLayout(int length) =>
        Failure(() => Pokemon.Read(new byte[length], (int)GameVersion.FR, SaveFormats.Find("unbound")!)).Should().Be(UnreadableReason.WrongLength);

    [Fact]
    public void RejectsAFormatWithoutAnEmptySave() =>
        Failure(() => Pokemon.Read(new byte[100], (int)GameVersion.E, SaveFormats.PKHeX)).Should().Be(UnreadableReason.UnsupportedFormat);

    [Fact]
    public void AnEmptySaveNamesTheHacksOwnSpecies()
    {
        var species = Game.EmptyOf(SaveFormats.Find("unbound")!)!.SpeciesRepository;

        species.FindKnown(0xF000 + 706)!.Name.Should().Be("Shadow Warrior");
        species.FindKnown((ushort)Species.Pikachu)!.Name.Should().Be("Pikachu");
        species.FindKnown(0).Should().BeNull();
    }

    private static byte[] PartyBytes(PKM pkm)
    {
        var bytes = new byte[pkm.SIZE_PARTY];
        pkm.WriteEncryptedDataParty(bytes);
        return bytes;
    }

    private static UnreadableReason Failure(Action read) =>
        read.Should().Throw<UnreadablePokemonException>().Which.Reason;
}
