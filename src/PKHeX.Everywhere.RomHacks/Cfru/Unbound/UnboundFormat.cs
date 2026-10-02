using PKHeX.Core;
using PKHeX.Facade.Abstractions;

namespace PKHeX.Everywhere.RomHacks.Cfru.Unbound;

public sealed class UnboundFormat : CfruFormat
{
    public override string Id => "unbound";
    public override string Name => "Pokémon Unbound";

    public override SaveFormatMatch Detect(ReadOnlySpan<byte> data) =>
        CfruSave.FindActiveSlot(data, UnboundSave.Signatures) is null ? SaveFormatMatch.No : SaveFormatMatch.Certain;

    public override SaveFile Load(byte[] data) => new UnboundSave(data);
}
