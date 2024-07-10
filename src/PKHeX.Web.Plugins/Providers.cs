using PKHeX.Facade;

namespace PKHeX.Web.Plugins;

public interface IGameProvider
{
    Game? Game { get; }
    Game LoadedGame => Game ?? throw new NullReferenceException("Expected game to be loaded, but it was null.");
    string? FileName { get; }
    bool IsLoaded { get; }
}