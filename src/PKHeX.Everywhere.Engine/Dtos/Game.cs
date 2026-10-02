using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// The loaded save's file name, game and generation, returned by <c>game.get</c>.
/// </summary>
/// <param name="FileName">The file name the save was loaded with. <c>game.load</c> uses <c>save.sav</c> when none is given, and blank saves use the game's name.</param>
/// <param name="Version">The game's display name, such as <c>Emerald</c>. Saves that can't tell paired games apart (such as HeartGold and SoulSilver) report one of the pair.</param>
/// <param name="Generation">The generation number, from 1 to 9.</param>
/// <param name="HasEvents">Whether the save has event flags and work to edit. When false, <c>events.get</c> returns null.</param>
public record SaveSummary(string? FileName, string Version, int Generation, bool HasEvents);

/// <summary>
/// The loaded save's game and generation, returned by <c>game.version</c>.
/// </summary>
/// <param name="Version">The game's display name, such as <c>Emerald</c>. Saves that can't tell paired games apart (such as HeartGold and SoulSilver) report one of the pair.</param>
/// <param name="VersionId">The game's PKHeX <c>GameVersion</c> value.</param>
/// <param name="Generation">The PKHeX <c>EntityContext</c> name, such as <c>Gen3</c>. Let's Go saves use <c>Gen7b</c>.</param>
/// <param name="GenerationId">The PKHeX <c>EntityContext</c> value. It equals the generation number (1 to 9) for main-series contexts. Let's Go, Legends: Arceus, Brilliant Diamond and Shining Pearl, and Legends: Z-A use 11 to 14.</param>
public record SaveVersion(string Version, int VersionId, string Generation, int GenerationId);

/// <summary>
/// The save file written by <c>game.export</c>, ready to download.
/// </summary>
/// <param name="Bytes">The complete save file, including every change made so far.</param>
/// <param name="FileName">The file name the save was loaded with.</param>
public record ExportedSave(byte[] Bytes, string FileName);

/// <summary>
/// A Pokémon in the trainer's party.
/// </summary>
/// <param name="SpeciesId">The species' National Pokédex number, as PKHeX.Core numbers species.</param>
/// <param name="Level">The Pokémon's current level.</param>
public record PartyMember(int SpeciesId, string Species, int Level);

/// <summary>
/// A snapshot of the save that game events carry: game, generation, trainer gender, box size and party.
/// </summary>
/// <param name="Version">The game's display name, such as <c>Emerald</c>. Saves that can't tell paired games apart (such as HeartGold and SoulSilver) report one of the pair.</param>
/// <param name="VersionId">The game's PKHeX <c>GameVersion</c> value.</param>
/// <param name="Generation">The PKHeX <c>EntityContext</c> name, such as <c>Gen3</c>. Let's Go saves use <c>Gen7b</c>.</param>
/// <param name="GenerationId">The PKHeX <c>EntityContext</c> value. It equals the generation number (1 to 9) for main-series contexts. Let's Go, Legends: Arceus, Brilliant Diamond and Shining Pearl, and Legends: Z-A use 11 to 14.</param>
/// <param name="TrainerGender">The trainer's gender as a capitalized name, such as <c>Male</c>.</param>
/// <param name="BoxCount">The number of box slots in the save, empty ones included.</param>
public record GameOverview(
    string Version,
    int VersionId,
    string Generation,
    int GenerationId,
    string TrainerGender,
    int BoxCount,
    PartyMember[] Party);

/// <summary>
/// The loaded save's current bytes, returned by <c>game.file</c>. Unlike <c>game.export</c>, it raises no <c>gameExported</c> event.
/// </summary>
/// <param name="Bytes">The complete save file, including every change made so far.</param>
/// <param name="FileName">The file name the save was loaded with, or empty when there is none.</param>
/// <param name="Version">The PKHeX <c>GameVersion</c> member name, such as <c>E</c> for Emerald. Saves that can't tell paired games apart report one of the pair.</param>
public record LoadedSave(byte[] Bytes, string FileName, string Version);

public static class GameMapping
{
    public static SaveSummary ToSummary(this Game game, string? fileName) =>
        new(fileName, game.GameVersionApproximation.Name, game.SaveFile.Generation, game.Events is not null);

    public static SaveVersion ToVersion(this Game game) => new(
        game.GameVersionApproximation.Name,
        game.GameVersionApproximation.Id,
        game.Generation.ToString(),
        (int)game.Generation);

    public static GameOverview ToOverview(this Game game) => new(
        game.GameVersionApproximation.Name,
        game.GameVersionApproximation.Id,
        game.Generation.ToString(),
        (int)game.Generation,
        game.Trainer.Gender.Name,
        game.Trainer.PokemonBox.All.Count,
        game.Trainer.Party.Pokemons.Select(p => new PartyMember(p.Species.Id, p.Species.Name, p.Level)).ToArray());

    public static LoadedSave ToFile(this Game game, string? fileName) =>
        new(game.ToByteArray(), fileName ?? string.Empty, game.GameVersionApproximation.Version.ToString());
}
