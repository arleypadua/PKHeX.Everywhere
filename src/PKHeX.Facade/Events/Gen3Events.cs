using System.Collections.Immutable;
using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Events;

public sealed class Gen3Events
{
    private Gen3Events(EventTickets tickets, ImmutableList<EventFlagEntry> islands)
    {
        Tickets = tickets;
        Islands = islands;
    }

    public EventTickets Tickets { get; }
    public ImmutableList<EventFlagEntry> Islands { get; }

    internal static Gen3Events? For(Game game) => game.SaveFile switch
    {
        SAV3E emerald => new Gen3Events(new EventTickets(game), EmeraldIslands(emerald)),
        SAV3 => new Gen3Events(new EventTickets(game), []),
        _ => null,
    };

    private static ImmutableList<EventFlagEntry> EmeraldIslands(SAV3E save) =>
    [
        Island(save, 0x864, "Can ride the ferry"),
        Island(save, 0x8B3, "Reachable: Southern Island"),
        Island(save, 0x8D5, "Reachable: Birth Island"),
        Island(save, 0x8D6, "Reachable: Faraway Island"),
        Island(save, 0x8E0, "Reachable: Navel Rock"),
        Island(save, 0x1D0, "Reachable: Battle Frontier"),
        Island(save, 0x1AE, "Initial event: Southern Island"),
        Island(save, 0x1AF, "Initial event: Birth Island"),
        Island(save, 0x1B0, "Initial event: Faraway Island"),
        Island(save, 0x1DB, "Initial event: Navel Rock"),
    ];

    private static EventFlagEntry Island(SAV3E save, int flag, string name) =>
        new(flag, name, nameof(NamedEventType.Misc), () => save.GetEventFlag(flag), v => save.SetEventFlag(flag, v));
}

public sealed class EventTickets
{
    private const string KeyItems = nameof(InventoryType.KeyItems);
    private const ushort SsTicketId = 265;
    private const ushort EonTicketId = 275;
    private const ushort MysticTicketId = 370;
    private const ushort AuroraTicketId = 371;
    private const ushort OldSeaMapId = 376;
    private static readonly ushort[] TicketIds = [SsTicketId, EonTicketId, MysticTicketId, AuroraTicketId, OldSeaMapId];

    private readonly Game _game;

    internal EventTickets(Game game)
    {
        _game = game;
    }

    private Inventory Pouch => _game.Trainer.Inventories[KeyItems];

    public ImmutableList<ItemDefinition> All => TicketIds
        .Select(_game.ItemRepository.GetGameItem)
        .Where(Pouch.Supports)
        .ToImmutableList();

    public ImmutableList<ItemDefinition> Missing => All
        .Where(t => Pouch.Items.All(i => i.Id != t.Id))
        .ToImmutableList();

    public bool OldSeaMapNeedsConfirmation =>
        _game.SaveFile is SAV3 { Japanese: false } && Missing.Any(t => t.Id == OldSeaMapId);

    public ImmutableList<ItemDefinition> Give(bool includeOldSeaMap)
    {
        var toAdd = Missing
            .Where(t => includeOldSeaMap || t.Id != OldSeaMapId)
            .ToImmutableList();

        var freeSlots = Pouch.Items.Count(i => i.IsNone);
        if (toAdd.Count > freeSlots)
            throw new InvalidOperationException("Not enough space in the Key Items pouch.");

        foreach (var ticket in toAdd)
            Pouch.Set(ticket.Id, 1);

        return toAdd;
    }
}
