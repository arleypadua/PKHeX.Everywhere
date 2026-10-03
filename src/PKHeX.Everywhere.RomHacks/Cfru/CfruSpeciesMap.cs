namespace PKHeX.Everywhere.RomHacks.Cfru;

public sealed class CfruSpeciesMap
{
    // PKHeX's species ids end below 1100, so an index with no national species travels as an id from here up.
    private const ushort UnknownIdBase = 0xF000;

    private readonly CfruSpecies[] _nationalByIndex;
    private readonly Dictionary<CfruSpecies, ushort> _indexByNational;
    private readonly HashSet<(ushort Species, byte Form)> _forms;
    private readonly HashSet<ushort> _species;

    public CfruSpeciesMap(CfruSpecies[] nationalByIndex)
    {
        _nationalByIndex = nationalByIndex;
        var mapped = nationalByIndex
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

    public CfruSpecies ToNational(ushort index) =>
        index < _nationalByIndex.Length ? _nationalByIndex[index] : default;

    public ushort ToSpeciesId(ushort index) => IsUnknown(index) ? (ushort)(UnknownIdBase + index) : ToNational(index).Species;

    public string? NameOf(int species) => UnknownIndex(species) is { } index ? $"Unknown (#{index})" : null;

    private bool IsUnknown(int index) => index is > 0 and <= ushort.MaxValue - UnknownIdBase && ToNational((ushort)index).Species == 0;

    private ushort? UnknownIndex(int species) => IsUnknown(species - UnknownIdBase) ? (ushort)(species - UnknownIdBase) : null;

    public ushort? ToIndex(CfruSpecies national) =>
        national.Species == 0 ? (ushort)0 : _indexByNational.TryGetValue(national, out var index) ? index : null;

    public ushort? ToIndex(ushort species) => ToIndex(new CfruSpecies(species, 0)) ?? _indexByNational
        .Where(entry => entry.Key.Species == species && entry.Key.IsPlain)
        .OrderBy(entry => entry.Key.Form)
        .Select(entry => (ushort?)entry.Value)
        .FirstOrDefault();

    public bool Contains(ushort species) => _species.Contains(species);

    public bool Contains(ushort species, byte form) => _forms.Contains((species, form));
}
