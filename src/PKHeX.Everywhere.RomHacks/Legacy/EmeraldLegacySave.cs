using PKHeX.Core;

namespace PKHeX.Everywhere.RomHacks.Legacy;

internal sealed class EmeraldLegacySave : SAV3, IDaycareRandomState<uint>
{
    private const int LargeBlockSize = 0x3D98; // sizeof(SaveBlock1); Emerald's is 0x3D88

    protected override EmeraldLegacySave CloneInternal() => new(GetFinalData()) { Language = Language };
    public override SaveBlock3SmallE SmallBlock { get; }
    public override EmeraldLegacyBlockLarge LargeBlock { get; }
    public override GameVersion Version { get => GameVersion.E; set { } }
    public override PersonalTable3 Personal => PersonalTable.E;

    public EmeraldLegacySave(Memory<byte> data) : base(data)
    {
        SmallBlock = new SaveBlock3SmallE(SmallBuffer[..0xF2C]);
        LargeBlock = new EmeraldLegacyBlockLarge(LargeBuffer[..LargeBlockSize]);
    }

    public EmeraldLegacySave(bool japanese = false) : base(japanese)
    {
        SmallBlock = new SaveBlock3SmallE(SmallBuffer[..0xF2C]);
        LargeBlock = new EmeraldLegacyBlockLarge(LargeBuffer[..LargeBlockSize]);
    }

    public override EmeraldLegacyBag Inventory => new(this);

    public override int MaxItemID => 376; // Legal.MaxItemID_3_E is internal to PKHeX.Core

    public override bool NationalDex
    {
        get => SmallBlock.PokedexNationalMagicRSE == PokedexNationalUnlockRSE;
        set
        {
            SmallBlock.PokedexMode = value ? (byte)1 : (byte)0;
            SmallBlock.PokedexNationalMagicRSE = value ? PokedexNationalUnlockRSE : (byte)0;
            SetEventFlag(0x8F6, value); // Emerald's 0x896, moved by the larger trainer flag range
            SetWork(0x46, PokedexNationalUnlockWorkRSE);
        }
    }

    public override uint Money
    {
        get => LargeBlock.Money ^ SmallBlock.SecurityKey;
        set => LargeBlock.Money = value ^ SmallBlock.SecurityKey;
    }

    public override uint Coin
    {
        get => (ushort)(LargeBlock.Coin ^ SmallBlock.SecurityKey);
        set => LargeBlock.Coin = (ushort)(value ^ SmallBlock.SecurityKey);
    }

    protected override int GetDaycareEXPOffset(int slot) => GetDaycareSlotOffset(slot + 1) - 4;

    uint IDaycareRandomState<uint>.Seed
    {
        get => LargeBlock.DaycareSeed;
        set => LargeBlock.DaycareSeed = value;
    }
}
