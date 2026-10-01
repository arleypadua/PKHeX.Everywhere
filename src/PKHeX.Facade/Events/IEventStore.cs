using System.Collections.Immutable;

namespace PKHeX.Facade.Events;

internal interface IEventStore
{
    ImmutableList<EventFlagEntry> Flags { get; }
    ImmutableList<EventWorkEntry> Work { get; }

    int FlagCount { get; }
    int WorkCount { get; }
    int WorkMin { get; }
    int WorkMax { get; }

    bool GetFlag(int index);
    void SetFlag(int index, bool value);
    int GetWork(int index);
    void SetWork(int index, int value);
}
