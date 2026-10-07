using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Everywhere.RomHacks.Cfru.RadicalRed;

// Radical Red keeps FireRed's signature, so a save is only a possible match. Vanilla FireRed writes 1 at 0xAC
// and keeps a copy of its non-zero security key, which Radical Red zeroes or leaves out of sync.
public sealed class RadicalRedFormat : CfruFormat
{
    public override string Id => "radicalred";
    public override string Name => "Pokémon Radical Red";

    // Radical Red keeps FireRed's map. RomHackGameDataTests confirms the fixture's Pokémon were met where they live in FireRed.
    protected override bool FireRedMetLocations => true;

    protected override uint Signature => RadicalRedSave.Signatures[0];

    public override SaveFormatMatch Detect(ReadOnlySpan<byte> data)
    {
        if (CfruSave.FindActiveSlot(data, RadicalRedSave.Signatures) is not { } slot) return SaveFormatMatch.No;

        var block = data[CfruSave.FindBlock(data, slot, 0)..];
        if (ReadUInt32LittleEndian(block[0xAC..]) != 1) return SaveFormatMatch.No;

        var key = ReadUInt32LittleEndian(block[0xF20..]);
        return key == 0 || key != ReadUInt32LittleEndian(block[0xAF8..]) ? SaveFormatMatch.Possible : SaveFormatMatch.No;
    }

    public override SaveFile Load(byte[] data) => new RadicalRedSave(data);
}
