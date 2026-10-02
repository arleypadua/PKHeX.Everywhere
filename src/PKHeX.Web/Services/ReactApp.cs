using Microsoft.JSInterop;

namespace PKHeX.Web.Services;

public sealed class ReactApp(IJSRuntime js, IConfiguration configuration)
{
    private Task<IJSObjectReference>? _module;

    public async Task InvokeVoidAsync(string identifier, params object?[] args) =>
        await (await Module()).InvokeVoidAsync(identifier, [configuration["React:DevServerUrl"], ..args]);

    public async Task<T> InvokeAsync<T>(string identifier, params object?[] args) =>
        await (await Module()).InvokeAsync<T>(identifier, [configuration["React:DevServerUrl"], ..args]);

    private Task<IJSObjectReference> Module() =>
        _module ??= js.InvokeAsync<IJSObjectReference>("import", "./Components/ReactPage.razor.js").AsTask();
}
