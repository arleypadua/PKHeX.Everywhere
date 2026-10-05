using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class GameOptionsTests
{
    [Theory]
    [InlineData(SaveFilePath.Crystal, 0)]
    [InlineData(SaveFilePath.Emerald, 25)]
    [InlineData(SaveFilePath.HgSs, 25)]
    [InlineData(SaveFilePath.LetsGoPikachu, 25)]
    public void NaturesExistFromGenerationThree(string saveFile, int count) =>
        SaveFilePath.Load(saveFile).Options.Natures.Should().HaveCount(count);

    [Theory]
    [InlineData(SaveFilePath.Crystal, 0)]
    [InlineData(SaveFilePath.Emerald, 12)]
    [InlineData(SaveFilePath.HgSs, 24)]
    public void BallsAreTheOnesTheSaveCanStore(string saveFile, int count)
    {
        var balls = SaveFilePath.Load(saveFile).Options.Balls;

        balls.Should().HaveCount(count);
        balls.Select(b => b.Id).Should().NotContain(0);
    }

    [Theory]
    [InlineData(SaveFilePath.Crystal, false, false)]
    [InlineData(SaveFilePath.Emerald, true, false)]
    [InlineData(SaveFilePath.HgSs, true, true)]
    public void LanguagesAreTheOnesTheGenerationKnows(string saveFile, bool english, bool korean)
    {
        var languages = SaveFilePath.Load(saveFile).Options.Languages.Select(l => (LanguageID)l.Id).ToList();

        languages.Contains(LanguageID.English).Should().Be(english);
        languages.Contains(LanguageID.Korean).Should().Be(korean);
    }

    [Theory]
    [SupportedSaveFiles]
    public void HeldItemsIncludeNoItemAndTheItemsPartyPokemonHold(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);

        var heldItems = game.Options.HeldItems.Select(i => i.Id).ToList();

        heldItems.Should().Contain(0);
        heldItems.Should().Contain(game.Trainer.Party.Pokemons.Select(p => p.Pkm.HeldItem));
    }

    [Theory]
    [InlineData(SaveFilePath.Crystal)]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.Unbound)]
    public void TypesAreNormalToFairyInEverySave(string saveFile)
    {
        var types = SaveFilePath.Load(saveFile).Options.Types;

        types.Select(t => t.Id).Should().Equal(Enumerable.Range(0, 18));
        types.Single(t => t.Id == 9).Name.Should().Be("Fire");
    }
}
