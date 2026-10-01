using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Dtos;

public record VersionEntry(int Id, string Name);

public record EncounterVersions(VersionEntry[] Versions, int Default);

public static class EncounterMapping
{
    public static VersionEntry ToEntry(this GameVersionDefinition version) => new(version.Id, version.Name);
}
