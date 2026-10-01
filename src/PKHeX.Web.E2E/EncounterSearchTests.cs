using System.Text.RegularExpressions;
using Microsoft.Playwright;
using PKHeX.Web.E2E.Infrastructure;

namespace PKHeX.Web.E2E;

public class EncounterSearchTests(WebAppFixture fixture) : E2ETest(fixture)
{
    [Fact]
    public Task OpeningEncounterSearchWithoutASave_RedirectsToLoad() => RunAsync("no-save", async page =>
    {
        await page.BootAsync();

        await page.GotoAsync("/pokemon/search-encounter");

        await Assertions.Expect(page).ToHaveURLAsync(new Uri(BaseAddress, "/load").ToString());
        await Assertions.Expect(page.GetByText("Expected game to be loaded")).ToBeHiddenAsync();
    });

    [Fact]
    public Task AddingAnEncounterFromTheBox_OpensTheNewPokemon() => RunAsync("emerald", async page =>
    {
        await page.BootAsync();
        await page.UploadSaveAsync("data/save/emerald.sav");

        await page.NavigateWithMenuAsync("Pokemon Box", new Uri(BaseAddress, "/pokemon-box"));
        await page.GetByRole(AriaRole.Button, new() { Name = "Add", Exact = true }).ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync(new Uri(BaseAddress, "/pokemon/search-encounter").ToString());

        await PickAsync(page, "Version", "Ruby");
        await PickAsync(page, "Species", "Abra");
        var search = new Regex(@"/pokemon/search-encounter\?version=2&species=63$");
        await Assertions.Expect(page).ToHaveURLAsync(search);

        var addButtons = page.GetByRole(AriaRole.Button, new() { Name = "Add to box", Exact = true });
        await addButtons.First.ClickAsync();

        await Assertions.Expect(page.GetByText("Abra added to your box")).ToBeVisibleAsync();
        await Assertions.Expect(page).ToHaveURLAsync(new Regex("/pokemon/box/[^/]+$"));
        await Assertions.Expect(page.GetByTestId("pokemon-species")).ToHaveTextAsync("Abra");

        await page.GoBackAsync();
        await Assertions.Expect(page).ToHaveURLAsync(search);
        await Assertions.Expect(addButtons.First).ToBeVisibleAsync();
    });

    private static async Task PickAsync(IPage page, string select, string option)
    {
        var combobox = page.GetByRole(AriaRole.Combobox, new() { Name = select, Exact = true });
        await combobox.ClickAsync();
        await combobox.FillAsync(option);
        await combobox.PressAsync("Enter");
    }
}
