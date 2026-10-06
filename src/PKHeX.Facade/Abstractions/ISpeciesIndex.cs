namespace PKHeX.Facade.Abstractions;

/// <summary>
/// A Pokémon that stores its species as an index of its own, read before conversion to PKHeX's species id.
/// </summary>
public interface ISpeciesIndex
{
    ushort SpeciesIndex { get; }
}
