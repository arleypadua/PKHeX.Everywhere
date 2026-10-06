using PKHeX.Core;
using static PKHeX.Core.InventoryType;

namespace PKHeX.Everywhere.RomHacks.Legacy;

// Only the Items pocket changes: BAG_ITEMS_COUNT 30 -> 120. Its stack cap stays 99, since the hack
// leaves MAX_BAG_ITEM_CAPACITY alone; the extra 90 slots push every later pocket along by 0x168.
internal sealed class EmeraldLegacyBag : PlayerBag, IPlayerBag3
{
    public override IReadOnlyList<InventoryPouch3> Pouches { get; } = GetPouches(ItemStorage3E.Instance);
    public override ItemStorage3E Info => ItemStorage3E.Instance;

    private static InventoryPouch3[] GetPouches(ItemStorage3E info) =>
    [
        new(0x0C8, 120, 099, info, Items),
        new(0x2A8, 030, 001, info, KeyItems),
        new(0x320, 016, 099, info, Balls),
        new(0x360, 064, 099, info, TMHMs),
        new(0x460, 046, 999, info, Berries),
        new(0x000, 050, 999, info, PCItems),
    ];

    public EmeraldLegacyBag(EmeraldLegacySave sav) : this(sav.LargeBlock.Inventory, sav.SmallBlock.SecurityKey) { }

    public EmeraldLegacyBag(ReadOnlySpan<byte> data, uint security)
    {
        UpdateSecurityKey(security);
        Pouches.LoadAll(data);
    }

    public override void CopyTo(SaveFile sav) => CopyTo((EmeraldLegacySave)sav);
    public void CopyTo(EmeraldLegacySave sav) => CopyTo(sav.LargeBlock.Inventory);
    public void CopyTo(Span<byte> data) => Pouches.SaveAll(data);

    public override int GetMaxCount(InventoryType type, int itemIndex)
    {
        if (type is TMHMs && ItemConverter.IsItemHM3((ushort)itemIndex))
            return 1;
        return GetMaxCount(type);
    }

    public void UpdateSecurityKey(uint securityKey)
    {
        foreach (var pouch in Pouches)
        {
            if (pouch.Type != PCItems)
                pouch.SecurityKey = securityKey;
        }
    }
}
