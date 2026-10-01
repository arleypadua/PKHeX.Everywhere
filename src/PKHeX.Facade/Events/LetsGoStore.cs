using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade.Events;

internal sealed class LetsGoStore : IEventStore
{
    private const string LabelFile = "gg";

    private readonly EventWork7b _block;

    public LetsGoStore(EventWork7b block)
    {
        _block = block;

        var editor = new SplitEventEditor<int>(block,
            GameLanguage.GetStrings(LabelFile, GameInfo.CurrentLanguage, "const"),
            GameLanguage.GetStrings(LabelFile, GameInfo.CurrentLanguage, "flags"));

        Flags = editor.Flag
            .SelectMany(g => g.Vars)
            .Select(v => new EventFlagEntry(v.RawIndex, v.Name, v.Type.ToString(),
                () => GetFlag(v.RawIndex),
                value => SetFlag(v.RawIndex, value)))
            .ToImmutableList();
        Work = editor.Work
            .SelectMany(g => g.Vars)
            .OfType<EventWork<int>>()
            .Select(v => new EventWorkEntry(v.RawIndex, v.Name, v.Type.ToString(),
                v.Options
                    .Where(o => !o.Custom)
                    .Select(o => new EventWorkOption(o.Text, o.Value))
                    .ToImmutableList(),
                () => GetWork(v.RawIndex),
                value => SetWork(v.RawIndex, value)))
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
