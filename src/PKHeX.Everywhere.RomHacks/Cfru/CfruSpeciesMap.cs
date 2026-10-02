namespace PKHeX.Everywhere.RomHacks.Cfru;

public sealed class CfruSpeciesMap
{
    private readonly (ushort Species, byte Form)[] _nationalByIndex;
    private readonly Dictionary<(ushort Species, byte Form), ushort> _indexByNational;
    private readonly HashSet<ushort> _species;

    public CfruSpeciesMap((ushort Species, byte Form)[] nationalByIndex)
    {
        _nationalByIndex = nationalByIndex;
        var mapped = nationalByIndex
            .Select((national, index) => (National: national, Index: (ushort)index))
            .Where(entry => entry.National.Species != 0)
            .ToArray();
        _indexByNational = mapped
            .DistinctBy(entry => entry.National)
            .ToDictionary(entry => entry.National, entry => entry.Index);
        _species = mapped.Select(entry => entry.National.Species).ToHashSet();
        Personal = new CfruPersonalTable(this);
    }

    public CfruPersonalTable Personal { get; }

    public (ushort Species, byte Form) ToNational(ushort index) =>
        index < _nationalByIndex.Length ? _nationalByIndex[index] : default;

    public ushort? ToIndex(ushort species, byte form) =>
        species == 0 ? (ushort)0 : _indexByNational.TryGetValue((species, form), out var index) ? index : null;

    public ushort? ToIndex(ushort species) => ToIndex(species, 0) ?? _indexByNational
        .Where(entry => entry.Key.Species == species)
        .OrderBy(entry => entry.Key.Form)
        .Select(entry => (ushort?)entry.Value)
        .FirstOrDefault();

    public bool Contains(ushort species) => _species.Contains(species);

    public bool Contains(ushort species, byte form) => _indexByNational.ContainsKey((species, form));
}
