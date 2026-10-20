using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Tests;

public class PokemonReadTests
{
    [Theory]
    [InlineData(GameVersion.E, true)]
    [InlineData(GameVersion.E, false)]
    [InlineData(GameVersion.FR, true)]
    [InlineData(GameVersion.SS, true)]
    [InlineData(GameVersion.SS, false)]
    [InlineData(GameVersion.B2, true)]
    [InlineData(GameVersion.B2, false)]
    public void ReadsTheDetailsOfAnEncryptedPokemon(GameVersion version, bool party)
    {
        var pokemon = Lead(version);

        var details = Pokemon.Read(Encrypted(pokemon.Pkm, party), (int)version);

        details.Should().BeEquivalentTo(pokemon.Details() with { Legality = null });
    }

    [Theory]
    [InlineData(GameVersion.E)]
    [InlineData(GameVersion.SS)]
    [InlineData(GameVersion.B2)]
    public void ReadsAPartyPokemonAtTheLevelItsGameStores(GameVersion version)
    {
        var pkm = Lead(version).Pkm;
        var stored = (byte)((pkm.CurrentLevel % 100) + 1);
        pkm.Stat_Level = stored;

        Pokemon.Read(Encrypted(pkm, party: true), (int)version).Level.Should().Be(stored);
    }

    [Theory]
    [InlineData(GameVersion.E)]
    [InlineData(GameVersion.SS)]
    [InlineData(GameVersion.B2)]
    public void ReadsABoxPokemonAtTheLevelItsExperienceGives(GameVersion version)
    {
        var pkm = Lead(version).Pkm;
        var level = pkm.CurrentLevel;
        pkm.Stat_Level = (byte)((level % 100) + 1);

        Pokemon.Read(Encrypted(pkm, party: false), (int)version).Level.Should().Be(level);
    }

    [Theory]
    [InlineData(GameVersion.E, true)]
    [InlineData(GameVersion.E, false)]
    [InlineData(GameVersion.SS, true)]
    [InlineData(GameVersion.SS, false)]
    [InlineData(GameVersion.B2, true)]
    [InlineData(GameVersion.B2, false)]
    public void RejectsACorruptedByte(GameVersion version, bool party)
    {
        var bytes = Encrypted(Lead(version).Pkm, party);
        bytes[0x30] ^= 0xFF;

        Failure(() => Pokemon.Read(bytes, (int)version)).Should().Be(UnreadableReason.BadChecksum);
    }

    [Theory]
    [InlineData(GameVersion.E, 80)]
    [InlineData(GameVersion.E, 100)]
    [InlineData(GameVersion.Pt, 136)]
    [InlineData(GameVersion.Pt, 236)]
    [InlineData(GameVersion.B, 136)]
    [InlineData(GameVersion.W2, 220)]
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
    [InlineData(GameVersion.BW)]
    [InlineData(GameVersion.B2W2)]
    [InlineData(GameVersion.CXD)]
    public void RejectsACombinedVersion(GameVersion version) =>
        Failure(() => Pokemon.Read(new byte[100], (int)version)).Should().Be(UnreadableReason.CombinedVersion);

    [Theory]
    [InlineData(GameVersion.E, 136)]
    [InlineData(GameVersion.E, 99)]
    [InlineData(GameVersion.SS, 100)]
    [InlineData(GameVersion.SS, 0)]
    [InlineData(GameVersion.W, 236)]
    [InlineData(GameVersion.B2, 100)]
    public void RejectsALengthThatDoesNotFitTheVersion(GameVersion version, int length) =>
        Failure(() => Pokemon.Read(new byte[length], (int)version)).Should().Be(UnreadableReason.WrongLength);

    [Theory]
    [InlineData((int)GameVersion.C)]
    [InlineData((int)GameVersion.X)]
    [InlineData((int)GameVersion.SWSH)]
    [InlineData((int)GameVersion.BATREV)]
    [InlineData(-1)]
    [InlineData(256 + (int)GameVersion.E)]
    public void RejectsAVersionOutsideGen3To5(int version) =>
        Failure(() => Pokemon.Read(new byte[100], version)).Should().Be(UnreadableReason.UnsupportedVersion);

    [Theory]
    [InlineData(GameVersion.COLO)]
    [InlineData(GameVersion.XD)]
    public void ReadsAColosseumOrXDPokemonAsItsGameStoresIt(GameVersion version)
    {
        var pokemon = GameCubeLead(version);

        var details = Pokemon.Read(pokemon.ToFile().Bytes, (int)version);

        details.Should().BeEquivalentTo(new
        {
            Species = (int)Species.Makuhita,
            Level = 31,
            Nature = (int)Nature.Adamant,
            Ability = (int)Ability.ThickFat,
        });
        details.Moves.Select(m => m.Id).Should().Equal((int)Move.Tackle, (int)Move.BulkUp, 0, 0);
    }

    [Fact]
    public void ReadsTheShadowFieldsOfAColosseumPokemon() =>
        Pokemon.Read(GameCubeLead(GameVersion.COLO).ToFile().Bytes, (int)GameVersion.COLO)
            .Should().BeEquivalentTo(new { IsShadow = true, ShadowId = 10, Purification = 3000 });

    [Fact]
    public void ReadsAnXDShadowPokemonWithoutTheStateItsSaveKeeps() =>
        Pokemon.Read(GameCubeLead(GameVersion.XD).ToFile().Bytes, (int)GameVersion.XD)
            .Should().BeEquivalentTo(new { IsShadow = (bool?)null, ShadowId = 10, Purification = (int?)null });

    [Theory]
    [InlineData(GameVersion.COLO, 312)]
    [InlineData(GameVersion.XD, 196)]
    public void RejectsAllZeroColosseumOrXDBytes(GameVersion version, int length) =>
        Failure(() => Pokemon.Read(new byte[length], (int)version)).Should().Be(UnreadableReason.BadChecksum);

    [Theory]
    [InlineData(GameVersion.COLO)]
    [InlineData(GameVersion.XD)]
    public void RejectsAColosseumOrXDSpeciesOutOfRange(GameVersion version)
    {
        var bytes = GameCubeLead(version).ToFile().Bytes;
        bytes[0] = 0xFF;
        bytes[1] = 0xFF;

        Failure(() => Pokemon.Read(bytes, (int)version)).Should().Be(UnreadableReason.BadChecksum);
    }

    [Theory]
    [InlineData(GameVersion.COLO, 196)]
    [InlineData(GameVersion.XD, 312)]
    [InlineData(GameVersion.XD, 100)]
    public void RejectsALengthThatDoesNotFitColosseumOrXD(GameVersion version, int length) =>
        Failure(() => Pokemon.Read(new byte[length], (int)version)).Should().Be(UnreadableReason.WrongLength);

    private static Pokemon GameCubeLead(GameVersion version)
    {
        var game = Game.EmptyOf(GameVersionRepository.Instance.Get(version));
        IShadowCapture shadow = version == GameVersion.COLO ? new CK3() : new XK3();
        var pkm = (PKM)shadow;
        pkm.Species = (ushort)Species.Makuhita;
        pkm.PID = 3;
        pkm.AbilityNumber = 1;
        pkm.Move1 = (ushort)Move.Tackle;
        pkm.Move2 = (ushort)Move.BulkUp;
        pkm.CurrentLevel = 30;
        pkm.ResetPartyStats();
        pkm.Stat_Level = 31;
        shadow.ShadowID = 10;
        shadow.Purification = 3000;
        return new Pokemon(pkm, game);
    }

    private static Pokemon Lead(GameVersion version) => version switch
    {
        GameVersion.B2 => Gen5Lead(),
        _ => Game.LoadFrom(SaveFilePath.PathFrom(version)).Trainer.Party.Pokemons[0],
    };

    private static Pokemon Gen5Lead()
    {
        var game = Game.EmptyOf(GameVersionRepository.Instance.Get(GameVersion.B2));
        var pkm = new PK5
        {
            Species = (ushort)Species.Oshawott,
            PID = 0x1234_5678,
            ID32 = 0x0BAD_F00D,
            OriginalTrainerName = "ASH",
            Version = GameVersion.B2,
            Language = (int)LanguageID.English,
            Ball = (byte)Ball.Poke,
            HeldItem = 1,
            Move1 = (ushort)Move.Tackle,
            Move2 = (ushort)Move.WaterGun,
            IV_HP = 31,
            EV_SPA = 100,
        };
        pkm.SetDefaultNickname();
        pkm.CurrentLevel = 23;
        pkm.ResetPartyStats();
        pkm.RefreshChecksum();
        return new Pokemon(pkm, game);
    }

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
