using PKHeX.Core;
using PKHeX.Facade.Abstractions;

namespace PKHeX.Everywhere.RomHacks.Cfru.Unbound;

public sealed class UnboundFormat : ISaveFormat
{
    public string Id => "unbound";
    public string Name => "Pokémon Unbound";
    public GameVersion BaseGame => GameVersion.FR;

    public SaveFormatMatch Detect(ReadOnlySpan<byte> data) =>
        CfruSave.FindActiveSlot(data, UnboundSave.Signatures) is null ? SaveFormatMatch.No : SaveFormatMatch.Certain;

    public SaveFile Load(byte[] data) => new UnboundSave(data);
}
