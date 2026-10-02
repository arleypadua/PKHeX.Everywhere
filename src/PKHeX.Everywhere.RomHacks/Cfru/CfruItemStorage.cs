using PKHeX.Core;

namespace PKHeX.Everywhere.RomHacks.Cfru;

// A hack's items go in the pocket modern games put them in. Key items have no pocket yet.
// Some key items, like the Escape Rope, became key items in later games, so an item any game lets a Pokémon hold isn't one.
public sealed class CfruItemStorage : IItemStorage
{
    private static readonly HashSet<ushort> Machines =
    [
        ..ItemStorage4.Machine, ..ItemStorage5.Machine, ..ItemStorage6AO.Machine, ..ItemStorage7SM.Machine,
        ..ItemStorage8BDSP.Machine, ..ItemStorage9SV.Machine, ..ItemStorage9ZA.TM,
    ];

    private static readonly HashSet<ushort> Balls =
    [
        ..ItemStorage4HGSS.BallsHGSS, ..ItemStorage8BDSP.Balls, ..ItemStorage8SWSH.Balls, ..ItemStorage9SV.Balls, ..ItemStorage9ZA.Balls,
    ];

    private static readonly HashSet<ushort> Berries =
    [
        ..ItemStorage4.Berry, ..ItemStorage5.Berry, ..ItemStorage6XY.Berry, ..ItemStorage7SM.Berry, ..ItemStorage8SWSH.Berry,
        ..ItemStorage9SV.Berry, ..ItemStorage9ZA.Berry,
    ];

    private static readonly HashSet<ushort> KeyItems =
    [
        ..ItemStorage4.Key, ..ItemStorage4Pt.KeyPt, ..ItemStorage4HGSS.KeyHGSS, ..ItemStorage5BW.Key, ..ItemStorage5B2W2.Key,
        ..ItemStorage6XY.Key, ..ItemStorage6AO.Key, ..ItemStorage7SM.Key, ..ItemStorage7USUM.Key, ..ItemStorage8SWSH.Key,
        ..ItemStorage8BDSP.Key, ..ItemStorage9ZA.Key,
    ];

    private static readonly HashSet<ushort> Holdable =
    [
        ..ItemStorage5.GetAllHeld(), ..ItemStorage6AO.GetAllHeld(), ..ItemStorage7USUM.GetAllHeld(), ..ItemStorage8SWSH.GetAllHeld(),
        ..ItemStorage9SV.GetAllHeld(),
    ];

    private readonly Dictionary<InventoryType, ushort[]> _items;

    public CfruItemStorage(IEnumerable<ushort> items) => _items = items
        .GroupBy(PocketOf)
        .ToDictionary(pocket => pocket.Key, pocket => pocket.Order().ToArray());

    private static InventoryType PocketOf(ushort item) => item switch
    {
        _ when Machines.Contains(item) => InventoryType.TMHMs,
        _ when Balls.Contains(item) => InventoryType.Balls,
        _ when Berries.Contains(item) => InventoryType.Berries,
        _ when KeyItems.Contains(item) && !Holdable.Contains(item) => InventoryType.KeyItems,
        _ => InventoryType.Items,
    };

    public bool IsLegal(InventoryType type, int itemIndex, int itemCount) => GetItems(type).Contains((ushort)itemIndex);

    public ReadOnlySpan<ushort> GetItems(InventoryType type) => _items.GetValueOrDefault(type, []);
}
