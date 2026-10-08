namespace PKHeX.Everywhere.RomHacks.Cfru.Unbound;

public sealed class UnboundPokemon : CfruPokemon
{
    internal static readonly CfruSpeciesMap Map = new(UnboundSpeciesTable.NationalByIndex);
    // No modern game has the Mega Cuff, so it isn't in a modern key items list.
    internal static readonly CfruItemMap Items = new(UnboundItemTable.ModernByIndex, 766);

    public UnboundPokemon() { }
    public UnboundPokemon(Memory<byte> data) : base(data) { }

    protected override CfruSpeciesMap SpeciesMap => Map;
    protected override CfruItemMap ItemMap => Items;

    public override uint PSV => ((PID >> 16) ^ (PID & 0xFFFF)) >> 4;
    public override uint TSV => (uint)(TID16 ^ SID16) >> 4;

    public override UnboundPokemon Clone() => new(Data.ToArray());
}
