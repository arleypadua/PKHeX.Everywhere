using AwesomeAssertions;
using Microsoft.Playwright;
using PKHeX.Facade;
using PKHeX.Web.E2E.Infrastructure;

namespace PKHeX.Web.E2E;

public class BootAndLoadTests(WebAppFixture fixture) : E2ETest(fixture)
{
    [Theory]
    [InlineData("data/save/emerald.sav")]
    [InlineData("data/save/savedata_4hgss.dsv")]
    public Task LoadingASave_ShowsTheTrainer(string saveFile) => RunAsync(Path.GetFileName(saveFile), async page =>
    {
        var trainerName = Game.LoadFrom(saveFile).Trainer.Name;
        trainerName.Should().NotBeNullOrWhiteSpace();

        await page.BootAsync();
        await page.UploadSaveAsync(saveFile);

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
