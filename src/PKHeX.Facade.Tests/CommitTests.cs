using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class CommitTests
{
    private const string NewTrainerName = "Tester";
    private const string EditedNickname = "Edited";

    [Theory]
    [SupportedSaveFiles]
    public void Export_AfterTrainerRename_WritesExistingPokemonBackUnchanged(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var before = Snapshot(game.SaveFile);

        game.Trainer.Name = NewTrainerName;

        game.SaveAndReload(reloaded => Snapshot(reloaded.SaveFile).Should().BeEquivalentTo(before));
    }

    [Theory]
    [SupportedSaveFiles]
    public void EditingOnePokemon_AfterTrainerRename_LeavesTheOthersUnchanged(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        var target = game.Trainer.PokemonBox.All.First(p => p.Pkm.Species != 0);
        var before = Snapshot(game.SaveFile).Where(s => s.Nickname != target.Nickname || s.Data != Hex(target.Pkm)).ToList();

        game.Trainer.Name = NewTrainerName;
        var edited = target.Clone();
        edited.ChangeNickname(EditedNickname);
        game.Trainer.AddOrUpdate(target.UniqueId, edited, PokemonSource.Box);

        Snapshot(game.SaveFile).Where(s => s.Nickname != EditedNickname).Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    [InlineData(SaveFilePath.LetsGoEevee)]
    public void AddingFromFile_AdaptsThePokemonToTheSave(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);
        game.Trainer.Name = NewTrainerName;
        var pokemon = PokemonFile.LoadFor(GameVersion.GP, game);
        pokemon.Owner.Name.Should().NotBe(NewTrainerName);

        game.Trainer.PokemonBox.AddOnEmptySlot(pokemon, out var index).Should().BeTrue();

        var added = game.Trainer.PokemonBox.All[index];
        added.Owner.HandlingTrainerName.Should().Be(NewTrainerName);
        added.Owner.CurrentHandler.Should().Be(Owner.Handler.SomeoneElse);
    }

    private record Slot(string Nickname, string Data, bool Legal);

    private static List<Slot> Snapshot(SaveFile save) => save.PartyData
        .Concat(save.BoxData)
        .Where(pkm => pkm.Species != 0)
        .Select(pkm => new Slot(pkm.Nickname, Hex(pkm), new LegalityAnalysis(pkm).Valid))
        .ToList();

    private static string Hex(PKM pkm) => Convert.ToHexString(pkm.Data);
}
