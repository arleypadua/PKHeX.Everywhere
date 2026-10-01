using Microsoft.JSInterop;

namespace PKHeX.Web.Services.Plugins;

/// <summary>
/// Reaches the React app's plug-in store, which owns plug-in storage, until the plug-in pages move to React.
/// </summary>
public sealed class PlugInStore(IJSRuntime js, IConfiguration configuration)
{
    private Task<IJSObjectReference>? _module;

    public async Task Start() => await (await Module()).InvokeVoidAsync("start", DevServerUrl);

    public Task<AvailablePlugIn[]> Available() => Call<AvailablePlugIn[]>("available");

    public Task Install(string sourceUrl, string id) => Call<object?>("install", sourceUrl, id);

    public Task<bool> Update(string id) => Call<bool>("update", id);

    public Task Uninstall(string id) => Call<object?>("uninstall", id);

    public Task Persist(string id) => Call<object?>("persist", id);

    private string? DevServerUrl => configuration["React:DevServerUrl"];

    private Task<IJSObjectReference> Module() =>
        _module ??= js.InvokeAsync<IJSObjectReference>("import", "./Components/ReactPage.razor.js").AsTask();

    private async Task<T> Call<T>(string method, params object?[] args) =>
        await (await Module()).InvokeAsync<T>("plugIns", [DevServerUrl, method, .. args]);
}

public sealed record AvailablePlugIn(
    string SourceUrl,
    string SourceName,
    string Id,
    string Name,
    string? Description,
    string? ProjectUrl,
    string Version);
