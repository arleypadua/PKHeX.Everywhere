using System.Text.Json;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PKHeX.Core;
using PKHeX.Web;
using PKHeX.Web.Services;
using PKHeX.Web.Services.Plugins;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<EncounterService>();
builder.Services.AddScoped<LoadPokemonService>();

builder.Services.AddScoped<PlugInService>();
builder.Services.AddScoped<PlugInRegistry>();
builder.Services.AddScoped<PlugInRuntime>();
builder.Services.AddScoped<PlugInRegistry>();
builder.Services.AddScoped<PlugInLocalStorage>();
builder.Services.AddScoped<PlugInLocalStorageLoader>();
builder.Services.AddScoped<PlugInSourceService>();
builder.Services.AddScoped<PlugInSourceLocalStorage>();

builder.Services.AddScoped<JsService>();
builder.Services.AddScoped<AntdThemeService>();
builder.Services.AddScoped<ClipboardService>();
builder.Services.AddScoped<BrowserWindowService.Instance>();

builder.Services.AddScoped<BlazorAesProvider>();
builder.Services.AddScoped<BlazorMd5Provider>();

builder.Services.AddAntDesign();

builder.Services.AddBlazoredLocalStorage(config =>
{
    config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
    config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    config.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
    config.JsonSerializerOptions.WriteIndented = false;
});

var app = builder.Build();

// Although Blazor WASM can target the whole .NET Framework API surface,
// during the Runtime, Microsoft has disabled the native support to some APIs under the System.Security.Cryptography namespace
// During startup we replace PKHeX unsupported cryptography APIs with a javascript-based alternative 
RuntimeCryptographyProvider.Aes = app.Services.GetRequiredService<BlazorAesProvider>();
RuntimeCryptographyProvider.Md5 = app.Services.GetRequiredService<BlazorMd5Provider>();

app.Services.GetRequiredService<PlugInLocalStorageLoader>()
    .InitializePlugIns();

await app.RunAsync();