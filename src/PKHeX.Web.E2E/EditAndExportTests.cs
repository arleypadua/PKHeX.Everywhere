using AwesomeAssertions;
using Microsoft.Playwright;
using PKHeX.Facade;
using PKHeX.Web.E2E.Infrastructure;

namespace PKHeX.Web.E2E;

public class EditAndExportTests(WebAppFixture fixture) : E2ETest(fixture)
{
    [Theory]
    [InlineData("data/save/emerald.sav")]
    [InlineData("data/save/savedata_4hgss.dsv")]
    public Task EditingAPokemon_EndsUpInTheExportedSave(string saveFile) => RunAsync(Path.GetFileName(saveFile), async page =>
    {
        var original = Game.LoadFrom(saveFile).Trainer.Party.Pokemons.First();
        var level = original.Level == 50 ? 51 : 50;
        const string nickname = "Roundtrip";
        const string heldItem = "Leftovers";
        original.HeldItem.Name.Should().NotBe(heldItem);

        await page.BootAsync();
        await page.UploadSaveAsync(saveFile);

        await page.NavigateWithMenuAsync("Party", new Uri(BaseAddress, "/party"));
        await page.GetByRole(AriaRole.Button, new() { Name = "View", Exact = true }).First.ClickAsync();
        var pokemonPage = new Uri(BaseAddress, $"/pokemon/party/{original.UniqueId}");
        await Assertions.Expect(page).ToHaveURLAsync(pokemonPage.ToString());

        var levelInput = page.GetByRole(AriaRole.Spinbutton, new() { Name = "Level", Exact = true });
        await levelInput.FillAsync(level.ToString());
        await levelInput.BlurAsync();
        await Assertions.Expect(levelInput).ToHaveValueAsync(level.ToString());

        await page.GetByRole(AriaRole.Button, new() { Name = "Edit", Exact = true }).ClickAsync();
        var nicknameInput = page.GetByRole(AriaRole.Textbox);
        await nicknameInput.FillAsync(nickname);
        await nicknameInput.PressAsync("Enter");
        await Assertions.Expect(page.GetByText(nickname, new() { Exact = true })).ToBeVisibleAsync();

        var heldItemSelect = page.GetByRole(AriaRole.Combobox, new() { Name = "Held Item", Exact = true });
        await heldItemSelect.ClickAsync();
        await heldItemSelect.PressSequentiallyAsync(heldItem);
        await page.GetByRole(AriaRole.Option, new() { Name = heldItem, Exact = true }).ClickAsync();
        await Assertions.Expect(page.SelectValue("Held Item")).ToContainTextAsync(heldItem);

        await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
        await Assertions.Expect(page).Not.ToHaveURLAsync(pokemonPage.ToString());

        var exported = await page.ExportSaveAsync(BaseAddress);

        var edited = exported.Trainer.Party.Pokemons.Single(p => p.UniqueId.Equals(original.UniqueId));
        edited.Level.Should().Be(level);
        edited.Nickname.Should().Be(nickname);
        edited.HeldItem.Name.Should().Be(heldItem);
    });
}
