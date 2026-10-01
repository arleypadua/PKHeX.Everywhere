namespace PKHeX.Web.Services;

public class NewsService
{
    public List<News> AllNews => _news.OrderByDescending(n => n.Date).ToList();
    public DateOnly LatestNewsDate => _news.MaxBy(n => n.Date)!.Date;

    public record News(DateOnly Date, List<string> Items);

    private readonly List<News> _news =
    [
        new(new(2024, 11, 29),
        [
            "Introduced the PKHeX.Web Cloud (alpha) with up to 6 Pokémon stored in the cloud.",
        ]),

        new(new(2024, 11, 30),
        [
            "Migrated to .NET 9",
            "Upgraded PKHeX.Core and ALM plugins to latest",
            "Added support for Battle Points.",
        ]),

        new(new(2025, 2, 14),
        [
            "Minor bug fixes",
            "News banner",
        ]),

        new(new(2025, 3, 25),
        [
            "Introduced plugin error pages showing a list of errors whenever executing a plugin",
            "Upgraded PKHeX.Core and ALM plugins to latest version",
        ]),

        new(new(2025, 5, 1),
        [
            "Fixed exporting of Let's Go Eevee save files.",
            "Update to privacy policy and cookie consent.",
            "Upgraded PKHeX.Core and ALM plugins to latest version",
        ]),

        new(new(2026, 3, 20),
        [
            "Upgraded PKHeX.Core and ALM plugins to latest version",
            "Upgraded from .NET 9.x to 10.x",
            "Small bug fixes",
        ]),

        new(new(2026, 10, 1),
        [
            "Added an Alpha toggle to the Pokémon editor.",
            "Added a Show all moves checkbox to the Moves tab.",
            "The Stats tab shows Hidden Power type and power.",
            "Imported Pokémon files convert to the loaded save's format.",
            "Action menus open on click instead of hover.",
            "Encounter Search preselects the game version on your first visit.",
            "Trainer name is editable on the Home page.",
            "Pre-Gen7 Pokémon show their 16-bit OT TID.",
            "Gen1/2 Pokémon of the same species no longer open the wrong one.",
            "Plug-in updates no longer crash on a 404.",
            "Search works in dropdowns again.",
            "Empty Legends: Arceus saves without item pockets load.",
            "The analytics page shows an alert when results fail to load.",
            "Money edits are disabled on Legends: Z-A saves without a money block.",
            "TM item icons load, and unnamed Z-A items are skipped.",
            "The Moves tab no longer crashes on Battle Revolution saves or unknown moves.",
            "The Pokémon editor no longer crashes on species missing from the loaded game.",
            "Demo loads a bundled Emerald save.",
            "Encounter Search sends you to the load page when no save is loaded.",
            "Let's Go party edits no longer crash on saves with empty slots.",
            "Malformed save files show an error instead of crashing.",
            "Fixed a unique id error in box rows.",
        ]),
    ];
}
