using Blazor.Analytics;
using Microsoft.AspNetCore.Components;
using PKHeX.Facade;
using PKHeX.Facade.Pokemons;
using PKHeX.Web.Components;
using PKHeX.Web.Extensions;
using PKHeX.Web.Plugins;
using PKHeX.Web.Services.Plugins;

namespace PKHeX.Web.Services;

public class AnalyticsService(
    IAnalytics analytics,
    NavigationManager navigation,
    GameService gameService)
{
    public void TrackGameLoaded(Game game)
    {
        analytics.TrackEvent("game_loaded", GetPayloadFrom(game));
    }
    
    public void TrackGameExported(Game game)
    {
        analytics.TrackEvent("game_exported", GetPayloadFrom(game));
    }

    private object GetPayloadFrom(Game game)
    {
        var party = game.Trainer.Party.Pokemons;
        return new
        {
            version_name = game.GameVersionApproximation.Name,
            version_id = game.GameVersionApproximation.Id,
            generation_name = game.Generation.ToString(),
            generation_id = (int)game.Generation,
            gender = game.Trainer.Gender.Name,
            box_size = game.Trainer.PokemonBox.All.Count,
            party_species_id_01 = party.ElementAtOrDefault(0)?.Species.Id,
            party_species_name_01 = party.ElementAtOrDefault(0)?.Species.Name,
            party_level_01 = party.ElementAtOrDefault(0)?.Level,
            party_species_id_02 = party.ElementAtOrDefault(1)?.Species.Id,
            party_species_name_02 = party.ElementAtOrDefault(1)?.Species.Name,
            party_level_02 = party.ElementAtOrDefault(1)?.Level,
            party_species_id_03 = party.ElementAtOrDefault(2)?.Species.Id,
            party_species_name_03 = party.ElementAtOrDefault(2)?.Species.Name,
            party_level_03 = party.ElementAtOrDefault(2)?.Level,
            party_species_id_04 = party.ElementAtOrDefault(3)?.Species.Id,
            party_species_name_04 = party.ElementAtOrDefault(3)?.Species.Name,
            party_level_04 = party.ElementAtOrDefault(3)?.Level,
            party_species_id_05 = party.ElementAtOrDefault(4)?.Species.Id,
            party_species_name_05 = party.ElementAtOrDefault(4)?.Species.Name,
            party_level_05 = party.ElementAtOrDefault(4)?.Level,
            party_species_id_06 = party.ElementAtOrDefault(5)?.Species.Id,
            party_species_name_06 = party.ElementAtOrDefault(5)?.Species.Name,
            party_level_06 = party.ElementAtOrDefault(5)?.Level,
        };
    }

    public void TrackPokemon(string eventType, Pokemon pokemon, PokemonSource? source = null)
    {
        analytics.TrackEvent(eventType, new
        {
            species_id = pokemon.Species.Id,
            species_name = pokemon.Species.Name,
            gender = pokemon.Gender.Name,
            ball_name = pokemon.Ball.Name,
            level = pokemon.Level,
            source = source?.ToString()
        });
    }

    public void TrackItemModified(AddItemModal.ItemToBeAdded itemToBeAdded)
    {
        analytics.TrackEvent("item_modified", new
        {
            item_id = itemToBeAdded.Id,
            quantity = itemToBeAdded.Count,
        });
    }

    public void TrackPlugInHookExecuted(IPluginHook hook, Exception? failure)
    {
        analytics.TrackEvent("plugin_hook_executed", new
        {
            hook_name = hook.GetType().Name,
            exception_type = failure?.GetType().Name,
            exception_message = failure?.Message,
        });
    }

    public void TrackInstalled(LoadedPlugIn plugIn)
    {
        analytics.TrackEvent("plug_in_installed", GetPayloadFrom(plugIn));
    }

    public void TrackUpdated(LoadedPlugIn plugIn)
    {
        analytics.TrackEvent("plug_in_updated", GetPayloadFrom(plugIn));
    }

    private object GetPayloadFrom(LoadedPlugIn plugIn) => new
    {
        id = plugIn.Id,
        source_id = plugIn.SourceId,
        file_url = plugIn.FileUrl,
        version = plugIn.Version.ToString(),
    };

    public void TrackError(Exception exception)
    {
        var game = gameService.Game;
        analytics.TrackEvent("unexpected_error", new
        {
            current_route = navigation.CurrentRoute(),
            exception_message = exception.Message,
            exception_stack_trace = exception.StackTrace,
            exception_type = exception.GetType().Name,
            exception_id = exception.GetExceptionTrackingId(),
            version_name = game?.GameVersionApproximation.Name,
            version_id = game?.GameVersionApproximation.Id,
            generation_name = game?.Generation.ToString(),
            generation_id = (int?)game?.Generation,
        });
    }
}