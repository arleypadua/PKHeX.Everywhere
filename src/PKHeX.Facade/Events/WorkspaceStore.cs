using System.Collections.Immutable;
using System.Numerics;
using PKHeX.Core;

namespace PKHeX.Facade.Events;

internal sealed class WorkspaceStore<TSave, TWork> : IEventStore
    where TSave : class, IEventFlagArray, IEventWorkArray<TWork>
    where TWork : unmanaged, IBinaryInteger<TWork>, IMinMaxValue<TWork>
{
    private readonly EventWorkspace<TSave, TWork> _workspace;

    public WorkspaceStore(TSave save, GameVersion version)
    {
        _workspace = new EventWorkspace<TSave, TWork>(save, version);

        Flags = _workspace.Labels.Flag
            .Select(l => new EventFlagEntry(l.Index, l.Name, l.Type.ToString(),
                () => GetFlag(l.Index),
                v => SetFlag(l.Index, v)))
            .ToImmutableList();
        Work = _workspace.Labels.Work
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

    public int FlagCount => _workspace.Flags.Length;
    public int WorkCount => _workspace.Values.Length;
    public int WorkMin => int.CreateChecked(TWork.MinValue);
    public int WorkMax => int.CreateChecked(TWork.MaxValue);

    public bool GetFlag(int index) => _workspace.Flags[index];

    public void SetFlag(int index, bool value)
    {
        _workspace.Flags[index] = value;
        _workspace.Save();
    }

    public int GetWork(int index) => int.CreateChecked(_workspace.Values[index]);

    public void SetWork(int index, int value)
    {
        _workspace.Values[index] = TWork.CreateChecked(value);
        _workspace.Save();
    }
}
