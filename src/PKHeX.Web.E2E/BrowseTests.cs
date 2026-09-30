using AwesomeAssertions;
using Microsoft.Playwright;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Web.E2E.Infrastructure;

namespace PKHeX.Web.E2E;

public class BrowseTests(WebAppFixture fixture) : E2ETest(fixture)
{
    [Theory]
    [InlineData("data/save/emerald.sav")]
    [InlineData("data/save/savedata_4hgss.dsv")]
    public Task BrowsingPartyAndBox_OpensAPokemon(string saveFile) => RunAsync(Path.GetFileName(saveFile), async page =>
    {
        var trainer = Game.LoadFrom(saveFile).Trainer;
        var partySpecies = trainer.Party.Pokemons.Select(p => p.Species.Name).ToList();
        partySpecies.Should().NotBeEmpty();
        var boxed = trainer.PokemonBox.All.First(p => p.Species.Species != Species.None);

        await page.BootAsync();
        await page.UploadSaveAsync(saveFile);

        await page.NavigateWithMenuAsync("Party", new Uri(BaseAddress, "/party"));
        await Assertions.Expect(PokemonRows(page)).ToHaveCountAsync(partySpecies.Count);
        foreach (var species in partySpecies)
            await Assertions.Expect(page.GetByRole(AriaRole.Cell, new() { Name = species, Exact = true }).First).ToBeVisibleAsync();

        await page.NavigateWithMenuAsync("Pokemon Box", new Uri(BaseAddress, "/pokemon-box"));
        var boxedRow = PokemonRows(page).Filter(new() { Has = page.GetByRole(AriaRole.Cell, new() { Name = boxed.Species.Name, Exact = true }) }).First;
        await Assertions.Expect(boxedRow).ToBeVisibleAsync();

        await boxedRow.GetByRole(AriaRole.Button, new() { Name = "View", Exact = true }).ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync(new Uri(BaseAddress, $"/pokemon/box/{boxed.UniqueId}").ToString());
        await Assertions.Expect(page.GetByTestId("pokemon-species")).ToHaveTextAsync(boxed.Species.Name);
    });

    private static ILocator PokemonRows(IPage page) =>
        page.GetByRole(AriaRole.Row).Filter(new() { Has = page.GetByRole(AriaRole.Button, new() { Name = "View", Exact = true }) });
}
