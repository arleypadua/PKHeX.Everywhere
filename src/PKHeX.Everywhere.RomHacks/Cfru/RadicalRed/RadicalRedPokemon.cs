namespace PKHeX.Everywhere.RomHacks.Cfru.RadicalRed;

public sealed class RadicalRedPokemon : CfruPokemon
{
    internal static readonly CfruSpeciesMap Map = new(RadicalRedSpeciesTable.NationalByIndex);
    // The TM Case, Exp. Share, Silph Scope and Lift Key are key items in Radical Red, though modern games put them elsewhere or lack them.
    internal static readonly CfruItemMap Items = new(RadicalRedItemTable.ModernByIndex, 123, 216, 874, 878);

    public RadicalRedPokemon() { }
    public RadicalRedPokemon(Memory<byte> data) : base(data) { }

    protected override CfruSpeciesMap SpeciesMap => Map;
    protected override CfruItemMap ItemMap => Items;

    public override uint PSV => ((PID >> 16) ^ (PID & 0xFFFF)) >> 3;
    public override uint TSV => (uint)(TID16 ^ SID16) >> 3;

    public override RadicalRedPokemon Clone() => new(Data.ToArray());
}
