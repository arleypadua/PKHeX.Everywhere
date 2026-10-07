namespace PKHeX.Facade.Abstractions;

/// <summary>
/// A Pokémon that stores its held item as an index of its own, read before conversion to PKHeX's item id.
/// </summary>
public interface IHeldItemIndex
{
    ushort HeldItemIndex { get; }
}
