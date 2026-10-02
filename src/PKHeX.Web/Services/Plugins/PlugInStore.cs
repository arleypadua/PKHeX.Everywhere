namespace PKHeX.Web.Services.Plugins;

/// <summary>
/// Starts the React app, which loads stored plug-ins, when the Blazor app starts.
/// </summary>
public sealed class PlugInStore(ReactApp reactApp)
{
    public Task Start() => reactApp.InvokeVoidAsync("start");
}
