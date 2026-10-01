using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Dtos;

public record SpeciesEntry(int Id, string Name);

public static class SpeciesMapping
{
    public static SpeciesEntry ToEntry(this SpeciesDefinition species) => new(species.Id, species.Name);
}
