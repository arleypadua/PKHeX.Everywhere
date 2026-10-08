using Microsoft.Playwright;
using PKHeX.Facade;

namespace PKHeX.Web.E2E.Infrastructure;

public static class PageExtensions
{
    public static async Task BootAsync(this IPage page)
    {
        await page.GotoAsync("/");
        await page.GetByRole(AriaRole.Button, new() { Name = "Open", Exact = true }).WaitForAsync();
    }

    public static async Task UploadSaveAsync(this IPage page, string saveFile)
    {
        await page.GetByRole(AriaRole.Button, new() { Name = "Open", Exact = true }).WaitForAsync();
        await page.Locator("input[type=file]").SetInputFilesAsync(saveFile);
    }

    public static async Task<FileDrag> DragFileOverAsync(this ILocator target, string file)
    {
        var bytes = Convert.ToBase64String(await File.ReadAllBytesAsync(file));
        var dataTransfer = await target.Page.EvaluateHandleAsync("""
            ({ bytes, name }) => {
                const transfer = new DataTransfer();
                transfer.items.add(new File([Uint8Array.from(atob(bytes), c => c.charCodeAt(0))], name));
                return transfer;
            }
            """, new { bytes, name = Path.GetFileName(file) });
        await target.DispatchEventAsync("dragenter", new { dataTransfer });
        return new FileDrag(dataTransfer);
    }

    public static async Task NavigateWithMenuAsync(this IPage page, string menuItem, Uri expected)
    {
        await page.GetByRole(AriaRole.Link, new() { Name = menuItem, Exact = true }).ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync(expected.ToString());
    }

    // antd renders a select's chosen label next to its combobox input, not inside it.
    public static ILocator SelectValue(this IPage page, string label) =>
        page.GetByRole(AriaRole.Combobox, new() { Name = label, Exact = true }).Locator("..");

    public static async Task<Game> ExportSaveAsync(this IPage page, Uri baseAddress)
    {
        await page.NavigateWithMenuAsync("Save", new Uri(baseAddress, "/save"));

        var download = await page.RunAndWaitForDownloadAsync(() =>
            page.GetByRole(AriaRole.Button, new() { Name = "Export", Exact = true }).ClickAsync());

        return Game.LoadFrom(await File.ReadAllBytesAsync(await download.PathAsync()), download.SuggestedFilename);
    }
}
