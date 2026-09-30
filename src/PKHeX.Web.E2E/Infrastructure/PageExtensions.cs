using Microsoft.Playwright;

namespace PKHeX.Web.E2E.Infrastructure;

public static class PageExtensions
{
    public static async Task BootAsync(this IPage page)
    {
        await page.GotoAsync("/");
        var decline = page.GetByRole(AriaRole.Button, new() { Name = "Decline", Exact = true });
        await decline.ClickAsync();
        await Assertions.Expect(decline).ToBeHiddenAsync();
    }

    public static async Task UploadSaveAsync(this IPage page, string saveFile)
    {
        await page.GetByRole(AriaRole.Button, new() { Name = "Open" }).WaitForAsync();
        await page.Locator("input[type=file]").SetInputFilesAsync(saveFile);
    }
}
