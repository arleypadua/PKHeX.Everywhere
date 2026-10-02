using Microsoft.JSInterop;
using PKHeX.Facade;
using PKHeX.Web.Extensions;

namespace PKHeX.Web.Services;

public class AnalyticsService(ReactApp reactApp)
{
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
