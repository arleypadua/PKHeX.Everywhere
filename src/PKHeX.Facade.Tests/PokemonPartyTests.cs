using FluentAssertions;

namespace PKHeX.Facade.Tests;

public class PokemonPartyTests
{
    [Fact]
    public void PartyShouldContain()
    {
        var game = GameFixture.CreateTestGame();
        game.Trainer.Party.Pokemons.Should().HaveCountGreaterThan(0);
    }
}
