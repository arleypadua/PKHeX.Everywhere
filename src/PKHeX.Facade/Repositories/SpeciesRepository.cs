using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public class SpeciesRepository
{
    internal SpeciesRepository(Game game)
    {
        var saveSpecificDataSource = new FilteredGameDataSource(game.SaveFile, GameInfo.Sources);
        Species = saveSpecificDataSource.Species.ToImmutableDictionary(
            k => (Species)k.Value,
            v => new SpeciesDefinition((Species)v.Value, v.Text));
    }

    public IImmutableDictionary<Species, SpeciesDefinition> Species { get; }
    
    public static IImmutableDictionary<Species, SpeciesDefinition> All = GameInfo.Sources
        .SpeciesDataSource
        .ToImmutableDictionary(
            k => (Species)k.Value,
            v => new SpeciesDefinition((Species)v.Value, v.Text));
}

public record SpeciesDefinition(Species Species, string Name)
{
    public int Id => (int)Species;
    
    public static bool IsSome(SpeciesDefinition species) => species.Species != Species.None;
}