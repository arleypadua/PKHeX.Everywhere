using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Dtos;

public record SaveSummary(string? FileName, string Version, int Generation);

public record ExportedSave(byte[] Bytes, string FileName);

public static class GameMapping
{
    public static SaveSummary ToSummary(this Game game, string? fileName) =>
        new(fileName, game.GameVersionApproximation.Name, game.SaveFile.Generation);
}
