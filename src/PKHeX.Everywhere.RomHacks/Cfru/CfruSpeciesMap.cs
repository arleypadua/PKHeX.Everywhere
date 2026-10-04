namespace PKHeX.Everywhere.RomHacks.Cfru;

public sealed class CfruSpeciesMap
{
    private readonly CfruSpecies[] _nationalByIndex;
    private readonly Dictionary<ushort, HackSpecies> _hackByIndex;
    private readonly Dictionary<CfruSpecies, ushort> _indexByNational;
    private readonly HashSet<(ushort Species, byte Form)> _forms;
    private readonly HashSet<ushort> _species;

    public CfruSpeciesMap(CfruSpecies[] nationalByIndex)
    {
        _hackByIndex = nationalByIndex
            .Select((entry, index) => (entry.Hack, Index: (ushort)index))
            .Where(entry => entry.Hack is not null)
            .ToDictionary(entry => entry.Index, entry => entry.Hack!);
        _nationalByIndex = nationalByIndex.Select(entry => entry with { Hack = null }).ToArray();
        var mapped = _nationalByIndex
            .Select((national, index) => (National: national, Index: (ushort)index))
            .Where(entry => entry.National.Species != 0)
            .ToArray();
        _indexByNational = mapped
            .DistinctBy(entry => entry.National)
            .ToDictionary(entry => entry.National, entry => entry.Index);
        _forms = mapped.Select(entry => (entry.National.Species, entry.National.Form)).ToHashSet();
        _species = mapped.Select(entry => entry.National.Species).ToHashSet();
        Personal = new CfruPersonalTable(this);
    }

    public CfruPersonalTable Personal { get; }

    public IEnumerable<ushort> Species => _species;

    public IEnumerable<(ushort Species, string Name)> HackSpeciesWithData => _hackByIndex
        .Where(entry => entry.Value.Data is not null)
        .Select(entry => ((ushort)(RomHackIds.Base + entry.Key), entry.Value.Name));

    public CfruSpecies ToNational(ushort index) =>
        index < _nationalByIndex.Length ? _nationalByIndex[index] : default;

    public HackSpecies? ToHack(ushort index) => _hackByIndex.GetValueOrDefault(index);

    public ushort ToSpeciesId(ushort index) => IsUnknown(index) ? (ushort)(RomHackIds.Base + index) : ToNational(index).Species;

    public string? NameOf(int species) => UnknownIndex(species) is { } index ? ToHack(index)?.Name ?? $"Unknown (#{index})" : null;

    private bool IsUnknown(int index) => index is > 0 and <= ushort.MaxValue - RomHackIds.Base && ToNational((ushort)index).Species == 0;

    private ushort? UnknownIndex(int species) => IsUnknown(species - RomHackIds.Base) ? (ushort)(species - RomHackIds.Base) : null;

    public ushort? ToIndex(CfruSpecies national) =>
        national.Species == 0 ? (ushort)0 : _indexByNational.TryGetValue(national, out var index) ? index : null;

    public ushort? ToIndex(ushort species) => HackIndex(species) ?? ToIndex(new CfruSpecies(species, 0)) ?? _indexByNational
        .Where(entry => entry.Key.Species == species && entry.Key.IsPlain)
        .OrderBy(entry => entry.Key.Form)
        .Select(entry => (ushort?)entry.Value)
        .FirstOrDefault();

    private ushort? HackIndex(int species) => UnknownIndex(species) is { } index && _hackByIndex.ContainsKey(index) ? index : null;

    public bool Contains(ushort species) => _species.Contains(species);

    public bool Contains(ushort species, byte form) => _forms.Contains((species, form));
}
