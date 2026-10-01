using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade.Events;

internal sealed class BrilliantDiamondShiningPearlStore : IEventStore
{
    private const string LabelFile = "bdsp";
    private const string SystemCategory = "System";

    private readonly FlagWork8b _block;

    public BrilliantDiamondShiningPearlStore(FlagWork8b block)
    {
        _block = block;

        var labels = new EventLabelCollectionSystem(LabelFile, block.CountFlag, block.CountSystem, block.CountWork);

        var flags = labels.Flag
            .Select(l => new EventFlagEntry(l.Index, l.Name, l.Type.ToString(),
                () => block.GetFlag(l.Index),
                v => block.SetFlag(l.Index, v)));
        var system = labels.System
            .Select(l => new EventFlagEntry(l.Index, l.Name, SystemCategory,
                () => block.GetSystemFlag(l.Index),
                v => block.SetSystemFlag(l.Index, v)));

        Flags = flags.Concat(system).ToImmutableList();
        Work = labels.Work
            .Select(l => new EventWorkEntry(l.Index, l.Name, l.Type.ToString(),
                EventWorkOption.From(l.PredefinedValues),
                () => block.GetWork(l.Index),
                v => block.SetWork(l.Index, v)))
            .ToImmutableList();
    }

    public ImmutableList<EventFlagEntry> Flags { get; }
    public ImmutableList<EventWorkEntry> Work { get; }

    public int FlagCount => _block.CountFlag;
    public int WorkCount => _block.CountWork;
    public int WorkMin => int.MinValue;
    public int WorkMax => int.MaxValue;

    public bool GetFlag(int index) => _block.GetFlag(index);
    public void SetFlag(int index, bool value) => _block.SetFlag(index, value);
    public int GetWork(int index) => _block.GetWork(index);
    public void SetWork(int index, int value) => _block.SetWork(index, value);
}
