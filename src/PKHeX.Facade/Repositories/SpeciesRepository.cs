using System.Collections.Immutable;
using PKHeX.Core;
using PKHeX.Facade.Abstractions;

namespace PKHeX.Facade.Repositories;

public class SpeciesRepository
{
    private readonly Game _game;
    private readonly IImmutableDictionary<Species, SpeciesDefinition> _species;

    internal SpeciesRepository(Game game)
    {
        _game = game;
        _species = game.GameData.Species.ToImmutableDictionary(
            k => (Species)k.Id,
            v => new SpeciesDefinition((Species)v.Id, v.Name));
    }

    public IEnumerable<SpeciesDefinition> AllGameSpecies => _species.Values;

    public SpeciesDefinition Get(Species species)
    {
        if (species == Species.None) return SpeciesDefinition.None;
        return _species.TryGetValue(species, out var definition)
            || All.TryGetValue(species, out definition)
            ? definition
            : new SpeciesDefinition(species, _game.GameData.NameOf(GameDataKind.Species, (int)species) ?? $"Unknown ({(int)species})");
    }

    internal bool Knows(ushort id) => Find(id) is not null || _species.ContainsKey((Species)id);

    public IImmutableList<SpeciesDefinition> GetEvolutionsFrom(SpeciesDefinition definition, byte form = 0) => Find(definition.ShortId) is null ? [] :
        EvolutionTree
            .GetEvolutionTree(_game.Generation)
            .GetEvolutionsAndPreEvolutions(definition.ShortId, form)
            .Select(result => All[(Species)result.Species])
            .Where(species => _species.ContainsKey(species) && _game.IsAwareOf(species, form))
            .ToImmutableList();

    public static IImmutableDictionary<Species, SpeciesDefinition> All = GameInfo.Sources
        .SpeciesDataSource
        .ToImmutableDictionary(
            k => (Species)k.Value,
            v => new SpeciesDefinition((Species)v.Value, v.Text));

    public static SpeciesDefinition? Find(ushort id) =>
        All.GetValueOrDefault((Species)id) is { } species && SpeciesDefinition.IsSome(species) ? species : null;
    
    public static IImmutableList<SpeciesDefinition> GetEvolutionsFrom(Species species, EntityContext generation, byte form = 0) => Find((ushort)species) is null ? [] :
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

    public static SpeciesDefinition Unknown(ushort stored) => new(Species.None, $"Unknown (#{stored})");
}