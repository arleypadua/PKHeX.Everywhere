using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonTypesTests
{
    [Theory]
    [InlineData(SaveFilePath.Yellow, Species.Charmander, new[] { MoveType.Fire })]
    [InlineData(SaveFilePath.Yellow, Species.Gastly, new[] { MoveType.Ghost, MoveType.Poison })]
    [InlineData(SaveFilePath.Crystal, Species.Steelix, new[] { MoveType.Steel, MoveType.Ground })]
    [InlineData(SaveFilePath.Crystal, Species.Umbreon, new[] { MoveType.Dark })]
    public void GenerationOneAndTwoTypesAreTheIdsOfLaterGames(string saveFile, Species species, MoveType[] types)
    {
        var pokemon = SaveFilePath.Load(saveFile).Trainer.Party.Pokemons[0];

        pokemon.Update(new PokemonPatch(Species: (int)species));

        pokemon.Details().Types.Should().Equal(types.Select(type => (int)type));
    }

    [Fact]
    public void LaterGamesKeepTheirTypes()
    {
        var pokemon = SaveFilePath.Load(SaveFilePath.Emerald).Trainer.Party.Pokemons[0];

        pokemon.Details().Types.Should().Equal((int)MoveType.Fire);
    }
}
