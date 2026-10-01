using PKHeX.Facade;

namespace PKHeX.Everywhere.PlugIns;

public interface IGameProvider
{
    Game? Game { get; }
    Game LoadedGame => Game ?? throw new NullReferenceException("Expected game to be loaded, but it was null.");
    string? FileName { get; }
    bool IsLoaded { get; }
}