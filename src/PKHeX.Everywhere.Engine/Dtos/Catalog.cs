using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Dtos;

public record CatalogNamesRequest(ushort[] SpeciesIds, ushort[] ItemIds);

public record CatalogName(int Id, string Name);

public record CatalogNames(CatalogName[] Species, CatalogName[] Items);

public static class CatalogMapping
{
    public static CatalogName ToCatalogName(this SpeciesDefinition species) => new(species.Id, species.Name);

    public static CatalogName ToCatalogName(this ItemDefinition item) => new(item.Id, item.Name);
}
