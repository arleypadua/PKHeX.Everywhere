using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Cfru;

namespace PKHeX.Everywhere.RomHacks.Expansion.Imperium;

/// <summary>
/// The six pockets of an Emerald Imperium bag, each slot an item index and a quantity XORed with the save's encryption key.
/// </summary>
public sealed class ImperiumBag : PlayerBag
{
    private const int MaxCount = 999;

    // From the pockets in Imperium's src/data/items.h: the Exp. Share, the Escape Rope and some story items are key items there,
    // though a modern game puts them in the Items pocket.
    private static readonly ushort[] KeyItems = [78, 113, 121, 123, 216, 723, 872, 874, 875, 876, 877, 878, 1833, 1857, 1858, 2481, 2554];

    private static readonly CfruItemStorage Storage = new(
        ImperiumPokemon.ItemMap.Items,
        ItemStorage9ZA.MegaStones.ToArray().Select(item => (item, InventoryType.MegaStones))
            .Concat(KeyItems.Select(item => (item, InventoryType.KeyItems)))
            .ToDictionary());

    internal ImperiumBag(ImperiumSave save)
    {
        CfruPouch Pouch(InventoryType type, int offset, int slots) =>
            new(type, Info, MaxCount, save.ItemMap, (ushort)save.EncryptionKey, save.SaveBlock1Slots(offset, slots, CfruPouch.SlotSize));

        Pouches =
        [
            Pouch(InventoryType.Items, 0x560, 180),
            Pouch(InventoryType.MegaStones, 0x830, 76),
            Pouch(InventoryType.KeyItems, 0x960, 60),
            Pouch(InventoryType.Balls, 0xA50, 50),
            Pouch(InventoryType.TMHMs, 0xB18, 252),
            Pouch(InventoryType.Berries, 0xF08, 70),
        ];
        Pouches.LoadAll(save.Data);
    }

    public override IItemStorage Info => Storage;
    public override IReadOnlyList<CfruPouch> Pouches { get; }

    public override void CopyTo(SaveFile sav) => Pouches.SaveAll(sav.Data);
}
