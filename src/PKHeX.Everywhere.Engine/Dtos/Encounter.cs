using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// A game version with its display name.
/// </summary>
/// <param name="Id">The game's PKHeX <c>GameVersion</c> value.</param>
public record VersionEntry(int Id, string Name);

/// <summary>
/// The games whose encounters <c>encounters.search</c> can look up for the loaded save, returned by <c>encounters.versions</c>.
/// </summary>
/// <param name="Versions">The games from the save's generation.</param>
/// <param name="Default">The loaded save's PKHeX <c>GameVersion</c> value, to preselect. Saves that can't tell paired games apart report one of the pair.</param>
public record EncounterVersions(VersionEntry[] Versions, int Default);

/// <summary>
/// One encounter from <c>encounters.search</c>: a way to obtain the species legally in the chosen game.
/// </summary>
/// <param name="Index">The row's position in the latest search. Pass it to <c>box.addEncounter</c>; it is only valid until the next search.</param>
/// <param name="SpeciesId">The species' National Pokédex number, as PKHeX.Core numbers species.</param>
/// <param name="Form">The form the encounter yields. Its name is empty when the form has none.</param>
/// <param name="Name">PKHeX's description of the encounter.</param>
/// <param name="IsEgg">Whether the encounter gives an egg.</param>
/// <param name="Ball">The Poké Ball the encounter always uses, or null when it has no fixed ball.</param>
/// <param name="LevelRange">The encounter's level, such as <c>5</c>, or its range, such as <c>3-7</c>.</param>
/// <param name="Location">Where the encounter happens. For eggs, the egg location.</param>
/// <param name="Version">The display name of the game the encounter belongs to.</param>
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
