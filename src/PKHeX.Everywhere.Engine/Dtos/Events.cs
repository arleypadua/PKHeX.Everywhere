using PKHeX.Facade.Events;

namespace PKHeX.Everywhere.Engine.Dtos;

public record EventFlag(int Index, string Name, string Category, bool Value);

public record WorkOption(string Name, int Value);

public record EventWork(int Index, string Name, string Category, int Value, WorkOption[] Options);

public record TicketsAndIslands(string[] Tickets, bool AnyTicketMissing, bool OldSeaMapNeedsConfirmation, EventFlag[] Islands);

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
