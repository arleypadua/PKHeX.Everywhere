using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Expansion.HeartAndSoul;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade.Tests;

public class HeartAndSoulSaveTests
{
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.HeartAndSoul);

    private const int Goldeen = 0;

    private static Game Load(byte[]? bytes = null) => Game.LoadFrom(bytes ?? Fixture.ToArray(), SaveFilePath.HeartAndSoul);

    private static Pokemon At(Game game, int index) => game.Trainer.PokemonBox.All[index];

    [Fact]
    public void LoadsWithTheFormatOn()
    {
        var game = Load();

        game.SaveFile.Should().BeOfType<HeartAndSoulSave>();
        game.Format!.Id.Should().Be("heart-and-soul");
        game.Trainer.Party.Pokemons.Select(pokemon => pokemon.Species.Name)
            .Should().Equal("Gastly", "Snubbull", "Paras", "Munchlax", "Hoothoot", "Poliwag");
    }

    [Fact]
    public void TheBoxesHold27Pokemon()
    {
        var save = Load().SaveFile;

        save.BoxCount.Should().Be(14);
        save.BoxData.Count(pokemon => pokemon.Species != 0).Should().Be(27);
    }

    [Theory]
    [InlineData(0, Species.Goldeen, "Goldster", new[] { Move.Flail, Move.WaterPulse, Move.AquaRing, Move.HornAttack })]
    [InlineData(12, Species.Vulpix, "Vulster", new[] { Move.BabyDollEyes, Move.QuickAttack, Move.ConfuseRay, Move.FireSpin })]
    [InlineData((13 * 30) + 1, Species.Mareep, "Marster", new[] { Move.Charge, Move.Growl, Move.ThunderWave, Move.ThunderShock })]
    public void ReadsTheBoxedPokemon(int index, Species species, string nickname, Move[] moves)
    {
        var pkm = At(Load(), index).Pkm;

        pkm.Species.Should().Be((ushort)species);
        pkm.Nickname.Should().Be(nickname);
        pkm.HeldItem.Should().Be(0);
        pkm.Moves.Should().Equal(moves.Select(move => (ushort)move));
    }

    // Heart & Soul's ids from 1489 differ from Imperium's.
    [Theory]
    [InlineData(1488, Species.Eevee, 1)]
    [InlineData(1492, Species.Butterfree, 0)]
    [InlineData(1523, Species.Mimikyu, 3)]
    [InlineData(1524, Species.Clefable, 1)]
    [InlineData(1540, Species.Greninja, 3)]
    [InlineData(1572, Species.Glimmora, 1)]
    public void ReadsSpeciesByHeartAndSoulsIds(ushort index, Species species, byte form)
    {
        var pokemon = new HeartAndSoulPokemon { SpeciesIndex = index };

        (pokemon.Species, pokemon.Form).Should().Be(((ushort)species, form));
    }

    [Theory]
    [InlineData(681, "TM100")]
    [InlineData(690, "Oval Charm")]
    [InlineData(829, "Clefablite")]
    [InlineData(873, "Glimmoranite")]
    [InlineData(881, "Clear Bell")]
    [InlineData(888, "Secret Medicine (G4)")]
    [InlineData(900, "Azure Flute")]
    public void ReadsItemsByHeartAndSoulsIds(ushort index, string item)
    {
        var pokemon = new HeartAndSoulPokemon { HeldItemIndex = index };

        pokemon.HeldItem.Should().Be(ItemRepository.GetItemByName(item)!.Id);
    }

    [Theory]
    [InlineData(874)]
    [InlineData(875)]
    public void AnItemOnlyHeartAndSoulHasIsUnknown(ushort index) =>
        new HeartAndSoulPokemon { HeldItemIndex = index }.HeldItem.Should().Be(0xF000 + index);

    [Fact]
    public void ReadsMovesUpToMalignantChain()
    {
        var pokemon = new HeartAndSoulPokemon();
        pokemon.SetMoveIndex(0, 847);
        pokemon.SetMoveIndex(1, 848);

        pokemon.Move1.Should().Be((ushort)Move.MalignantChain);
        pokemon.Move2.Should().Be(0);
    }

    [Theory]
    [InlineData(26, Ball.Beast)]
    [InlineData(27, Ball.Cherish)]
    public void ReadsBallsInHeartAndSoulsOrder(byte index, Ball ball) =>
        new HeartAndSoulPokemon { BallIndex = index }.Ball.Should().Be((byte)ball);

    [Theory]
    [InlineData(7, true)]
    [InlineData(8, false)]
    public void ShininessUsesHeartAndSoulsOdds(ushort xor, bool shiny)
    {
        var pokemon = new HeartAndSoulPokemon { TID16 = 12345, SID16 = 54321 };
        pokemon.PID = (uint)(pokemon.TID16 ^ pokemon.SID16 ^ xor) << 16;

        pokemon.IsShiny.Should().Be(shiny);
    }

    [Fact]
    public void ExportingWithoutEditsKeepsEveryByte() =>
        Load().ToByteArray().Should().Equal(Fixture);

    [Fact]
    public void AnEditIsWrittenToTheNewerSlotOnly()
    {
        var game = Load();

        At(game, Goldeen).Update(new PokemonPatch(Level: 70));

        var exported = game.ToByteArray();
        exported.AsSpan(0, NewerSlot * 0x1000).SequenceEqual(Fixture.AsSpan(0, NewerSlot * 0x1000)).Should().BeTrue();
        exported.AsSpan(28 * 0x1000).SequenceEqual(Fixture.AsSpan(28 * 0x1000)).Should().BeTrue();
        exported.Should().NotEqual(Fixture);
    }

    [Fact]
    public void AnEditKeepsTheBytesThatFollowTheChecksummedData()
    {
        var game = Load();

        At(game, Goldeen).Update(new PokemonPatch(Level: 70));

        var exported = game.ToByteArray();
        for (var sector = NewerSlot; sector < NewerSlot + 14; sector++)
        {
            var id = ReadUInt16LittleEndian(Fixture.AsSpan((sector * 0x1000) + 0xFF4));
            var end = (sector * 0x1000) + 0xFF4;
            var start = (sector * 0x1000) + ChecksumLength(id);
            exported.AsSpan(start..end).SequenceEqual(Fixture.AsSpan(start..end)).Should().BeTrue($"sector id {id} keeps its bytes after the checksum");
        }
    }

    [Fact]
    public void EditsSurviveExportAndReload()
    {
        var game = Load();

        At(game, Goldeen).Update(new PokemonPatch(
            Level: 70,
            HeldItem: ItemRepository.GetItemByName("Leftovers")!.Id,
            Moves: [(int)Move.Thunderbolt, (int)Move.Surf, (int)Move.IceBeam, (int)Move.Psychic]));

        game.SaveAndReload(reloaded =>
        {
            reloaded.SaveFile.ChecksumsValid.Should().BeTrue();
            var pkm = At(reloaded, Goldeen).Pkm;
            pkm.CurrentLevel.Should().Be(70);
            pkm.HeldItem.Should().Be(ItemRepository.GetItemByName("Leftovers")!.Id);
            pkm.Moves.Should().Equal((ushort)Move.Thunderbolt, (ushort)Move.Surf, (ushort)Move.IceBeam, (ushort)Move.Psychic);
        });
    }

    // The fixture's first slot has counter 100 and its second 101.
    private const int NewerSlot = 14;

    private static int ChecksumLength(int id) => id switch
    {
        0 => 3892,
        4 => 3856,
        13 => 2400,
        _ => 3968,
    };
}
