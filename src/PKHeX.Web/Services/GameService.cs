using PKHeX.Everywhere.Engine;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;

namespace PKHeX.Web.Services;

public class GameService(
    AnalyticsService analytics,
    Session session)
{
    public Game? Game => session.Game;
    public Game LoadedGame => Game ?? throw new NullReferenceException("Expected game to be loaded, but it was null.");
    public string? FileName => session.FileName;
    
    public bool IsLoaded => Game != null;

    public event EventHandler? OnGameLoaded;

    public void Load(byte[] bytes, string fileName)
    {
        session.Load(Game.LoadFrom(bytes, fileName), fileName);

        OnGameLoaded?.Invoke(this, EventArgs.Empty);

        analytics.TrackGameLoaded(LoadedGame);
    }

    public void LoadBlank(GameVersionDefinition version)
    {
        session.Load(Game.EmptyOf(version), version.Name);
        
        OnGameLoaded?.Invoke(this, EventArgs.Empty);
        
        analytics.TrackGameLoaded(LoadedGame);
    }

    public Stream Export()
    {
        ArgumentNullException.ThrowIfNull(Game, nameof(Game));

        var bytes = Game.ToByteArray();
        return new MemoryStream(bytes);
    }
}