using System.Collections.Immutable;
using System.Numerics;
using PKHeX.Core;

namespace PKHeX.Facade.Events;

internal sealed class WorkspaceStore<TSave, TWork> : IEventStore
    where TSave : class, IEventFlagArray, IEventWorkArray<TWork>
    where TWork : unmanaged, IBinaryInteger<TWork>, IMinMaxValue<TWork>
{
    private readonly TSave _save;

    public WorkspaceStore(TSave save, GameVersion version)
        : this(save, new EventWorkspace<TSave, TWork>(save, version).Labels)
    {
    }

    private WorkspaceStore(TSave save, EventLabelCollection? labels)
    {
        _save = save;

        Flags = labels?.Flag
            .Select(l => new EventFlagEntry(l.Index, l.Name, l.Type.ToString(),
                () => GetFlag(l.Index),
                v => SetFlag(l.Index, v)))
            .ToImmutableList() ?? [];
        Work = labels?.Work
            .Select(l => new EventWorkEntry(l.Index, l.Name, l.Type.ToString(),
                EventWorkOption.From(l.PredefinedValues),
                () => GetWork(l.Index),
                v => SetWork(l.Index, v)))
            .ToImmutableList() ?? [];
    }

    // EventWorkspace throws for games PKHeX has no label files for, such as Generation 1.
    public static WorkspaceStore<TSave, TWork> Unlabelled(TSave save) => new(save, labels: null);

    public ImmutableList<EventFlagEntry> Flags { get; }
    public ImmutableList<EventWorkEntry> Work { get; }

    public int FlagCount => _save.EventFlagCount;
    public int WorkCount => _save.EventWorkCount;
    public int WorkMin => int.CreateChecked(TWork.MinValue);
    public int WorkMax => int.CreateChecked(TWork.MaxValue);

    public bool GetFlag(int index) => _save.GetEventFlag(index);

    public void SetFlag(int index, bool value)
    {
        _save.SetEventFlag(index, value);
        UpdateQrConstants();
    }

    public int GetWork(int index) => int.CreateChecked(_save.GetWork(index));

    public void SetWork(int index, int value)
    {
        _save.SetWork(index, TWork.CreateChecked(value));
        UpdateQrConstants();
    }

    // Writes go straight to the save rather than through EventWorkspace.Save, which would overwrite flags changed
    // elsewhere with its stale copy, so its Gen7 QR fix-up is repeated here.
    private void UpdateQrConstants()
    {
        if (_save is EventWork7 sm)
            sm.UpdateQrConstants();
    }
}
