namespace PKHeX.Everywhere.RomHacks.Cfru.Unbound;

public sealed class UnboundPokemon : CfruPokemon
{
    internal static readonly CfruSpeciesMap Map = new(UnboundSpeciesTable.NationalByIndex);
    internal static readonly CfruItemMap Items = new(UnboundItemTable.ModernByIndex);

    public UnboundPokemon() { }
    public UnboundPokemon(Memory<byte> data) : base(data) { }

    protected override CfruSpeciesMap SpeciesMap => Map;
    protected override CfruItemMap ItemMap => Items;

    public override uint PSV => ((PID >> 16) ^ (PID & 0xFFFF)) >> 4;
    public override uint TSV => (uint)(TID16 ^ SID16) >> 4;

    public override UnboundPokemon Clone() => new(Data.ToArray());
}
