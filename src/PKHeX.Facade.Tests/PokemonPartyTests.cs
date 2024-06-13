using FluentAssertions;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class PokemonPartyTests
{
    [Theory]
    [SupportedSaveFiles]
    public void PartyShouldContain(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        game.Trainer.Party.Pokemons.Should().HaveCountGreaterThan(0);
    }
}
