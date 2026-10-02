using PKHeX.Facade.Events;

namespace PKHeX.Everywhere.Engine.Dtos;

/// <summary>
/// A named event flag: an on/off story or progress marker in the save.
/// </summary>
/// <param name="Index">The flag's index, which <c>events.flag</c> and <c>events.setFlag</c> take.</param>
/// <param name="Name">The flag's label from PKHeX.</param>
/// <param name="Category">The PKHeX label category, such as <c>Misc</c>.</param>
/// <param name="Value">Whether the flag is set.</param>
public record EventFlag(int Index, string Name, string Category, bool Value);

/// <summary>
/// A known value for an event work entry, with its label.
/// </summary>
public record WorkOption(string Name, int Value);

/// <summary>
/// A named event work entry: a numeric story or progress value in the save.
/// </summary>
/// <param name="Index">The entry's index, which <c>events.setWork</c> takes.</param>
/// <param name="Name">The entry's label from PKHeX.</param>
/// <param name="Category">The PKHeX label category, such as <c>Misc</c>.</param>
/// <param name="Options">Known values for this entry. Empty when PKHeX knows none; any value between <c>workMin</c> and <c>workMax</c> is still accepted.</param>
public record EventWork(int Index, string Name, string Category, int Value, WorkOption[] Options);

/// <summary>
/// The Generation 3 event tickets and, in Emerald, the island access flags.
/// </summary>
/// <param name="Tickets">Names of the event tickets this game's Key Items pouch can hold. <c>events.giveTickets</c> adds the missing ones.</param>
/// <param name="AnyTicketMissing">Whether at least one of the tickets is not in the Key Items pouch.</param>
/// <param name="OldSeaMapNeedsConfirmation">Whether the Old Sea Map is missing from a non-Japanese save. It was only distributed in Japan, so ask the user before passing <c>includeOldSeaMap</c> to <c>events.giveTickets</c>.</param>
/// <param name="Islands">Flags for ferry access and the event islands. Empty outside Emerald.</param>
public record TicketsAndIslands(string[] Tickets, bool AnyTicketMissing, bool OldSeaMapNeedsConfirmation, EventFlag[] Islands);

/// <summary>
/// The save's event flags and work, returned by <c>events.get</c>, which returns null for saves without them.
/// </summary>
/// <param name="Flags">The flags PKHeX has labels for. The save can have more.</param>
/// <param name="Work">The work entries PKHeX has labels for. The save can have more.</param>
/// <param name="FlagCount">How many flags the save has. <c>events.setFlag</c> accepts indexes from 0 to <c>flagCount - 1</c>.</param>
/// <param name="WorkCount">How many work entries the save has. <c>events.setWork</c> accepts indexes from 0 to <c>workCount - 1</c>.</param>
/// <param name="WorkMin">The lowest value <c>events.setWork</c> accepts.</param>
/// <param name="WorkMax">The highest value <c>events.setWork</c> accepts.</param>
/// <param name="Gen3">Event tickets and islands. Null outside Ruby, Sapphire, Emerald, FireRed and LeafGreen.</param>
public record SaveEvents(
    EventFlag[] Flags,
    EventWork[] Work,
    int FlagCount,
    int WorkCount,
    int WorkMin,
    int WorkMax,
    TicketsAndIslands? Gen3);

public static class EventsMapping
{
    public static SaveEvents ToSaveEvents(this GameEvents events) => new(
        events.Flags.Select(ToEventFlag).ToArray(),
        events.Work
            .Select(w => new EventWork(w.Index, w.Name, w.Category, w.Value,
                w.Options.Select(o => new WorkOption(o.Name, o.Value)).ToArray()))
            .ToArray(),
        events.FlagCount,
        events.WorkCount,
        events.WorkMin,
        events.WorkMax,
        events.Gen3 is { } gen3
            ? new TicketsAndIslands(
                gen3.Tickets.All.Select(t => t.Name).ToArray(),
                !gen3.Tickets.Missing.IsEmpty,
                gen3.Tickets.OldSeaMapNeedsConfirmation,
                gen3.Islands.Select(ToEventFlag).ToArray())
            : null);

    private static EventFlag ToEventFlag(EventFlagEntry flag) => new(flag.Index, flag.Name, flag.Category, flag.Value);
}
