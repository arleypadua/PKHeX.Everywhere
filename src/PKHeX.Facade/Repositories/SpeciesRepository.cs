using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public class SpeciesRepository
{
    private readonly Game _game;

    internal SpeciesRepository(Game game)
    {
        _game = game;
        var saveSpecificDataSource = new FilteredGameDataSource(game.SaveFile, GameInfo.Sources);
        Species = saveSpecificDataSource.Species.ToImmutableDictionary(
            k => (Species)k.Value,
            v => new SpeciesDefinition((Species)v.Value, v.Text));
    }

    public IImmutableDictionary<Species, SpeciesDefinition> Species { get; }

    public IImmutableList<SpeciesDefinition> GetEvolutionsFrom(SpeciesDefinition definition, byte form = 0) =>
        EvolutionTree
            .GetEvolutionTree(_game.Generation)
            .GetEvolutionsAndPreEvolutions(definition.ShortId, form)
            .Select(result => All[(Species)result.Species])
            .Where(species => _game.IsAwareOf(species, form))
            .ToImmutableList();

    public static IImmutableDictionary<Species, SpeciesDefinition> All = GameInfo.Sources
        .SpeciesDataSource
        .ToImmutableDictionary(
            k => (Species)k.Value,
            v => new SpeciesDefinition((Species)v.Value, v.Text));
}

public record SpeciesDefinition(Species Species, string Name)
{
    public int Id => (int)Species;
    internal ushort ShortId => (ushort)Species;
    
    public static bool IsSome(SpeciesDefinition species) => species.Species != Species.None;
    
    public static implicit operator Species(SpeciesDefinition d) => d.Species;
}