using PKHeX.Everywhere.Engine;
using PKHeX.Everywhere.Engine.PlugIns;

namespace PKHeX.Web.Services;

public sealed class EngineEventAnalytics : IDisposable
{
    private readonly Session _session;
    private readonly AnalyticsService _analytics;

    public EngineEventAnalytics(Session session, AnalyticsService analytics)
    {
        _session = session;
        _analytics = analytics;
        _session.Published += HandlePublished;
    }

    private void HandlePublished(IEngineEvent engineEvent)
    {
        if (engineEvent is ItemChanged changed) _analytics.TrackItemModified(changed.ItemId, changed.Count);
        if (engineEvent is PokemonAdded added) OnPokemonAdded(added);
        if (engineEvent is GameExported && _session.Game is { } game) _analytics.TrackGameExported(game);
        if (engineEvent is PlugInInstalled installed) _analytics.TrackPlugInInstalled(installed.PlugInId, installed.Version);
        if (engineEvent is PlugInUpdated updated) _analytics.TrackPlugInUpdated(updated.PlugInId, updated.Version);
    }

    private void OnPokemonAdded(PokemonAdded added)
    {
        if (_session.Game is not { } game || added.At.Box is not { } box) return;

        var pokemon = game.Trainer.PokemonBox.All[box * game.SaveFile.BoxSlotCount + added.At.Slot];
        var eventName = added.Source switch
        {
            PokemonAddSource.File => "pokemon_loaded_from_file",
            PokemonAddSource.Encounter => "pokemon_loaded_from_encounter",
            _ => null,
        };
        if (eventName is not null) _analytics.TrackPokemon(eventName, pokemon);
    }

    public void Dispose() => _session.Published -= HandlePublished;
}
