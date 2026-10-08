using System.Collections.Frozen;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Abstractions;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Everywhere.RomHacks.Expansion.Imperium;

// Imperium keeps one slot of 28 sectors and no version field, so a save is told apart by how many bytes each sector's checksum covers.
public sealed class ImperiumFormat : ISaveFormat
{
    private const int SectorSize = 0x1000;
    private const int SectorCount = 28;
    private const uint Signature = 0x08012025;

    private static readonly int[] Version2ChecksumLengths =
        [2940, 4084, 4084, 4084, 772, .. new int[9], .. Enumerable.Repeat(4084, 12), 1512, 0];

    public string Id => "emerald-imperium";
    public string Name => "Emerald Imperium";
    public GameVersion BaseGame => GameVersion.E;
    public IReadOnlySet<Capability> Capabilities => FrozenSet<Capability>.Empty;

    public IGameDataSource GameData(SaveFile save) => new ImperiumGameData((ImperiumSave)save);

    public SaveFormatMatch Detect(ReadOnlySpan<byte> data) =>
        Matches(data, ImperiumSave.ChunkLengths) || Matches(data, Version2ChecksumLengths) ? SaveFormatMatch.Certain : SaveFormatMatch.No;

    // 2.0 changes the layout and isn't released, so it's refused.
    public SaveFile Load(byte[] data) => Matches(data, ImperiumSave.ChunkLengths) ? new ImperiumSave(data) : throw new GameNotLoadedException();

    public SaveFile Blank() => new ImperiumSave(ImperiumSave.Blank());

    private static bool Matches(ReadOnlySpan<byte> data, int[] checksumLengths)
    {
        if (!Gen3SaveSize.IsSupported(data.Length)) return false;

        var seen = 0;
        for (var index = 0; index < SectorCount; index++)
        {
            var sector = data.Slice(index * SectorSize, SectorSize);
            int id = ReadUInt16LittleEndian(sector[0xFF4..]);
            if (id >= SectorCount || (seen & (1 << id)) != 0) return false;
            if (ReadUInt32LittleEndian(sector[0xFF8..]) != Signature) return false;
            if (ReadUInt16LittleEndian(sector[0xFF6..]) != Checksums.CheckSum32(sector[..checksumLengths[id]])) return false;
            seen |= 1 << id;
        }

        return true;
    }
}
