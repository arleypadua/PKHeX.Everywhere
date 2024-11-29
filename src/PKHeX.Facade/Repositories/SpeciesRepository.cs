using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public class SpeciesRepository
{
    private readonly Game _game;
    private readonly IImmutableDictionary<Species, SpeciesDefinition> _species;

    internal SpeciesRepository(Game game)
    {
        _game = game;
        var saveSpecificDataSource = new FilteredGameDataSource(game.SaveFile, GameInfo.Sources);
        _species = saveSpecificDataSource.Species.ToImmutableDictionary(
            k => (Species)k.Value,
            v => new SpeciesDefinition((Species)v.Value, v.Text));
    }

    public IEnumerable<SpeciesDefinition> AllGameSpecies => _species.Values;

    public SpeciesDefinition Get(Species species)
    {
        if (species == Species.None) return SpeciesDefinition.None;
        return _species[species];
    }

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
    
    public static IImmutableList<SpeciesDefinition> GetEvolutionsFrom(Species species, EntityContext generation, byte form = 0) =>
        EvolutionTree
            .GetEvolutionTree(generation)
            .GetEvolutionsAndPreEvolutions((ushort)species, form)
            .Select(result => All[(Species)result.Species])
            .ToImmutableList();
}

public record SpeciesDefinition(Species Species, string Name)
{
    public int Id => (int)Species;
    internal ushort ShortId => (ushort)Species;
    
    public static bool IsSome(SpeciesDefinition species) => species.Species != Species.None;
    
    public static implicit operator Species(SpeciesDefinition d) => d.Species;
    
    public static SpeciesDefinition None => new(Species.None, "None");
}