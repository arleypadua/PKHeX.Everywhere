using PKHeX.Engine;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;

namespace PKHeX.Web.Services;

public class GameService(
    AnalyticsService analytics)
{
    public Game? Game => Session.Game;
    public Game LoadedGame => Game ?? throw new NullReferenceException("Expected game to be loaded, but it was null.");
    public string? FileName => Session.FileName;
    
    public bool IsLoaded => Game != null;

    public event EventHandler? OnGameLoaded;

    public void Load(byte[] bytes, string fileName)
    {
        Session.Load(Game.LoadFrom(bytes, fileName), fileName);

        OnGameLoaded?.Invoke(this, EventArgs.Empty);

        analytics.TrackGameLoaded(Game);
    }

    public void LoadBlank(GameVersionDefinition version)
    {
        Session.Load(Game.EmptyOf(version), version.Name);
        
        OnGameLoaded?.Invoke(this, EventArgs.Empty);
        
        analytics.TrackGameLoaded(Game);
    }

    public Stream Export()
    {
        ArgumentNullException.ThrowIfNull(Game, nameof(Game));

        var bytes = Game.ToByteArray();
        return new MemoryStream(bytes);
    }
}