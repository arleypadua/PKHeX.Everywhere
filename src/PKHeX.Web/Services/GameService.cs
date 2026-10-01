using PKHeX.Everywhere.Engine;
using PKHeX.Facade;

namespace PKHeX.Web.Services;

public class GameService : IDisposable
{
    private readonly AnalyticsService _analytics;
    private readonly Session _session;

    public GameService(AnalyticsService analytics, Session session)
    {
        _analytics = analytics;
        _session = session;
        _session.GameChanged += HandleGameChanged;
    }

    public Game? Game => _session.Game;
    public Game LoadedGame => Game ?? throw new NullReferenceException("Expected game to be loaded, but it was null.");
    public string? FileName => _session.FileName;
    
    public bool IsLoaded => Game != null;

    public event EventHandler? OnGameLoaded;
    public event EventHandler? OnGameClosed;

    public void Load(byte[] bytes, string fileName) => _session.Load(Game.LoadFrom(bytes, fileName), fileName);

    private void HandleGameChanged()
    {
        if (Game is null)
        {
            OnGameClosed?.Invoke(this, EventArgs.Empty);
            return;
        }

        OnGameLoaded?.Invoke(this, EventArgs.Empty);
        _analytics.TrackGameLoaded(LoadedGame);
    }

    public void Dispose() => _session.GameChanged -= HandleGameChanged;
}
