namespace PKHeX.Facade.Abstractions;

/// <summary>
/// A Pokémon that stores its species and held item as its save's own ids, some of which have no PKHeX id.
/// Its getters report those as 0, and writing back what a getter returned keeps the stored id.
/// </summary>
public interface IUnmappedValues
{
    ushort? UnmappedSpecies { get; }
    ushort? UnmappedHeldItem { get; }
}
