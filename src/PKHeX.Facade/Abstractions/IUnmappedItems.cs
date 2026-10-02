namespace PKHeX.Facade.Abstractions;

/// <summary>
/// A pouch that stores items as its save's own ids, some of which have no PKHeX id.
/// Those stay out of the pouch's items, and writing the pouch back leaves them in their slots.
/// </summary>
public interface IUnmappedItems
{
    IReadOnlyList<(ushort Id, int Count)> UnmappedItems { get; }
}
