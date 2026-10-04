using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Expansion.Imperium;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade.Tests;

public class ImperiumPokemonTests
{
    private static readonly byte[] Fixture = File.ReadAllBytes(SaveFilePath.Imperium);

    private const int Rapidash = 8;
    private const int Meowscarada = 12;
    private const int FirstEmptySlot = 24;

    private static Game Load(byte[]? bytes = null) => Game.LoadFrom(bytes ?? Fixture.ToArray(), SaveFilePath.Imperium);

    private static Pokemon At(Game game, int index) => game.Trainer.PokemonBox.All[index];

    private static ImperiumPokemon Pkm(Pokemon pokemon) => (ImperiumPokemon)pokemon.Pkm;

    [Fact]
    public void ChangingTheNatureKeepsThePidAndSurvivesExport()
    {
        var game = Load();
        var pid = At(game, Rapidash).Pkm.PID;

        At(game, Rapidash).Update(new PokemonPatch(Nature: (int)Nature.Timid));

        game.SaveAndReload(reloaded =>
        {
            At(reloaded, Rapidash).Pkm.Nature.Should().Be(Nature.Timid);
            At(reloaded, Rapidash).Pkm.PID.Should().Be(pid);
        });
    }

    [Fact]
    public void ANewPidKeepsTheNature()
    {
        var pokemon = Pkm(At(Load(), Rapidash));
        pokemon.Nature = Nature.Timid;

        pokemon.PID += 1;

        pokemon.Nature.Should().Be(Nature.Timid);
    }

    [Theory]
    [InlineData("Crabominable")]
    [InlineData("Cat")]
    public void ANicknameOfUpTo12CharactersSurvivesExport(string nickname)
    {
        var game = Load();
        At(game, Meowscarada).Nickname.Should().Be("Meowscarada");

        At(game, Meowscarada).Update(new PokemonPatch(Nickname: nickname));

        game.SaveAndReload(reloaded => At(reloaded, Meowscarada).Nickname.Should().Be(nickname));
    }

    [Fact]
    public void EditsKeepTheBitsAroundTheFieldsTheyChange()
    {
        var save = new ImperiumSave(Fixture.ToArray());
        var stored = (ImperiumPokemon)save.GetBoxSlotAtIndex(Rapidash);
        SetExpansionBits(stored);
        save.SetBoxSlotAtIndex(stored, Rapidash);
        var game = Load(save.Write().ToArray());

        At(game, Rapidash).Update(new PokemonPatch(
            Level: 70,
            HeldItem: ItemRepository.GetItemByName("Leftovers")!.Id,
            Ball: (int)Ball.Ultra,
            Nickname: "Crabominable",
            Moves: [(int)Move.Thunderbolt, (int)Move.Surf, (int)Move.IceBeam, (int)Move.Psychic]));

        game.SaveAndReload(reloaded => ExpansionBits(Pkm(At(reloaded, Rapidash))).Should().Be(ExpansionBits(stored)));
    }

    // Tera type, the evolution trackers, hyper training, and Imperium's IV and ability flags.
    private static void SetExpansionBits(ImperiumPokemon pokemon)
    {
        var data = pokemon.Data;
        WriteUInt16LittleEndian(data[0x20..], (ushort)(ReadUInt16LittleEndian(data[0x20..]) | 0xF800));
        WriteUInt16LittleEndian(data[0x2A..], (ushort)(ReadUInt16LittleEndian(data[0x2A..]) | 0x4000));
        foreach (var move in (int[])[0x2C, 0x2E, 0x30])
            WriteUInt16LittleEndian(data[move..], (ushort)(ReadUInt16LittleEndian(data[move..]) | 0xF800));
        WriteUInt16LittleEndian(data[0x32..], (ushort)(ReadUInt16LittleEndian(data[0x32..]) | 0xC000));
        for (var pp = 0x34; pp < 0x38; pp++) data[pp] |= 0x80;
        WriteUInt32LittleEndian(data[0x4C..], ReadUInt32LittleEndian(data[0x4C..]) | (1u << 28));
    }

    private static string ExpansionBits(ImperiumPokemon pokemon)
    {
        var data = pokemon.Data;
        ushort[] masked =
        [
            (ushort)(ReadUInt16LittleEndian(data[0x20..]) & 0xF800),
            (ushort)(ReadUInt16LittleEndian(data[0x2A..]) & 0x4000),
            (ushort)(ReadUInt16LittleEndian(data[0x2C..]) & 0xF800),
            (ushort)(ReadUInt16LittleEndian(data[0x2E..]) & 0xF800),
            (ushort)(ReadUInt16LittleEndian(data[0x30..]) & 0xF800),
            (ushort)(ReadUInt16LittleEndian(data[0x32..]) & 0xC000),
            (ushort)(data[0x34] & data[0x35] & data[0x36] & data[0x37] & 0x80),
            (ushort)((ReadUInt32LittleEndian(data[0x4C..]) >> 28) & 1),
        ];
        return string.Join(",", masked);
    }

    // Imperium's shiny odds are 36 in 65536, and its shiny flag flips the result.
    [Theory]
    [InlineData(35, false, true)]
    [InlineData(36, false, false)]
    [InlineData(35, true, false)]
    [InlineData(36, true, true)]
    public void ShininessUsesImperiumsOddsAndShinyFlag(ushort xor, bool flag, bool shiny)
    {
        var pokemon = new ImperiumPokemon { TID16 = 12345, SID16 = 54321 };
        pokemon.PID = (uint)(pokemon.TID16 ^ pokemon.SID16 ^ xor) << 16;
        if (flag) pokemon.Data[0x1F] |= 0x40;

        pokemon.IsShiny.Should().Be(shiny);
    }

    [Fact]
    public void APokemonCopiedToAnEmptySlotIsListedAfterExport()
    {
        var game = Load();

        game.Trainer.PokemonBox.AddOnEmptySlot(At(game, Rapidash).Clone(), out var index).Should().BeTrue();

        index.Should().Be(FirstEmptySlot);
        game.SaveAndReload(reloaded =>
        {
            reloaded.SaveFile.ChecksumsValid.Should().BeTrue();
            At(reloaded, FirstEmptySlot).Species.Name.Should().Be("Rapidash");
        });
    }
}
