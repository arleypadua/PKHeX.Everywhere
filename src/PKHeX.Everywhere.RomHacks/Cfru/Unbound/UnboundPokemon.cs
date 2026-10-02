namespace PKHeX.Everywhere.RomHacks.Cfru.Unbound;

public sealed class UnboundPokemon : CfruPokemon
{
    internal static readonly CfruSpeciesMap Map = new(UnboundSpeciesTable.NationalByIndex);

    public UnboundPokemon() { }
    public UnboundPokemon(Memory<byte> data) : base(data) { }

    protected override CfruSpeciesMap SpeciesMap => Map;

    public override uint PSV => ((PID >> 16) ^ (PID & 0xFFFF)) >> 4;
    public override uint TSV => (uint)(TID16 ^ SID16) >> 4;

    public override UnboundPokemon Clone() => new(Data.ToArray());
}
