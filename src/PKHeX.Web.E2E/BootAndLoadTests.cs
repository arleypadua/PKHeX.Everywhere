using AwesomeAssertions;
using Microsoft.Playwright;
using PKHeX.Facade;
using PKHeX.Web.E2E.Infrastructure;

namespace PKHeX.Web.E2E;

public class BootAndLoadTests(WebAppFixture fixture) : E2ETest(fixture)
{
    [Fact]
    public Task OpeningASave_ShowsTheTrainer() => RunAsync("open", async page =>
    {
        const string saveFile = "data/save/savedata_4hgss.dsv";

        await page.BootAsync();
        await page.UploadSaveAsync(saveFile);

        await ExpectHomeToShowTheTrainerOfAsync(page, saveFile);
    });

    [Fact]
    public Task DroppingASave_ShowsTheTrainer() => RunAsync("drop", async page =>
    {
        const string saveFile = "data/save/emerald.sav";
        var openButton = page.GetByRole(AriaRole.Button, new() { Name = "Open", Exact = true });
        var dropOverlay = page.GetByRole(AriaRole.Dialog, new() { Name = "Drop to open save", Exact = true });

        await page.BootAsync();

        await using var drag = await openButton.DragFileOverAsync(saveFile);
        await Assertions.Expect(dropOverlay).ToBeVisibleAsync();

        await drag.DropOnAsync(dropOverlay);
        await Assertions.Expect(dropOverlay).ToBeHiddenAsync();

        await ExpectHomeToShowTheTrainerOfAsync(page, saveFile);
    });

    [Fact]
    public Task Demo_LoadsTheBundledSave() => RunAsync("demo", async page =>
    {
        await page.BootAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Demo", Exact = true }).ClickAsync();

        await ExpectHomeToShowTheTrainerOfAsync(page, "data/save/emerald.sav");
    });

    private async Task ExpectHomeToShowTheTrainerOfAsync(IPage page, string saveFile)
    {
        var trainerName = Game.LoadFrom(saveFile).Trainer.Name;
        trainerName.Should().NotBeNullOrWhiteSpace();

        await Assertions.Expect(page).ToHaveURLAsync(new Uri(BaseAddress, "/").ToString());
        await Assertions.Expect(page.GetByRole(AriaRole.Cell, new() { Name = trainerName, Exact = true })).ToBeVisibleAsync();
    }
}
