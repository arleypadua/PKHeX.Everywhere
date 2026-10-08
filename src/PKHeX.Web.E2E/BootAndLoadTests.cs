using AwesomeAssertions;
using Microsoft.Playwright;
using PKHeX.Facade;
using PKHeX.Web.E2E.Infrastructure;

namespace PKHeX.Web.E2E;

public class BootAndLoadTests(WebAppFixture fixture) : E2ETest(fixture)
{
    [Theory]
    [InlineData("data/save/emerald.sav", true)]
    [InlineData("data/save/savedata_4hgss.dsv", false)]
    public Task LoadingASave_ShowsTheTrainer(string saveFile, bool drop) => RunAsync(Path.GetFileName(saveFile), async page =>
    {
        var trainerName = Game.LoadFrom(saveFile).Trainer.Name;
        trainerName.Should().NotBeNullOrWhiteSpace();

        await page.BootAsync();
        if (drop)
        {
            var bytes = Convert.ToBase64String(await File.ReadAllBytesAsync(saveFile));
            await using var dataTransfer = await page.EvaluateHandleAsync("""
                ({ bytes, name }) => {
                    const transfer = new DataTransfer();
                    transfer.items.add(new File([Uint8Array.from(atob(bytes), c => c.charCodeAt(0))], name));
                    return transfer;
                }
                """, new { bytes, name = Path.GetFileName(saveFile) });
            var overlay = page.GetByRole(AriaRole.Dialog, new() { Name = "Drop to open save", Exact = true });
            await Assertions.Expect(overlay).ToBeHiddenAsync();
            await page.Locator("body").DispatchEventAsync("dragenter", new { dataTransfer });
            await Assertions.Expect(overlay).ToBeVisibleAsync();
            await overlay.DispatchEventAsync("drop", new { dataTransfer });
            await Assertions.Expect(overlay).ToBeHiddenAsync();
        }
        else
        {
            await page.UploadSaveAsync(saveFile);
        }

        await Assertions.Expect(page).ToHaveURLAsync(new Uri(BaseAddress, "/").ToString());
        await Assertions.Expect(page.GetByRole(AriaRole.Cell, new() { Name = trainerName, Exact = true })).ToBeVisibleAsync();
    });

    [Fact]
    public Task Demo_LoadsTheBundledSave() => RunAsync("demo", async page =>
    {
        var trainerName = Game.LoadFrom("data/save/emerald.sav").Trainer.Name;
        trainerName.Should().NotBeNullOrWhiteSpace();

        await page.BootAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Demo", Exact = true }).ClickAsync();

        await Assertions.Expect(page).ToHaveURLAsync(new Uri(BaseAddress, "/").ToString());
        await Assertions.Expect(page.GetByRole(AriaRole.Cell, new() { Name = trainerName, Exact = true })).ToBeVisibleAsync();
    });
}
