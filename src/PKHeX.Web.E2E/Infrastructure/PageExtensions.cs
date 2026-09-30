using Microsoft.Playwright;
using PKHeX.Facade;

namespace PKHeX.Web.E2E.Infrastructure;

public static class PageExtensions
{
    public static async Task BootAsync(this IPage page)
    {
        await page.GotoAsync("/");
        await page.GetByRole(AriaRole.Button, new() { Name = "Open" }).WaitForAsync();
    }

    public static async Task UploadSaveAsync(this IPage page, string saveFile)
    {
        await page.GetByRole(AriaRole.Button, new() { Name = "Open" }).WaitForAsync();
        await page.Locator("input[type=file]").SetInputFilesAsync(saveFile);
    }

    public static async Task NavigateWithMenuAsync(this IPage page, string menuItem, Uri expected)
    {
        await page.GetByRole(AriaRole.Link, new() { Name = menuItem, Exact = true }).ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync(expected.ToString());
    }

    public static async Task<Game> ExportSaveAsync(this IPage page, Uri baseAddress)
    {
        await page.NavigateWithMenuAsync("Save", new Uri(baseAddress, "/save"));

        var download = await page.RunAndWaitForDownloadAsync(() =>
            page.GetByRole(AriaRole.Button, new() { Name = "Export", Exact = true }).ClickAsync());

        return Game.LoadFrom(await File.ReadAllBytesAsync(await download.PathAsync()), download.SuggestedFilename);
    }
}
