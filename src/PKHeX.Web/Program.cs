using System.Text.Json;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PKHeX.Core;
using PKHeX.Everywhere.Engine;
using PKHeX.Everywhere.Engine.PlugIns;
using PKHeX.Web;
using PKHeX.Web.Extensions;
using PKHeX.Web.Services;
using PKHeX.Web.Services.Plugins;
using App = PKHeX.Web.App;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.HostEnvironment.Environment}.json", optional: true, reloadOnChange: true);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton(Session.Current);
builder.Services.AddScoped<GameService>();
builder.Services.AddSingleton<ReactApp>();

builder.Services.AddSingleton(sp => new PlugInHost(sp.GetRequiredService<Session>()));
builder.Services.AddScoped<PlugInStore>();
builder.Services.AddScoped<PlugInRanHandler>();

builder.Services.AddScoped<UserJourneyService>();
builder.Services.AddSingleton<AnalyticsService>();
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

builder.Services.AddSingleton<ILoggerProvider, ErrorReportingLoggerProvider>();

var app = builder.Build();

// Although Blazor WASM can target the whole .NET Framework API surface,
// during the Runtime, Microsoft has disabled the native support to some APIs under the System.Security.Cryptography namespace
// During startup we replace PKHeX unsupported cryptography APIs with a javascript-based alternative 
RuntimeCryptographyProvider.Aes = app.Services.GetRequiredService<BlazorAesProvider>();
RuntimeCryptographyProvider.Md5 = app.Services.GetRequiredService<BlazorMd5Provider>();

await app.RunAsync();