using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class CatalogHandlers
{
    [Query("catalog.names")]
    public static CatalogNames Names(CatalogNamesRequest request) => new(
        request.SpeciesIds
            .Select(SpeciesRepository.Find)
            .OfType<SpeciesDefinition>()
            .Select(species => species.ToCatalogName())
            .ToArray(),
        request.ItemIds.Select(id => ItemRepository.GetItem(id).ToCatalogName()).ToArray());
}
