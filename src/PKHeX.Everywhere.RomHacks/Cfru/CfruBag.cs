using PKHeX.Core;

namespace PKHeX.Everywhere.RomHacks.Cfru;

/// <summary>
/// The main, Poké Ball, TM and berry pockets of a CFRU save.
/// </summary>
public sealed class CfruBag : PlayerBag
{
    private const int SlotSize = 4;
    private const int MainPocket = 0xAD8;
    private const int MainSlots = 450;
    private const int MainSlotsInBlock = (0xFF0 - MainPocket) / SlotSize;

    // CFRU keeps the pockets in one run of memory that is saved to block 13 from the main pocket on, then to sector 30.
    // Main items overflow into the start of sector 30, followed by 75 key items at 0x1F0.
    // TMs are reusable, so the game holds one of each.
    public CfruBag(ReadOnlySpan<byte> data, int block13, int sector30, CfruItemMap map)
    {
        Info = map.Pockets;
        Pouches =
        [
            new CfruPouch(InventoryType.Items, Info, 999, map,
                (block13 + MainPocket, MainSlotsInBlock), (sector30, MainSlots - MainSlotsInBlock)),
            new CfruPouch(InventoryType.Balls, Info, 999, map, (sector30 + 0x31C, 50)),
            new CfruPouch(InventoryType.TMHMs, Info, 1, map, (sector30 + 0x3E4, 128)),
            new CfruPouch(InventoryType.Berries, Info, 999, map, (sector30 + 0x5E4, 75)),
        ];
        Pouches.LoadAll(data);
    }

    public override IItemStorage Info { get; }
    public override IReadOnlyList<CfruPouch> Pouches { get; }

    public override void CopyTo(SaveFile sav) => Pouches.SaveAll(sav.Data);
}
