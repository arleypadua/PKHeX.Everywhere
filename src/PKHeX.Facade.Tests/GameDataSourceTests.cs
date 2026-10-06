using System.Collections.Frozen;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class GameDataSourceTests
{
    private static Game WithOwnGameData() =>
        Game.LoadFrom(File.ReadAllBytes(SaveFilePath.LetsGoPikachu), SaveFilePath.LetsGoPikachu, new LetsGoWithOwnGameData());

    [Fact]
    public void ASaveWithNoFormatGetsThePKHeXDefault() =>
        SaveFilePath.Load(SaveFilePath.Emerald).GameData.Should().BeOfType<PKHeXGameData>();

    [Theory]
    [InlineData(SaveFilePath.Yellow)]
    [InlineData(SaveFilePath.Crystal)]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.FireRed)]
    [InlineData(SaveFilePath.HgSs)]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    public void TheDefaultLocksNothing(string saveFile) =>
        SaveFilePath.Load(saveFile).Trainer.Party.Pokemons[0].Options().Locked.Should().BeEmpty();

    [Fact]
    public void TheOptionListsComeFromTheFormatsGameData()
    {
        var game = WithOwnGameData();

        game.Options.Natures.Should().Equal(new Choice((int)Nature.Adamant, "Adamant"));
        game.Trainer.Party.Pokemons[0].Options().Locked.Should().Equal(PokemonField.Ability);
    }

    [Fact]
    public void AnUpdateThatChangesALockedFieldFails()
    {
        var pokemon = WithOwnGameData().Trainer.Party.Pokemons[0];
        var ability = pokemon.Options().Abilities.First(a => a.Id != pokemon.Pkm.Ability).Id;

        var update = () => pokemon.Update(new PokemonPatch(Ability: ability));

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(nameof(PokemonPatch.Ability));
    }

    [Fact]
    public void AnUpdateThatKeepsALockedFieldPasses()
    {
        var pokemon = WithOwnGameData().Trainer.Party.Pokemons[0];

        pokemon.Update(new PokemonPatch(Ability: pokemon.Pkm.Ability, Level: 42));

        pokemon.Level.Should().Be(42);
    }

    [Fact]
    public void TheFormatsGameDataNamesIdsPKHeXCantName()
    {
        var game = WithOwnGameData();

        game.ItemRepository.GetGameItem(LetsGoWithOwnGameData.OwnItem).Name.Should().Be("Own item");
        game.ItemRepository.GetGameItem(LetsGoWithOwnGameData.OwnItem + 1).Name.Should().Be($"Unknown Item {LetsGoWithOwnGameData.OwnItem + 1}");
    }

    private sealed class LetsGoWithOwnGameData : ISaveFormat
    {
        public const ushort OwnItem = 60000;

        public string Id => "own-game-data";
        public string Name => "Own Game Data";
        public GameVersion BaseGame => GameVersion.GP;
        public IReadOnlySet<Capability> Capabilities { get; } = new HashSet<Capability>();
        public SaveFormatMatch Detect(ReadOnlySpan<byte> data) => SaveFormatMatch.No;
        public SaveFile Load(byte[] data) => SaveUtil.GetSaveFile(data)!;
        public IGameDataSource GameData(SaveFile save) => new OwnGameData(save);
    }

    private sealed class OwnGameData(SaveFile save) : PKHeXGameData(save)
    {
        public override IReadOnlyList<Choice> Natures { get; } = [new((int)Nature.Adamant, "Adamant")];
        public override IReadOnlySet<PokemonField> Locked { get; } = FrozenSet.Create(PokemonField.Ability);
        public override string? NameOf(GameDataKind kind, int id) =>
            kind == GameDataKind.Item && id == LetsGoWithOwnGameData.OwnItem ? "Own item" : null;
    }
}
