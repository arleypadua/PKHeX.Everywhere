using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Dtos;

public record VersionEntry(int Id, string Name);

public record EncounterVersions(VersionEntry[] Versions, int Default);

public record EncounterRow(
    int Index,
    int SpeciesId,
    string Species,
    PokemonForm Form,
    string Name,
    bool IsEgg,
    string? Ball,
    string LevelRange,
    string Location,
    string Version);

public static class EncounterMapping
{
    public static VersionEntry ToEntry(this GameVersionDefinition version) => new(version.Id, version.Name);

    public static EncounterRow ToRow(this Encounter encounter, int index)
    {
        var ball = encounter.Ball;
        return new EncounterRow(
            index,
            (int)encounter.Species,
            SpeciesRepository.All[encounter.Species].Name,
            new PokemonForm(encounter.Data.Form, encounter.Form?.Name ?? ""),
            encounter.Data.LongName,
            encounter.Data.IsEgg,
            ball.IsNone ? null : ball.Name,
            encounter.LevelRange.Format,
            encounter.Location.Name,
            encounter.Version.Name);
    }
}
