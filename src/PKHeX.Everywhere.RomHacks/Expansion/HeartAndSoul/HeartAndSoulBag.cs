using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Cfru;

namespace PKHeX.Everywhere.RomHacks.Expansion.HeartAndSoul;

/// <summary>
/// The six pockets of a Heart &amp; Soul bag, each slot an item index and a quantity XORed with the save's encryption key.
/// </summary>
public sealed class HeartAndSoulBag : PlayerBag
{
    private const int MaxCount = 999;

    // The first index of each run of items sharing a pocket, from the pockets in Heart & Soul's src/data/items.h at Release-v2.0.6.
    // Heart & Soul combines the bag's pockets, so treasures sit in Items and battle items in Medicine.
    // Its config keeps the Escape Rope in Items and the Gen 3 fossils in Key Items.
    private static readonly (int First, InventoryType Pocket)[] PocketRuns =
    [
        (1, InventoryType.Balls), (28, InventoryType.Medicine), (73, InventoryType.Items), (102, InventoryType.Medicine),
        (112, InventoryType.Items), (121, InventoryType.Medicine), (133, InventoryType.Items), (165, InventoryType.KeyItems),
        (170, InventoryType.Items), (461, InventoryType.KeyItems), (462, InventoryType.Items), (514, InventoryType.Berries),
        (582, InventoryType.TMHMs), (690, InventoryType.KeyItems), (758, InventoryType.Items), (770, InventoryType.KeyItems),
        (773, InventoryType.Items), (813, InventoryType.KeyItems), (814, InventoryType.Items), (816, InventoryType.Medicine),
        (820, InventoryType.Items), (828, InventoryType.Balls), (829, InventoryType.Items), (874, InventoryType.KeyItems),
        (875, InventoryType.Balls), (876, InventoryType.Items), (878, InventoryType.KeyItems), (897, InventoryType.Berries),
        (898, InventoryType.Items), (900, InventoryType.KeyItems),
    ];

    private static readonly CfruItemStorage Storage = new(
        HeartAndSoulPokemon.Items.Items,
        HeartAndSoulPokemon.Items.Items.ToDictionary(item => item, item => PocketOf(HeartAndSoulPokemon.Items.ToIndex(item)!.Value)));

    private static InventoryType PocketOf(int index) => PocketRuns.Last(run => run.First <= index).Pocket;

    internal HeartAndSoulBag(HeartAndSoulSave save)
    {
        CfruPouch Pouch(InventoryType type, int offset, int slots) =>
            new(type, Info, MaxCount, save.ItemMap, (ushort)save.EncryptionKey, save.SaveBlock1Slots(offset, slots, CfruPouch.SlotSize));

        Pouches =
        [
            Pouch(InventoryType.Items, 0x564, 236),
            Pouch(InventoryType.Medicine, 0xE28, 92),
            Pouch(InventoryType.Balls, 0xA04, 39),
            Pouch(InventoryType.TMHMs, 0xAA0, 160),
            Pouch(InventoryType.Berries, 0xD20, 66),
            Pouch(InventoryType.KeyItems, 0x914, 60),
        ];
        Pouches.LoadAll(save.Data);
    }

    public override IItemStorage Info => Storage;
    public override IReadOnlyList<CfruPouch> Pouches { get; }

    public override void CopyTo(SaveFile sav) => Pouches.SaveAll(sav.Data);
}
