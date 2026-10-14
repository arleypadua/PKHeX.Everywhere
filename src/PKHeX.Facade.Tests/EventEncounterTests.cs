using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

// Events are listed under the save's ParseSettings, which are global, so no other test runs meanwhile.
[Collection(nameof(TransferTests))]
public class EventEncounterTests
{
    [Theory]
    [InlineData(SaveFilePath.FireRed)]
    [InlineData(SaveFilePath.Emerald)]
    [InlineData(SaveFilePath.Crystal)]
    [InlineData(SaveFilePath.Yellow)]
    public void EveryEventConvertsToALegalPokemonOfTheSave(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);

        var events = game.PokemonRepository.FindEvents().ToList();

        events.Should().NotBeEmpty();
        foreach (var gift in events)
        {
            var pokemon = gift.ConvertToPokemon();
            pokemon.Pkm.Should().BeOfType(game.SaveFile.PKMType);
            SaveRules.Under(game.SaveFile, pokemon.LegalityReport).Should().Match<Pokemons.PokemonLegality>(l => l.Valid, $"{gift.Species} from {gift.Version.Name} should be legal");
        }
    }

    [Theory]
    [InlineData(SaveFilePath.FireRed)]
    [InlineData(SaveFilePath.Emerald)]
    public void Gen3SavesListTheWishmakerJirachiWithItsTrainer(string saveFile)
    {
        var events = SaveFilePath.Load(saveFile).PokemonRepository.FindEvents(Species.Jirachi).ToList();

        events.Should().Contain(e => e.Trainer == new EventTrainer("WISHMKR", 20043, "ENG") && e.Version.Version == GameVersion.R);
    }

    [Fact]
    public void ASpeciesNarrowsTheEvents()
    {
        var events = SaveFilePath.Load(SaveFilePath.Yellow).PokemonRepository.FindEvents(Species.Mew).ToList();

        events.Should().NotBeEmpty().And.OnlyContain(e => e.Species == Species.Mew);
    }

    [Fact]
    public void ACrystalSaveListsGen1EventsThroughTheTimeCapsule() =>
        SaveFilePath.Load(SaveFilePath.Crystal).PokemonRepository.FindEvents().Should().Contain(e => e.Data.Generation == 1);

    [Fact]
    public void AnEventTheRecipientOwnsHasNoTrainer() =>
        SaveFilePath.Load(SaveFilePath.Emerald).PokemonRepository.FindEvents()
            .Where(e => e.Data is EncounterGift3 { IsFixedTrainer: false, Language: 0 })
            .Should().NotBeEmpty().And.OnlyContain(e => e.Trainer == new EventTrainer(null, null, null));

    [Fact]
    public void WonderCardsAreLeftToTheMysteryGiftAlbum() =>
        SaveFilePath.Load(SaveFilePath.HgSs).PokemonRepository.FindEvents().Select(e => e.Data).OfType<MysteryGift>().Should().BeEmpty();
}
