using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Web;
using PKHeX.Web.Plugins;
using PKHeX.Web.Services;
using PKHeX.Web.Services.Plugins;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<EncounterService>();
builder.Services.AddScoped<AutoLegalityService>();

builder.Services.AddSingleton<PlugInSandbox>();
builder.Services.AddSingleton<JsService>();
builder.Services.AddSingleton<AntdThemeService>();
builder.Services.AddSingleton<ClipboardService>();
builder.Services.AddSingleton<BrowserWindowService.Instance>();

builder.Services.AddSingleton<BlazorAesProvider>();
builder.Services.AddSingleton<BlazorMd5Provider>();

builder.Services.AddAntDesign();

var app = builder.Build();

// Although Blazor WASM can target the whole .NET Framework API surface,
// during the Runtime, Microsoft has disabled the native support to some APIs under the System.Security.Cryptography namespace
// During startup we replace PKHeX unsupported cryptography APIs with a javascript-based alternative 
RuntimeCryptographyProvider.Aes = app.Services.GetRequiredService<BlazorAesProvider>();
RuntimeCryptographyProvider.Md5 = app.Services.GetRequiredService<BlazorMd5Provider>();

await app.RunAsync();