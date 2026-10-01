using PKHeX.Facade;

namespace PKHeX.Engine;

public static class Session
{
    public static Game? Game { get; private set; }
    public static string? FileName { get; private set; }

    public static event Action? Changed;

    public static void Load(Game game, string? fileName)
    {
        Game = game;
        FileName = string.IsNullOrWhiteSpace(fileName) ? FileName : fileName;
        Changed?.Invoke();
    }
}
