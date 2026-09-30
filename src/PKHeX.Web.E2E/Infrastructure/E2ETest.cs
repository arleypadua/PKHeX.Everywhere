using System.Runtime.CompilerServices;
using Microsoft.Playwright;

namespace PKHeX.Web.E2E.Infrastructure;

[Collection(WebAppCollection.Name)]
[Trait("Category", "E2E")]
public abstract class E2ETest(WebAppFixture fixture)
{
    private static readonly string ArtifactsDirectory = Path.Combine(AppContext.BaseDirectory, "playwright-artifacts");

    protected Uri BaseAddress => fixture.BaseAddress;

    protected async Task RunAsync(string variant, Func<IPage, Task> test, [CallerMemberName] string testName = "")
    {
        await using var context = await fixture.Browser.NewContextAsync(new() { BaseURL = BaseAddress.ToString() });
        await context.RouteAsync("**/*", route =>
            new Uri(route.Request.Url).Authority == BaseAddress.Authority
                ? route.ContinueAsync()
                : route.AbortAsync());

        await context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
        var page = await context.NewPageAsync();
        var console = new List<string>();
        page.Console += (_, message) => console.Add($"[{message.Type}] {message.Text}");
        page.PageError += (_, error) => console.Add($"[pageerror] {error}");

        try
        {
            await test(page);
            await context.Tracing.StopAsync();
        }
        catch
        {
            var artifact = Path.Combine(ArtifactsDirectory, string.Concat($"{testName}-{variant}".Split(Path.GetInvalidFileNameChars())));
            Directory.CreateDirectory(ArtifactsDirectory);
            await File.WriteAllLinesAsync($"{artifact}.console.log", console);
            await TryAsync(() => context.Tracing.StopAsync(new() { Path = $"{artifact}.zip" }));
            await TryAsync(() => page.ScreenshotAsync(new() { Path = $"{artifact}.png", FullPage = true }));
            throw;
        }
    }

    private static async Task TryAsync(Func<Task> capture)
    {
        try
        {
            await capture();
        }
        catch (PlaywrightException)
        {
        }
    }
}
