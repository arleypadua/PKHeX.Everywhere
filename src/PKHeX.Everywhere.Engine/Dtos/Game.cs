using PKHeX.Facade;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// The loaded save's file name, game, generation, save format and capabilities, returned by <c>game.get</c>.
/// </summary>
/// <param name="FileName">The file name the save was loaded with. <c>game.load</c> uses <c>save.sav</c> when none is given, and blank saves use the game's name.</param>
/// <param name="Version">The game's display name, such as <c>Emerald</c>. Saves that can't tell paired games apart (such as HeartGold and SoulSilver) report one of the pair.</param>
/// <param name="Generation">The generation number, from 1 to 9.</param>
/// <param name="HasEvents">Whether the save has event flags and work to edit. When false, <c>events.get</c> returns null, or fails with <c>not-supported</c> when the save doesn't support <c>events</c>.</param>
/// <param name="Format">The save format the save loaded through, or null for a save PKHeX reads on its own.</param>
/// <param name="Capabilities">What the save supports. A save from PKHeX supports all of them. Calls behind a capability that's missing fail with <c>not-supported</c>.</param>
/// <param name="StatsApproximate">Whether computed stats may differ from the game's. ROM hacks use the official games' base stats, which the hack may change.</param>
public record SaveSummary(string? FileName, string Version, int Generation, bool HasEvents, SaveFormat? Format, SaveCapability[] Capabilities, bool StatsApproximate);

/// <summary>
/// A save format PKHeX doesn't know, such as a ROM hack's, that the engine loads on its own.
/// </summary>
/// <param name="Id">The format's id, such as <c>unbound</c>.</param>
/// <param name="Name">The format's display name, such as <c>Pokémon Unbound</c>. Show it as the loaded game.</param>
/// <param name="BaseGame">The display name of the game the format builds on, such as <c>FireRed</c>.</param>
/// <param name="BaseGameId">The PKHeX <c>GameVersion</c> value of the game the format builds on.</param>
public record SaveFormat(string Id, string Name, string BaseGame, int BaseGameId);

/// <summary>
/// A save format <c>game.load()</c> can load a save with, returned by <c>game.formats()</c>.
/// </summary>
/// <param name="Id">The format's id, to pass to <c>game.load()</c> as <c>formatId</c>.</param>
/// <param name="Name">The format's display name.</param>
public record FormatEntry(string Id, string Name);

/// <summary>
/// A feature that relies on PKHeX knowing the save's game: the legality check in <c>pokemon.details()</c>, AutoLegality, the <c>encounters</c> calls,
/// Showdown export, the <c>events</c> calls and plug-ins. Without <c>plugIns</c>, hooks don't run, <c>plugins.actions()</c> and <c>plugins.pages()</c> are empty,
/// and <c>plugins.run()</c> and <c>plugins.pageModule()</c> fail with <c>not-supported</c>.
/// </summary>
public enum SaveCapability
{
    Legality,
    AutoLegality,
    Encounters,
    Showdown,
    Events,
    PlugIns,
}

/// <summary>
/// The loaded save's game and generation, returned by <c>game.version</c>.
/// </summary>
/// <param name="Version">The game's display name, such as <c>Emerald</c>. Saves that can't tell paired games apart (such as HeartGold and SoulSilver) report one of the pair.</param>
/// <param name="VersionId">The game's PKHeX <c>GameVersion</c> value.</param>
/// <param name="Generation">The PKHeX <c>EntityContext</c> name, such as <c>Gen3</c>. Let's Go saves use <c>Gen7b</c>.</param>
/// <param name="GenerationId">The PKHeX <c>EntityContext</c> value. It equals the generation number (1 to 9) for main-series contexts. Let's Go, Legends: Arceus, Brilliant Diamond and Shining Pearl, and Legends: Z-A use 11 to 14.</param>
/// <param name="FormatId">The id of the save format the save loaded through, or null for a save PKHeX reads on its own.</param>
public record SaveVersion(string Version, int VersionId, string Generation, int GenerationId, string? FormatId);

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
/// <param name="FormatId">The id of the save format the save loaded through, or null for a save PKHeX reads on its own.</param>
public record GameOverview(
    string Version,
    int VersionId,
    string Generation,
    int GenerationId,
    string TrainerGender,
    int BoxCount,
    PartyMember[] Party,
    string? FormatId);

/// <summary>
/// The loaded save's current bytes, returned by <c>game.file</c>. Unlike <c>game.export</c>, it raises no <c>gameExported</c> event.
/// </summary>
/// <param name="Bytes">The complete save file, including every change made so far.</param>
/// <param name="FileName">The file name the save was loaded with, or empty when there is none.</param>
/// <param name="Version">The PKHeX <c>GameVersion</c> member name, such as <c>E</c> for Emerald. Saves that can't tell paired games apart report one of the pair.</param>
public record LoadedSave(byte[] Bytes, string FileName, string Version);

public static class GameMapping
{
    public static SaveSummary ToSummary(this Game game, string? fileName) => new(
        fileName,
        game.GameVersionApproximation.Name,
        game.SaveFile.Generation,
        game.Events is not null,
        game.Format?.ToDto(),
        game.Capabilities.Order().Select(ToDto).ToArray(),
        game.GameData.StatsApproximate);

    public static SaveVersion ToVersion(this Game game) => new(
        game.GameVersionApproximation.Name,
        game.GameVersionApproximation.Id,
        game.Generation.ToString(),
        (int)game.Generation,
        game.Format?.Id);

    public static FormatEntry ToEntry(this ISaveFormat format) => new(format.Id, format.Name);

    private static SaveFormat ToDto(this SaveFormatDescription format) => new(
        format.Id,
        format.Name,
        GameVersionRepository.Instance.Get(format.BaseGame).Name,
        (int)format.BaseGame);

    private static SaveCapability ToDto(Capability capability) => capability switch
    {
        Capability.Legality => SaveCapability.Legality,
        Capability.AutoLegality => SaveCapability.AutoLegality,
        Capability.Encounters => SaveCapability.Encounters,
        Capability.Showdown => SaveCapability.Showdown,
        Capability.Events => SaveCapability.Events,
        Capability.PlugIns => SaveCapability.PlugIns,
        _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null),
    };

    public static GameOverview ToOverview(this Game game) => new(
        game.GameVersionApproximation.Name,
        game.GameVersionApproximation.Id,
        game.Generation.ToString(),
        (int)game.Generation,
        game.Trainer.Gender.Name,
        game.Trainer.PokemonBox.All.Count,
        game.Trainer.Party.Pokemons.Select(p => new PartyMember(p.Species.Id, p.Species.Name, p.Level)).ToArray(),
        game.Format?.Id);

    public static LoadedSave ToFile(this Game game, string? fileName) =>
        new(game.ToByteArray(), fileName ?? string.Empty, game.GameVersionApproximation.Version.ToString());
}
