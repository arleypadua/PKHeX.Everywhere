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

        var levelInput = page.GetByTestId("pokemon-level").GetByRole(AriaRole.Spinbutton);
        await levelInput.FillAsync(level.ToString());
        await levelInput.BlurAsync();

        var nicknameField = page.GetByTestId("pokemon-nickname");
        await nicknameField.GetByRole(AriaRole.Button).ClickAsync();
        var nicknameInput = nicknameField.GetByRole(AriaRole.Textbox);
        await nicknameInput.FillAsync(nickname);
        await nicknameInput.BlurAsync();
        await Assertions.Expect(nicknameField).ToHaveTextAsync(nickname);

        var heldItemField = page.GetByTestId("pokemon-held-item");
        await heldItemField.ClickAsync();
        await heldItemField.Locator("input").PressSequentiallyAsync(heldItem);
        await page.GetByRole(AriaRole.Option, new() { Name = heldItem, Exact = true }).ClickAsync();
        await Assertions.Expect(heldItemField).ToContainTextAsync(heldItem);

        await page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
        await Assertions.Expect(page).Not.ToHaveURLAsync(pokemonPage.ToString());

        var exported = await page.ExportSaveAsync(BaseAddress);

        var edited = exported.Trainer.Party.Pokemons.Single(p => p.UniqueId.Equals(original.UniqueId));
        edited.Level.Should().Be(level);
        edited.Nickname.Should().Be(nickname);
        edited.HeldItem.Name.Should().Be(heldItem);
    });
}
