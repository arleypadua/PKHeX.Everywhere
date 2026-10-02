using Microsoft.JSInterop;

namespace PKHeX.Web.Services.Plugins;

/// <summary>
/// Starts the React app, which loads stored plug-ins, when the Blazor app starts.
/// </summary>
public sealed class PlugInStore(IJSRuntime js, IConfiguration configuration)
{
    public async Task Start()
    {
        var module = await js.InvokeAsync<IJSObjectReference>("import", "./Components/ReactPage.razor.js");
        await module.InvokeVoidAsync("start", configuration["React:DevServerUrl"]);
    }
}
