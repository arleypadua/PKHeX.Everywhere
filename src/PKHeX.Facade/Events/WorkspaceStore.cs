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
    {
        _save = save;
        var labels = new EventWorkspace<TSave, TWork>(save, version).Labels;

        Flags = labels.Flag
            .Select(l => new EventFlagEntry(l.Index, l.Name, l.Type.ToString(),
                () => GetFlag(l.Index),
                v => SetFlag(l.Index, v)))
            .ToImmutableList();
        Work = labels.Work
            .Select(l => new EventWorkEntry(l.Index, l.Name, l.Type.ToString(),
                l.PredefinedValues
                    .Where(v => !v.IsCustom)
                    .Select(v => new EventWorkOption(v.Name, v.Value))
                    .ToImmutableList(),
                () => GetWork(l.Index),
                v => SetWork(l.Index, v)))
            .ToImmutableList();
    }

    public IReadOnlyList<EventFlagEntry> Flags { get; }
    public IReadOnlyList<EventWorkEntry> Work { get; }

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
