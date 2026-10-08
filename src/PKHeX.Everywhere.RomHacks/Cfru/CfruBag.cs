using PKHeX.Core;

namespace PKHeX.Everywhere.RomHacks.Cfru;

/// <summary>
/// The main, key item, Poké Ball, TM and berry pockets of a CFRU save.
/// </summary>
public sealed class CfruBag : PlayerBag
{
    private const int MainPocket = 0xAD8;
    private const int MainSlots = 450;
    private const int MainSlotsInBlock = (0xFF0 - MainPocket) / CfruPouch.SlotSize;

    // The main pocket runs past block 13 into the start of sector 30. TMs are reusable, so the game holds one of each.
    public CfruBag(ReadOnlySpan<byte> data, int block13, int sector30, CfruItemMap map)
    {
        Info = map.Pockets;
        Pouches =
        [
            new CfruPouch(InventoryType.Items, Info, 999, map, countKey: 0,
                (block13 + MainPocket, MainSlotsInBlock), (sector30, MainSlots - MainSlotsInBlock)),
            new CfruPouch(InventoryType.KeyItems, Info, 999, map, countKey: 0, (sector30 + 0x1F0, 75)),
            new CfruPouch(InventoryType.Balls, Info, 999, map, countKey: 0, (sector30 + 0x31C, 50)),
            new CfruPouch(InventoryType.TMHMs, Info, 1, map, countKey: 0, (sector30 + 0x3E4, 128)),
            new CfruPouch(InventoryType.Berries, Info, 999, map, countKey: 0, (sector30 + 0x5E4, 75)),
        ];
        Pouches.LoadAll(data);
    }

    public override IItemStorage Info { get; }
    public override IReadOnlyList<CfruPouch> Pouches { get; }

    public override void CopyTo(SaveFile sav) => Pouches.SaveAll(sav.Data);
}
