using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// The species and item ids <c>catalog.names</c> should name. It doesn't need a loaded save.
/// </summary>
/// <param name="SpeciesIds">National Pokédex numbers, as PKHeX.Core numbers species.</param>
/// <param name="ItemIds">PKHeX.Core item ids, which follow the Generation 4 and later numbering. Generation 1 to 3 saves number their pouch items differently.</param>
public record CatalogNamesRequest(ushort[] SpeciesIds, ushort[] ItemIds);

/// <summary>
/// A species or item id with its display name.
/// </summary>
public record CatalogName(int Id, string Name);

/// <summary>
/// The names <c>catalog.names</c> found for a <see cref="CatalogNamesRequest"/>.
/// </summary>
/// <param name="Species">One entry per known species id. Unknown ids and 0 are left out.</param>
/// <param name="Items">One entry per requested item id, in order.</param>
public record CatalogNames(CatalogName[] Species, CatalogName[] Items);

public static class CatalogMapping
{
    public static CatalogName ToCatalogName(this SpeciesDefinition species) => new(species.Id, species.Name);

    public static CatalogName ToCatalogName(this ItemDefinition item) => new(item.Id, item.Name);
}
