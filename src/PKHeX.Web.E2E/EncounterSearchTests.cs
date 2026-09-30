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
        await Assertions.Expect(page.GetByRole(AriaRole.Alert).Filter(new() { HasText = "Expected game to be loaded" })).ToBeHiddenAsync();
    });
}
