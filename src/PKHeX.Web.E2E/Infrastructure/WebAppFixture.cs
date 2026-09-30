using Microsoft.Playwright;

namespace PKHeX.Web.E2E.Infrastructure;

public sealed class WebAppFixture : IAsyncLifetime
{
    private WebAppHost? _host;
    private IPlaywright? _playwright;

    public Uri BaseAddress => _host!.BaseAddress;
    public IBrowser Browser { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        var wwwroot = await WebAppHost.PublishAsync(Path.Combine(AppContext.BaseDirectory, "pkhex-web-publish"));
        _host = await WebAppHost.StartAsync(wwwroot);

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null) await Browser.DisposeAsync();
        _playwright?.Dispose();
        if (_host is not null) await _host.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class WebAppCollection : ICollectionFixture<WebAppFixture>
{
    public const string Name = "PKHeX.Web";
}
