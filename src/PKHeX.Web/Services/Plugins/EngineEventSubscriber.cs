using PKHeX.Everywhere.Engine;
using PKHeX.Web.Plugins;

namespace PKHeX.Web.Services.Plugins;

public sealed class EngineEventSubscriber : IDisposable
{
    private readonly Session _session;
    private readonly PlugInRuntime _runtime;
    private readonly AnalyticsService _analytics;

    public EngineEventSubscriber(Session session, PlugInRuntime runtime, AnalyticsService analytics)
    {
        _session = session;
        _runtime = runtime;
        _analytics = analytics;
        _session.Published += HandlePublished;
    }

    private async Task OnItemChanged(int itemId, int count)
    {
        await _runtime.RunAll<IRunOnItemChanged>(h => h.OnItemChanged(new((ushort)itemId, (uint)count)));
        _analytics.TrackItemModified(itemId, count);
    }

    private void HandlePublished(IEngineEvent engineEvent)
    {
        if (engineEvent is ItemChanged changed) _ = OnItemChanged(changed.ItemId, changed.Count);
    }

    public void Dispose() => _session.Published -= HandlePublished;
}
