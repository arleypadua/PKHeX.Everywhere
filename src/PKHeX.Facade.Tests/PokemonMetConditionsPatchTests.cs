using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonMetConditionsPatchTests
{
    [Theory]
    [InlineData(SaveFilePath.HgSs)]
    [InlineData(SaveFilePath.LetsGoPikachu)]
    public void AppliesTheMetConditions(string saveFile)
    {
        var pokemon = SaveFilePath.Load(saveFile).Trainer.Party.Pokemons[0];
        var before = pokemon.Details();
        var location = pokemon.Options().MetLocations.First(l => l.Id != before.MetLocation).Id;
        var date = new DateOnly(2020, 5, 17);

        pokemon.Update(new PokemonPatch(MetLocation: location, MetLevel: 7, MetDate: date, FatefulEncounter: !before.FatefulEncounter));

        pokemon.Details().Should().BeEquivalentTo(new
        {
            MetLocation = location,
            MetLevel = 7,
            MetDate = (DateOnly?)date,
            FatefulEncounter = !before.FatefulEncounter,
        });
    }

    [Theory]
    [SupportedSaveFiles]
    public void ReportsTheMetConditions(string saveFile)
    {
        var pokemon = SaveFilePath.Load(saveFile).Trainer.Party.Pokemons[0];

        pokemon.Details().Should().BeEquivalentTo(new
        {
            Version = (int)pokemon.Pkm.Version,
            MetLocation = (int)pokemon.Pkm.MetLocation,
            MetLevel = (int)pokemon.Pkm.MetLevel,
            pokemon.Pkm.MetDate,
            pokemon.Pkm.FatefulEncounter,
        });
    }

    [Fact]
    public void ChangingTheOriginGameChangesTheMetLocations()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons.First(p => p.Pkm.Version is GameVersion.HG or GameVersion.SS);
        var before = pokemon.Options().MetLocations;

        pokemon.Update(new PokemonPatch(Version: (int)GameVersion.D));

        pokemon.Details().Version.Should().Be((int)GameVersion.D);
        pokemon.Options().MetLocations.Should().NotEqual(before);
        pokemon.Options().MetLocations.Should().BeEquivalentTo(
            GameInfo.GetLocationList(GameVersion.D, EntityContext.Gen4).DistinctBy(l => l.Value).Select(l => new Choice(l.Value, l.Text)));
    }

    [Fact]
    public void ChangingTheOriginGameToAnotherRegionResetsTheMetLocation()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons.First(p => p.Pkm.Version is GameVersion.HG or GameVersion.SS);
        pokemon.Update(new PokemonPatch(MetLocation: 126));

        pokemon.Update(new PokemonPatch(Version: (int)GameVersion.E));

        pokemon.Details().MetLocation.Should().Be(EncounterSuggestion.TryGetSuggestedTransferLocation(pokemon.Pkm));
    }

    [Fact]
    public void AppliesTheOriginGameBeforeTheMetLocation()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons.First(p => p.Pkm.Version is GameVersion.HG or GameVersion.SS);

        pokemon.Update(new PokemonPatch(Version: (int)GameVersion.Pt, MetLocation: 20));

        pokemon.Details().Should().BeEquivalentTo(new { Version = (int)GameVersion.Pt, MetLocation = 20 });
    }

    [Fact]
    public void ReportsAMetLevelAboveTheCurrentLevelAsIllegal()
    {
        var pokemon = Game.LoadFrom(SaveFilePath.HgSs).Trainer.Party.Pokemons.First(p => p.Level < 100 && p.Details().Legality!.Valid);

        pokemon.Update(new PokemonPatch(MetLevel: pokemon.Level + 1));

        pokemon.Details().Legality!.Valid.Should().BeFalse();
    }

    [Theory]
    [InlineData(SaveFilePath.Yellow)]
    [InlineData(SaveFilePath.Crystal)]
    public void OffersNoOriginGamesBeforeGenerationThree(string saveFile) =>
        SaveFilePath.Load(saveFile).Options.OriginGames.Should().BeEmpty();

    [Theory]
    [InlineData(SaveFilePath.Emerald, GameVersion.E)]
    [InlineData(SaveFilePath.HgSs, GameVersion.SS)]
    [InlineData(SaveFilePath.LetsGoPikachu, GameVersion.GP)]
    public void OffersTheOriginGamesTheSaveCanHold(string saveFile, GameVersion version)
    {
        var games = SaveFilePath.Load(saveFile).Options.OriginGames.Select(g => g.Id).ToList();

        games.Should().Contain((int)version);
        games.Should().NotContain((int)GameVersion.SL);
    }

    [Fact]
    public void OffersNoMetLocationsInGenerationOne() =>
        Game.LoadFrom(SaveFilePath.Yellow).Trainer.Party.Pokemons[0].Options().MetLocations.Should().BeEmpty();

    public static TheoryData<string, PokemonPatch, string> UnstorableValues() => new()
    {
        { SaveFilePath.HgSs, new PokemonPatch(Version: (int)GameVersion.SL), nameof(PokemonPatch.Version) },
        { SaveFilePath.Crystal, new PokemonPatch(Version: (int)GameVersion.E), nameof(PokemonPatch.Version) },
        { SaveFilePath.HgSs, new PokemonPatch(MetLocation: 5000), nameof(PokemonPatch.MetLocation) },
        { SaveFilePath.Yellow, new PokemonPatch(MetLocation: 1), nameof(PokemonPatch.MetLocation) },
        { SaveFilePath.HgSs, new PokemonPatch(MetLevel: 101), nameof(PokemonPatch.MetLevel) },
        { SaveFilePath.HgSs, new PokemonPatch(MetLevel: -1), nameof(PokemonPatch.MetLevel) },
        { SaveFilePath.Yellow, new PokemonPatch(MetLevel: 5), nameof(PokemonPatch.MetLevel) },
        { SaveFilePath.Emerald, new PokemonPatch(MetDate: new DateOnly(2020, 1, 1)), nameof(PokemonPatch.MetDate) },
        { SaveFilePath.HgSs, new PokemonPatch(MetDate: new DateOnly(1999, 12, 31)), nameof(PokemonPatch.MetDate) },
        { SaveFilePath.Crystal, new PokemonPatch(FatefulEncounter: true), nameof(PokemonPatch.FatefulEncounter) },
    };

    [Theory]
    [MemberData(nameof(UnstorableValues))]
    public void RejectsAValueTheSaveCantStore(string saveFile, PokemonPatch patch, string field)
    {
        var pokemon = SaveFilePath.Load(saveFile).Trainer.Party.Pokemons[0];
        var before = pokemon.Pkm.Data.ToArray();

        var update = () => pokemon.Update(patch with { Nickname = "Sparky" });

        update.Should().Throw<InvalidPatchException>().Which.Field.Should().Be(field);
        pokemon.Pkm.Data.ToArray().Should().Equal(before);
    }
}
