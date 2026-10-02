using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Dtos;

public record SaveSummary(string? FileName, string Version, int Generation);

public record SaveVersion(string Version, int VersionId, string Generation, int GenerationId);

public record ExportedSave(byte[] Bytes, string FileName);

public record PartyMember(int SpeciesId, string Species, int Level);

public record GameOverview(
    string Version,
    int VersionId,
    string Generation,
    int GenerationId,
    string TrainerGender,
    int BoxCount,
    PartyMember[] Party);

public record LoadedSave(byte[] Bytes, string FileName, string Version);

public static class GameMapping
{
    public static SaveSummary ToSummary(this Game game, string? fileName) =>
        new(fileName, game.GameVersionApproximation.Name, game.SaveFile.Generation);

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
