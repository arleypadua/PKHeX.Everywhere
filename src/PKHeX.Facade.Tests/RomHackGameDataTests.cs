using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class RomHackGameDataTests
{
    private static Pokemon At(Game game, int box, int slot) => game.Trainer.PokemonBox.All[(box * 30) + slot];

    private static string MetLocationName(Pokemon pokemon) =>
        pokemon.Options().MetLocations.Single(location => location.Id == pokemon.Pkm.MetLocation).Name;

    // Radical Red keeps FireRed's map, so its met locations use FireRed's names only while its wild Pokémon are met where they live in FireRed.
    [Theory]
    [InlineData(0, 1, Species.Pidgey, 4, "Route 1")]
    [InlineData(0, 8, Species.Pikachu, 5, "Viridian Forest")]
    [InlineData(1, 18, Species.Paras, 14, "Mt. Moon")]
    [InlineData(1, 19, Species.Diglett, 25, "Diglett's Cave")]
    [InlineData(1, 11, Species.Spiritomb, 46, "Pokémon Tower")]
    public void RadicalRedsMetLocationsAreTheFireRedPlacesItsPokemonComeFrom(int box, int slot, Species species, int metLevel, string place)
    {
        var pokemon = At(SaveFilePath.Load(SaveFilePath.RadicalRed), box, slot);

        pokemon.Species.Species.Should().Be(species);
        pokemon.Pkm.MetLevel.Should().Be((byte)metLevel);
        MetLocationName(pokemon).Should().Be(place);
        pokemon.Options().Locked.Should().NotContain(PokemonField.MetLocation);
    }

    [Fact]
    public void UnboundNamesTheStoredMetLocationByNumberAndLocksIt()
    {
        var pokemon = SaveFilePath.Load(SaveFilePath.Unbound).Trainer.Party.Pokemons[0];

        MetLocationName(pokemon).Should().Be("Location #179");
        pokemon.Options().Locked.Should().Contain(PokemonField.MetLocation);
    }

    [Theory]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    public void ChangingTheOriginGameKeepsTheMetLocation(string saveFile)
    {
        var pokemon = SaveFilePath.Load(saveFile).Trainer.Party.Pokemons[0];
        var metLocation = pokemon.Pkm.MetLocation;

        pokemon.Update(new PokemonPatch(Version: (int)GameVersion.E));

        pokemon.Details().Should().BeEquivalentTo(new { Version = (int)GameVersion.E, MetLocation = (int)metLocation });
    }

    [Theory]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    public void BallsAreTheOnesTheHacksBallTableMaps(string saveFile)
    {
        var balls = SaveFilePath.Load(saveFile).Options.Balls;

        balls.Should().HaveCount(26);
        balls.Select(ball => (Ball)ball.Id).Should().Contain([Ball.Poke, Ball.Dream, Ball.Beast]).And.NotContain([Ball.None, Ball.Strange]);
    }

    [Theory]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    public void AllNaturesAreListedButLocked(string saveFile)
    {
        var game = SaveFilePath.Load(saveFile);

        game.Options.Natures.Should().HaveCount(25);
        game.Trainer.Party.Pokemons[0].Options().Locked.Should().Contain([PokemonField.Nature, PokemonField.Ability, PokemonField.Gender]);
    }

    [Theory]
    [InlineData(SaveFilePath.Unbound)]
    [InlineData(SaveFilePath.RadicalRed)]
    public void MovesAreTheOnesTheHackCanStore(string saveFile)
    {
        var moves = SaveFilePath.Load(saveFile).Options.Moves;

        moves.Should().Contain(new Choice((int)Move.FishiousRend, "Fishious Rend")).And.NotContain(move => move.Id == (int)Move.None);
        moves.Select(move => move.Id).Should().NotContain((int)Move.TeraBlast);
    }

    [Theory]
    [InlineData(SaveFilePath.Unbound, true)]
    [InlineData(SaveFilePath.RadicalRed, true)]
    [InlineData(SaveFilePath.Emerald, false)]
    [InlineData(SaveFilePath.LetsGoPikachu, false)]
    public void StatsAreApproximateOnlyForTheHacks(string saveFile, bool approximate) =>
        SaveFilePath.Load(saveFile).GameData.StatsApproximate.Should().Be(approximate);
}
