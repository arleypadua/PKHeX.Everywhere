using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Tests;

public class PokemonReadTests
{
    [Theory]
    [InlineData(SaveFilePath.Emerald, GameVersion.E, true)]
    [InlineData(SaveFilePath.Emerald, GameVersion.E, false)]
    [InlineData(SaveFilePath.FireRed, GameVersion.FR, true)]
    [InlineData(SaveFilePath.HgSs, GameVersion.SS, true)]
    [InlineData(SaveFilePath.HgSs, GameVersion.SS, false)]
    public void ReadsTheDetailsOfAnEncryptedPokemon(string saveFile, GameVersion version, bool party)
    {
        var pokemon = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0];

        var details = Pokemon.Read(Encrypted(pokemon.Pkm, party), (int)version);

        details.Should().BeEquivalentTo(pokemon.Details() with { Legality = null });
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald, GameVersion.E)]
    [InlineData(SaveFilePath.HgSs, GameVersion.SS)]
    public void ReadsAPartyPokemonAtTheLevelItsGameStores(string saveFile, GameVersion version)
    {
        var pkm = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0].Pkm;
        var stored = (byte)((pkm.CurrentLevel % 100) + 1);
        pkm.Stat_Level = stored;

        Pokemon.Read(Encrypted(pkm, party: true), (int)version).Level.Should().Be(stored);
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald, GameVersion.E)]
    [InlineData(SaveFilePath.HgSs, GameVersion.SS)]
    public void ReadsABoxPokemonAtTheLevelItsExperienceGives(string saveFile, GameVersion version)
    {
        var pkm = Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0].Pkm;
        var level = pkm.CurrentLevel;
        pkm.Stat_Level = (byte)((level % 100) + 1);

        Pokemon.Read(Encrypted(pkm, party: false), (int)version).Level.Should().Be(level);
    }

    [Theory]
    [InlineData(SaveFilePath.Emerald, GameVersion.E, true)]
    [InlineData(SaveFilePath.Emerald, GameVersion.E, false)]
    [InlineData(SaveFilePath.HgSs, GameVersion.SS, true)]
    [InlineData(SaveFilePath.HgSs, GameVersion.SS, false)]
    public void RejectsACorruptedByte(string saveFile, GameVersion version, bool party)
    {
        var bytes = Encrypted(Game.LoadFrom(saveFile).Trainer.Party.Pokemons[0].Pkm, party);
        bytes[0x30] ^= 0xFF;

        Failure(() => Pokemon.Read(bytes, (int)version)).Should().Be(UnreadableReason.BadChecksum);
    }

    [Theory]
    [InlineData(GameVersion.E, 80)]
    [InlineData(GameVersion.E, 100)]
    [InlineData(GameVersion.Pt, 136)]
    [InlineData(GameVersion.Pt, 236)]
    public void RejectsAllZeroBytes(GameVersion version, int length) =>
        Failure(() => Pokemon.Read(new byte[length], (int)version)).Should().Be(UnreadableReason.BadChecksum);

    [Fact]
    public void RejectsAGen3BadEgg()
    {
        var bytes = Encrypted(Game.LoadFrom(SaveFilePath.Emerald).Trainer.Party.Pokemons[0].Pkm, party: true);
        bytes[0x13] |= 1;

        Failure(() => Pokemon.Read(bytes, (int)GameVersion.E)).Should().Be(UnreadableReason.BadChecksum);
    }

    [Theory]
    [InlineData(GameVersion.FRLG)]
    [InlineData(GameVersion.HGSS)]
    public void RejectsACombinedVersion(GameVersion version) =>
        Failure(() => Pokemon.Read(new byte[100], (int)version)).Should().Be(UnreadableReason.CombinedVersion);

    [Theory]
    [InlineData(GameVersion.E, 136)]
    [InlineData(GameVersion.E, 99)]
    [InlineData(GameVersion.SS, 100)]
    [InlineData(GameVersion.SS, 0)]
    public void RejectsALengthThatDoesNotFitTheVersion(GameVersion version, int length) =>
        Failure(() => Pokemon.Read(new byte[length], (int)version)).Should().Be(UnreadableReason.WrongLength);

    [Theory]
    [InlineData((int)GameVersion.C)]
    [InlineData((int)GameVersion.B)]
    [InlineData((int)GameVersion.SWSH)]
    [InlineData((int)GameVersion.CXD)]
    [InlineData((int)GameVersion.BATREV)]
    [InlineData(-1)]
    [InlineData(256 + (int)GameVersion.E)]
    public void RejectsAVersionOutsideGen3And4(int version) =>
        Failure(() => Pokemon.Read(new byte[100], version)).Should().Be(UnreadableReason.UnsupportedVersion);

    private static byte[] Encrypted(PKM pkm, bool party)
    {
        var bytes = new byte[party ? pkm.SIZE_PARTY : pkm.SIZE_STORED];
        if (party) pkm.WriteEncryptedDataParty(bytes);
        else pkm.WriteEncryptedDataStored(bytes);
        return bytes;
    }

    private static UnreadableReason Failure(Action read) =>
        read.Should().Throw<UnreadablePokemonException>().Which.Reason;
}
