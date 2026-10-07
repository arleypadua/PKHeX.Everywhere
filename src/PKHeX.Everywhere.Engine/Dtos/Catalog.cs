using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// The species, item, ability and nature ids <c>catalog.names</c> should name. It doesn't need a loaded save.
/// </summary>
/// <param name="SpeciesIds">National Pokédex numbers, as PKHeX.Core numbers species. With a <c>FormatId</c>, also the ids <c>species.list</c> gives the hack's own species.</param>
/// <param name="ItemIds">PKHeX.Core item ids, which follow the Generation 4 and later numbering. Generation 1 to 3 saves number their pouch items differently.</param>
/// <param name="AbilityIds">PKHeX.Core ability ids, as <c>pokemon.read</c> returns in <c>ability</c>. Not the ability slot. Leave out to name none.</param>
/// <param name="NatureIds">PKHeX.Core nature ids, 0 (Hardy) to 24, as <c>pokemon.read</c> returns in <c>nature</c>. Leave out to name none.</param>
/// <param name="FormatId">
/// The id of a ROM hack's format from <c>game.formats()</c>, such as <c>unbound</c>, to name the hack's own species and items, as <c>species.list</c> and <c>pokemon.options()</c> do with its save loaded.
/// Abilities and natures are named the same with or without it.
/// An unknown or disabled id fails with <c>not-found</c>, and <c>pkhex</c> with <c>not-supported</c>.
/// </param>
public record CatalogNamesRequest(ushort[] SpeciesIds, ushort[] ItemIds, string? FormatId = null, ushort[]? AbilityIds = null, ushort[]? NatureIds = null);

/// <summary>
/// A species, item, ability or nature id with its display name.
/// </summary>
public record CatalogName(int Id, string Name);

/// <summary>
/// The names <c>catalog.names</c> found for a <see cref="CatalogNamesRequest"/>.
/// </summary>
/// <param name="Species">One entry per known species id. Unknown ids and 0 are left out.</param>
/// <param name="Items">One entry per requested item id, in order.</param>
/// <param name="Abilities">One entry per requested ability id, in order. An id PKHeX doesn't know is named <c>Unknown Ability {id}</c>.</param>
/// <param name="Natures">One entry per requested nature id, in order. An id PKHeX doesn't know is named <c>Unknown Nature {id}</c>.</param>
public record CatalogNames(CatalogName[] Species, CatalogName[] Items, CatalogName[] Abilities, CatalogName[] Natures);

public static class CatalogMapping
{
    public static CatalogName ToCatalogName(this SpeciesDefinition species) => new(species.Id, species.Name);

    public static CatalogName ToCatalogName(this ItemDefinition item) => new(item.Id, item.Name);

    public static CatalogName ToCatalogName(this AbilityDefinition ability) => new(ability.Id, ability.Name);

    public static CatalogName ToCatalogName(this NatureDefinition nature) => new(nature.Id, nature.Name);
}
