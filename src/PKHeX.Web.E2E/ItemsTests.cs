using AwesomeAssertions;
using Microsoft.Playwright;
using PKHeX.Facade;
using PKHeX.Web.E2E.Infrastructure;

namespace PKHeX.Web.E2E;

public class ItemsTests(WebAppFixture fixture) : E2ETest(fixture)
{
    private const string SaveFile = "data/save/emerald.sav";

    [Fact]
    public Task EditingAnItemCount_EndsUpInTheExportedSave() => RunAsync(Path.GetFileName(SaveFile), async page =>
    {
        var inventories = Game.LoadFrom(SaveFile).Trainer.Inventories;
        const string inventoryType = "Balls";
        var original = inventories[inventoryType].AllExceptNone().First();
        var count = original.Count == 7 ? 8 : 7;

        await page.BootAsync();
        await page.UploadSaveAsync(SaveFile);

        await page.NavigateWithMenuAsync("Items", new Uri(BaseAddress, "/items"));
        await page.GetByRole(AriaRole.Tab, new() { Name = inventoryType, Exact = true }).ClickAsync();
        var itemRow = page.GetByRole(AriaRole.Tabpanel).GetByRole(AriaRole.Row).Filter(new() { Has = page.GetByRole(AriaRole.Cell, new() { Name = original.Name, Exact = true }) });
        await itemRow.GetByRole(AriaRole.Button, new() { Name = "Edit", Exact = true }).ClickAsync();

        var dialog = page.GetByRole(AriaRole.Dialog);
        var countInput = dialog.GetByRole(AriaRole.Spinbutton);
        await countInput.FillAsync(count.ToString());
        await countInput.BlurAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "OK", Exact = true }).ClickAsync();
        await Assertions.Expect(dialog).ToBeHiddenAsync();
        await Assertions.Expect(itemRow.GetByRole(AriaRole.Cell, new() { Name = count.ToString(), Exact = true })).ToBeVisibleAsync();

        var exported = await page.ExportSaveAsync(BaseAddress);

        exported.Trainer.Inventories[inventoryType].Single(i => i.Id == original.Id).Count.Should().Be(count);
    });
}
