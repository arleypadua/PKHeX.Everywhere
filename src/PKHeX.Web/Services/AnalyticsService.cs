using Microsoft.JSInterop;
using PKHeX.Facade;
using PKHeX.Web.Components;
using PKHeX.Web.Extensions;

namespace PKHeX.Web.Services;

public class AnalyticsService(ReactApp reactApp)
{
    public void TrackGameLoaded(Game game)
    {
        Track("game_loaded", GetPayloadFrom(game));
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

    public void TrackError(Exception exception, string? currentRoute = null, Game? currentGame = null)
    {
        var details = new
        {
            current_route = currentRoute,
            exception_message = exception.Message,
            exception_stack_trace = exception.StackTrace,
            exception_type = exception.GetType().Name,
            exception_id = exception.GetExceptionTrackingId(),
            version_name = currentGame?.GameVersionApproximation.Name,
            version_id = currentGame?.GameVersionApproximation.Id,
            generation_name = currentGame?.Generation.ToString(),
            generation_id = (int?)currentGame?.Generation,
        };
        
        Track("unexpected_error", details);
        CaptureError(exception);
    }

    public void CaptureError(Exception exception) => _ = InvokeAsync("captureBlazorError", new
    {
        type = exception.GetType().Name,
        message = exception.Message,
        details = exception.ToString(),
        id = exception.GetExceptionTrackingId(),
    });

    private void Track(string eventName, object payload) => _ = InvokeAsync("track", eventName, payload);

    private async Task InvokeAsync(string identifier, params object[] args)
    {
        try
        {
            await reactApp.InvokeVoidAsync(identifier, args);
        }
        catch (JSException)
        {
            // A failed analytics or error report must not surface as an unobserved task exception.
        }
    }
}
