using AwesomeAssertions;
using PKHeX.Facade;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Tests.Base;

namespace PKHeX.Everywhere.Engine.Tests;

public class TopicTests
{
    [Fact]
    public void LoadingASaveChangesEverything()
    {
        var (session, changes) = Observe(new Session());

        session.Load(Game.LoadFrom(SaveFilePath.HgSs), SaveFilePath.HgSs);

        changes.Should().BeEquivalentTo([new[] { Topics.All }]);
    }

    [Fact]
    public void UpdatingAPartyPokemonChangesTheParty()
    {
        var (session, changes) = Loaded();
        var pokemon = session.Game!.Trainer.Party.Pokemons.First();
        var edited = pokemon.Clone();
        edited.Pkm.CurrentLevel = 42;

        session.Game.Trainer.AddOrUpdate(pokemon.UniqueId, edited, PokemonSource.Party);

        changes.Should().BeEquivalentTo([new[] { Topics.Party }]);
    }

    [Fact]
    public void UpdatingABoxPokemonChangesTheBoxes()
    {
        var (session, changes) = Loaded();
        var pokemon = session.Game!.Trainer.PokemonBox.All.First(p => p.Species != PKHeX.Core.Species.None);
        var edited = pokemon.Clone();
        edited.Pkm.CurrentLevel = 42;

        session.Game.Trainer.AddOrUpdate(pokemon.UniqueId, edited, PokemonSource.Box);

        changes.Should().BeEquivalentTo([new[] { Topics.Box }]);
    }

    [Fact]
    public void WritesToAReplacedSaveChangeNothing()
    {
        var (session, changes) = Loaded();
        var replaced = session.Game!;
        session.Load(Game.LoadFrom(SaveFilePath.HgSs), SaveFilePath.HgSs);
        changes.Clear();

        var pokemon = replaced.Trainer.Party.Pokemons.First();
        replaced.Trainer.AddOrUpdate(pokemon.UniqueId, pokemon.Clone(), PokemonSource.Party);

        changes.Should().BeEmpty();
    }

    private static (Session, List<string[]>) Loaded()
    {
        var (session, changes) = Observe(new Session());
        session.Load(Game.LoadFrom(SaveFilePath.HgSs), SaveFilePath.HgSs);
        changes.Clear();
        return (session, changes);
    }

    private static (Session, List<string[]>) Observe(Session session)
    {
        var changes = new List<string[]>();
        session.Changed += topics => changes.Add(topics);
        return (session, changes);
    }
}
