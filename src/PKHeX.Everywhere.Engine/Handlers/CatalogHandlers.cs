using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class CatalogHandlers
{
    [Query("catalog.names")]
    public static CatalogNames Names(CatalogNamesRequest request) => new(
        request.SpeciesIds
            .Select(id => SpeciesRepository.All.GetValueOrDefault((Species)id))
            .OfType<SpeciesDefinition>()
            .Where(SpeciesDefinition.IsSome)
            .Select(species => species.ToCatalogName())
            .ToArray(),
        request.ItemIds.Select(id => ItemRepository.GetItem(id).ToCatalogName()).ToArray());
}
