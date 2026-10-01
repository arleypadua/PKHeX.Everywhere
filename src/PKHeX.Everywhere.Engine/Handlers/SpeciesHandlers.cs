using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class SpeciesHandlers
{
    [Query("species.list", Topics.Game)]
    public static SpeciesEntry[] List(Game game) => game.SpeciesRepository.AllGameSpecies
        .Where(SpeciesDefinition.IsSome)
        .OrderBy(species => species.Name, StringComparer.Ordinal)
        .Select(species => species.ToEntry())
        .ToArray();
}
