using System.Diagnostics;
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

    public static async Task<WebAppHost> StartAsync(string wwwroot)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { WebRootPath = wwwroot });
        builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");

        var app = builder.Build();
        var files = new PhysicalFileProvider(wwwroot);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = files,
            ServeUnknownFileTypes = true,
        });
        app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = files });

        await app.StartAsync();

        return new WebAppHost(app, new Uri(app.Urls.Single()));
    }

    public static async Task<string> PublishAsync(string outputDirectory)
    {
        var project = Path.Combine(RepositoryRoot(), "src", "PKHeX.Web", "PKHeX.Web.csproj");
        var startInfo = new ProcessStartInfo("dotnet")
        {
            ArgumentList = { "publish", project, "-c", "Release", "-o", outputDirectory, "-nodeReuse:false" },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        using var process = Process.Start(startInfo)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"dotnet publish failed:\n{await stdout}\n{await stderr}");

        return Path.Combine(outputDirectory, "wwwroot");
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
