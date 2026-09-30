using Microsoft.Playwright;
using PKHeX.Facade;

namespace PKHeX.Web.E2E.Infrastructure;

public static class SaveExport
{
    public static async Task<Game> ExportSaveAsync(this IPage page, Uri baseAddress)
    {
        await page.NavigateWithMenuAsync("Save", new Uri(baseAddress, "/save"));

        var download = await page.RunAndWaitForDownloadAsync(() =>
            page.GetByRole(AriaRole.Button, new() { Name = "Export", Exact = true }).ClickAsync());

        var path = await download.PathAsync();
        return Game.LoadFrom(await File.ReadAllBytesAsync(path), download.SuggestedFilename);
    }
}
