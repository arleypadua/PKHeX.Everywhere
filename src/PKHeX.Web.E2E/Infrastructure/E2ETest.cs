using Microsoft.Playwright;

namespace PKHeX.Web.E2E.Infrastructure;

[Collection(WebAppCollection.Name)]
[Trait("Category", "E2E")]
public abstract class E2ETest(WebAppFixture fixture)
{
    public static readonly string ArtifactsDirectory = Path.Combine(AppContext.BaseDirectory, "playwright-artifacts");

    protected Uri BaseAddress => fixture.BaseAddress;

    protected async Task RunAsync(string name, Func<IPage, Task> test)
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
            var fileName = string.Concat(name.Split(Path.GetInvalidFileNameChars()));
            await page.ScreenshotAsync(new() { Path = Path.Combine(ArtifactsDirectory, $"{fileName}.png"), FullPage = true });
            await context.Tracing.StopAsync(new() { Path = Path.Combine(ArtifactsDirectory, $"{fileName}.zip") });
            await File.WriteAllLinesAsync(Path.Combine(ArtifactsDirectory, $"{fileName}.console.log"), console);
            throw;
        }
    }
}
