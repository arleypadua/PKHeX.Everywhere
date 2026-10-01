namespace PKHeX.Facade.Events;

internal interface IEventStore
{
    IReadOnlyList<EventFlagEntry> Flags { get; }
    IReadOnlyList<EventWorkEntry> Work { get; }

    int FlagCount { get; }
    int WorkCount { get; }
    int WorkMin { get; }
    int WorkMax { get; }

    bool GetFlag(int index);
    void SetFlag(int index, bool value);
    int GetWork(int index);
    void SetWork(int index, int value);
}
