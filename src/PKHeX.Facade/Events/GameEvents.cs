using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade.Events;

public sealed class GameEvents
{
    private readonly IEventStore _store;

    private GameEvents(IEventStore store, Gen3Events? gen3)
    {
        _store = store;
        Gen3 = gen3;
    }

    public ImmutableList<EventFlagEntry> Flags => _store.Flags;
    public ImmutableList<EventWorkEntry> Work => _store.Work;
    public Gen3Events? Gen3 { get; }

    public int FlagCount => _store.FlagCount;
    public int WorkCount => _store.WorkCount;
    public int WorkMin => _store.WorkMin;
    public int WorkMax => _store.WorkMax;

    public bool GetFlag(int index)
    {
        EnsureInRange(index, FlagCount);
        return _store.GetFlag(index);
    }

    public void SetFlag(int index, bool value)
    {
        EnsureInRange(index, FlagCount);
        _store.SetFlag(index, value);
    }

    public int GetWork(int index)
    {
        EnsureInRange(index, WorkCount);
        return _store.GetWork(index);
    }

    public void SetWork(int index, int value)
    {
        EnsureInRange(index, WorkCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(value, WorkMin);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, WorkMax);
        _store.SetWork(index, value);
    }

    private static void EnsureInRange(int index, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, count);
    }

    internal static GameEvents? For(Game game)
    {
        var saveFile = game.SaveFile;
        IEventStore? store = saveFile switch
        {
            IEventFlag37 s => new WorkspaceStore<IEventFlag37, ushort>(s, saveFile.Version),
            IEventFlagProvider37 p => new WorkspaceStore<IEventFlag37, ushort>(p.EventWork, saveFile.Version),
            SAV2 s => new WorkspaceStore<SAV2, byte>(s, saveFile.Version),
            SAV7b s => new LetsGoStore(s.EventWork),
            SAV8BS s => new BrilliantDiamondShiningPearlStore(s.FlagWork),
            _ => null,
        };

        return store is null || store.Flags.Count == 0 ? null : new GameEvents(store, Gen3Events.For(game));
    }
}

public sealed class EventFlagEntry(int index, string name, string category, Func<bool> get, Action<bool> set)
{
    public int Index { get; } = index;
    public string Name { get; } = name;
    public string Category { get; } = category;

    public bool Value
    {
        get => get();
        set => set(value);
    }
}

public sealed class EventWorkEntry(
    int index,
    string name,
    string category,
    ImmutableList<EventWorkOption> options,
    Func<int> get,
    Action<int> set)
{
    public int Index { get; } = index;
    public string Name { get; } = name;
    public string Category { get; } = category;
    public ImmutableList<EventWorkOption> Options { get; } = options;

    public int Value
    {
        get => get();
        set => set(value);
    }
}

public sealed record EventWorkOption(string Name, int Value)
{
    internal static ImmutableList<EventWorkOption> From(IEnumerable<NamedEventConst> predefined) => predefined
        .Where(v => !v.IsCustom)
        .Select(v => new EventWorkOption(v.Name, v.Value))
        .ToImmutableList();
}
