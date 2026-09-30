using System.Text.RegularExpressions;
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
        var party = trainer.Party.Pokemons.Select(p => p.Species.Name).ToList();
        var boxed = trainer.PokemonBox.All.First(p => p.Species.Species != Species.None).Species.Name;
        party.Should().NotBeEmpty();

        await page.BootAsync();
        await page.UploadSaveAsync(saveFile);

        await page.GetByRole(AriaRole.Link, new() { Name = "Party", Exact = true }).ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync(new Uri(BaseAddress, "/party").ToString());
        foreach (var species in party)
            await Assertions.Expect(page.GetByRole(AriaRole.Cell, new() { Name = species, Exact = true }).First).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Link, new() { Name = "Pokemon Box", Exact = true }).ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync(new Uri(BaseAddress, "/pokemon-box").ToString());
        var row = page.GetByRole(AriaRole.Row).Filter(new() { Has = page.GetByRole(AriaRole.Cell, new() { Name = boxed, Exact = true }) }).First;
        await Assertions.Expect(row).ToBeVisibleAsync();

        await row.GetByRole(AriaRole.Button, new() { Name = "View", Exact = true }).ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync(new Regex("/pokemon/box/"));
        await Assertions.Expect(page.GetByTestId("pokemon-species")).ToHaveTextAsync(boxed);
    });
}
