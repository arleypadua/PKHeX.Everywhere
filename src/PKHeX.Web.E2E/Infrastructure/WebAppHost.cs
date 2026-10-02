using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace PKHeX.Web.E2E.Infrastructure;

public sealed class WebAppHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    private WebAppHost(WebApplication app, Uri baseAddress)
    {
        _app = app;
        BaseAddress = baseAddress;
    }

    public Uri BaseAddress { get; }

    public static async Task<WebAppHost> StartAsync(string root)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { WebRootPath = root });
        builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        var app = builder.Build();
        var files = new PhysicalFileProvider(root);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = files,
            ServeUnknownFileTypes = true,
        });
        app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = files });

        await app.StartAsync();

        return new WebAppHost(app, new Uri(app.Urls.Single()));
    }

    public static string Dist()
    {
        var dist = Path.Combine(RepositoryRoot(), "src", "PKHeX.Web.React", "dist");
        if (!File.Exists(Path.Combine(dist, "index.html")))
            throw new InvalidOperationException($"{dist} is missing. Run 'npm run build' in src/PKHeX.Web.React first.");

        return dist;
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "PKHeX.CLI.sln")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found");
    }

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
