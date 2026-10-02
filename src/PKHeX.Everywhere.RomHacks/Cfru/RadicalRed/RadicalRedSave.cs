using PKHeX.Core;

namespace PKHeX.Everywhere.RomHacks.Cfru.RadicalRed;

public sealed class RadicalRedSave(byte[] data) : CfruSave(data, Signatures)
{
    public static readonly uint[] Signatures = [0x08012025];

    protected override CfruSpeciesMap SpeciesMap => RadicalRedPokemon.Map;
    protected override CfruItemMap ItemMap => RadicalRedPokemon.Items;

    public override Type PKMType => typeof(RadicalRedPokemon);
    public override RadicalRedPokemon BlankPKM => new();
    protected override RadicalRedPokemon GetPKM(Memory<byte> data) => new(data);
    protected override RadicalRedSave CloneInternal() => new(GetFinalData().ToArray());
}
