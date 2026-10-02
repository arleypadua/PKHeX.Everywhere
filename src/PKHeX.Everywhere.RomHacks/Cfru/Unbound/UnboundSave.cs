using PKHeX.Core;

namespace PKHeX.Everywhere.RomHacks.Cfru.Unbound;

public sealed class UnboundSave(byte[] data) : CfruSave(data, Signatures)
{
    public static readonly uint[] Signatures = [0x01122000, 0x01121999, 0x01121998];

    protected override CfruSpeciesMap SpeciesMap => UnboundPokemon.Map;

    public override Type PKMType => typeof(UnboundPokemon);
    public override UnboundPokemon BlankPKM => new();
    protected override UnboundPokemon GetPKM(Memory<byte> data) => new(data);
    protected override UnboundSave CloneInternal() => new(GetFinalData().ToArray());
}
