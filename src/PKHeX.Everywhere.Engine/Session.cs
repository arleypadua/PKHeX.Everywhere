using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine;

public sealed class Session
{
    public static Session Current { get; } = new();

    public Game? Game { get; private set; }
    public string? FileName { get; private set; }

    public void Load(Game game, string? fileName)
    {
        Game = game;
        FileName = string.IsNullOrWhiteSpace(fileName) ? FileName : fileName;
    }

    internal Game RequireGame() =>
        Game ?? throw new EngineException(ErrorCodes.NoSave, "No save is loaded.");
}
