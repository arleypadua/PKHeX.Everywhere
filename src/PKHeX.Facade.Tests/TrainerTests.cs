using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Facade.Repositories;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Facade.Tests;

public class TrainerTests
{
    [Theory]
    [SupportedSaveFiles]
    public void TrainerData_ShouldBeParsed(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        game.Trainer.Gender.Should().Be(Gender.Male);
        game.Trainer.Name.Should().NotBeNull();
        game.Trainer.Money.Amount.Should().BeGreaterThan(0);
    }

    [Theory]
    [SupportedSaveFiles]
    public void TrainerName_ShouldBeMutable(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        game.Trainer.Name = "NewName";

        game.SaveAndReload(reloaded =>
        {
            reloaded.Trainer.Name.Should().Be("NewName");
        });
    }

    [Theory]
    [SupportedSaveFiles]
    public void TrainerTID_ShouldBeMutable(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        game.Trainer.TID = 12345;

        game.SaveAndReload(reloaded =>
        {
            reloaded.Trainer.TID.Should().Be(12345);
        });
    }

    [Theory]
    [SupportedSaveFiles]
    public void TrainerSID_ShouldBeMutable(string saveFile)
    {
        var game = Game.LoadFrom(saveFile);
        game.Trainer.SID = 54321;

        game.SaveAndReload(reloaded =>
        {
            reloaded.Trainer.SID.Should().Be(54321);
        });
    }

    [Theory]
    [Games(GameVersion.HG)]
    public void ApplyOwnerToAll_ShouldUpdateAllPokemon(Game game)
    {
        game.Trainer.Name = "TestOT";
        game.Trainer.TID = 11111;
        game.Trainer.SID = 22222;

        game.Trainer.ApplyOwnerToAll();

        game.SaveAndReload(reloaded =>
        {
            var allPokemon = reloaded.Trainer.Party.Pokemons
                .Concat(reloaded.Trainer.PokemonBox.All)
                .Where(p => p.Species != SpeciesDefinition.None)
                .ToList();

            allPokemon.Should().HaveCountGreaterThan(0);
            allPokemon.Should().AllSatisfy(p =>
            {
                p.Owner.Name.Should().Be("TestOT");
                // p.Id.TID reads Pkm.DisplayTID directly — the same path used by the
                // ApplyOwnerToAll setter — because Owner.TID getter uses TrainerTID7,
                // which differs from DisplayTID in Gen 4 (it incorporates SID16).
                p.Id.TID.Should().Be(11111u);
                p.Id.SID.Should().Be(22222u);
            });
        });
    }
}
