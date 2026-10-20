using System.Collections.Frozen;
using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade;

internal static class MemoryCards
{
    public static readonly IReadOnlyList<MemoryCardFormat> Formats =
    [
        new("memory-card-colosseum", "Pokémon Colosseum", GameVersion.COLO, data => new SAV3Colosseum(data)),
        new("memory-card-xd", "Pokémon XD", GameVersion.XD, data => new SAV3XD(data)),
        new("memory-card-box", "Pokémon Box", GameVersion.RSBOX, data => new SAV3RSBox(data)),
    ];

    public static MemoryCardFormat? Find(string id) => Formats.FirstOrDefault(format => format.Id == id);

    public static Game? Detect(byte[] data)
    {
        if (MemoryCard.Read(data) is not { } card) return null;

        return Formats.Where(format => card.Find(format.BaseGame) is not null).ToArray() switch
        {
            [] => throw new GameNotLoadedException(),
            [var only] => only.LoadGame(card),
            var several => throw new FormatChoiceRequiredException(several),
        };
    }
}

internal sealed class MemoryCardFormat(string id, string name, GameVersion game, Func<byte[], SaveFile> open) : ISaveFormat
{
    public string Id => id;
    public string Name => name;
    public GameVersion BaseGame => game;
    public IReadOnlySet<Capability> Capabilities { get; } = Enum.GetValues<Capability>().ToFrozenSet();
    public SaveFormatMatch Detect(ReadOnlySpan<byte> data) => SaveFormatMatch.No;
    public SaveFile Load(byte[] data) => LoadGame(data).SaveFile;

    public Game LoadGame(byte[] data) => LoadGame(MemoryCard.Read(data) ?? throw new GameNotLoadedException());

    public Game LoadGame(MemoryCard card)
    {
        var file = card.Find(game) ?? throw new GameNotLoadedException();
        return Game.FromPKHeX(open(file.Read()), file) ?? throw new GameNotLoadedException();
    }
}

// PKHeX's SAV3GCMemoryCard reads a file as if its blocks were contiguous, and writes one back by copying the directory entry
// over the file's first 0x40 bytes, the comment the GameCube shows, and by changing the active directory without its checksum.
// The card is read and written here instead, with Dolphin's layout.
internal sealed class MemoryCard
{
    private const int BlockSize = 0x2000;
    private const int EntrySize = 0x40;
    private const int EntryCount = 127;
    private const int FirstFileBlock = 5;
    private const int BlockMapSize = 0xFFB;
    private const int ModifiedAt = 0x28;
    private const int FirstBlockAt = 0x36;
    private const int BlockCountAt = 0x38;
    private const int NextBlocksAt = 0x0A;

    private readonly byte[] _card;
    private readonly int _directory;
    private readonly int _blockMap;

    private MemoryCard(byte[] card, int directory, int blockMap)
    {
        _card = card;
        _directory = directory;
        _blockMap = blockMap;
    }

    public static MemoryCard? Read(byte[] data)
    {
        if (!SAV3GCMemoryCard.IsMemoryCardSize(data) || !Sealed(data, 0, 0x1FC, ..0x1FC)) return null;

        var directory = Latest(data, 1, 2, 0x1FFA, 0x1FFC, ..0x1FFC);
        var blockMap = Latest(data, 3, 4, 0x04, 0x00, 4..);
        return directory is null || blockMap is null ? null : new MemoryCard(data.ToArray(), directory.Value, blockMap.Value);
    }

    public MemoryCardFile? Find(GameVersion game)
    {
        for (var entry = 0; entry < EntryCount; entry++)
        {
            var entryAt = (_directory * BlockSize) + (entry * EntrySize);
            if (SaveHandlerGCI.GetGameCode(_card.AsSpan(entryAt, 4)) == game && Blocks(entryAt) is { } blocks)
                return new MemoryCardFile(this, entryAt, blocks);
        }

        return null;
    }

    public byte[] Read(IReadOnlyList<int> blocks) => blocks.SelectMany(block => _card.AsSpan(block * BlockSize, BlockSize).ToArray()).ToArray();

    public byte[] Write(int entryAt, IReadOnlyList<int> blocks, ReadOnlySpan<byte> file)
    {
        var card = _card.ToArray();
        for (var i = 0; i < blocks.Count; i++)
            file.Slice(i * BlockSize, BlockSize).CopyTo(card.AsSpan(blocks[i] * BlockSize));

        WriteUInt32BigEndian(card.AsSpan(entryAt + ModifiedAt), (uint)(DateTime.Now - new DateTime(2000, 1, 1)).TotalSeconds);
        Seal(card, _directory, 0x1FFC, ..0x1FFC);
        return card;
    }

    private int[]? Blocks(int entryAt)
    {
        var block = (int)ReadUInt16BigEndian(_card.AsSpan(entryAt + FirstBlockAt));
        var count = ReadUInt16BigEndian(_card.AsSpan(entryAt + BlockCountAt));
        var blocks = new int[count];
        for (var i = 0; i < count; i++)
        {
            if (block < FirstFileBlock || block - FirstFileBlock >= BlockMapSize || (block + 1) * BlockSize > _card.Length) return null;
            blocks[i] = block;
            block = ReadUInt16BigEndian(_card.AsSpan((_blockMap * BlockSize) + NextBlocksAt + ((block - FirstFileBlock) * 2)));
        }

        return blocks;
    }

    private static int? Latest(byte[] data, int block, int backup, int counterAt, int checksumAt, Range covered)
    {
        var valid = new[] { block, backup }.Where(candidate => Sealed(data, candidate, checksumAt, covered)).ToArray();
        return valid.Length == 0 ? null : valid.MaxBy(candidate => ReadUInt16BigEndian(data.AsSpan((candidate * BlockSize) + counterAt)));
    }

    private static bool Sealed(byte[] data, int block, int checksumAt, Range covered)
    {
        var span = data.AsSpan(block * BlockSize, BlockSize);
        var (sum, inverse) = Checksum(span[covered]);
        return ReadUInt16BigEndian(span[checksumAt..]) == sum && ReadUInt16BigEndian(span[(checksumAt + 2)..]) == inverse;
    }

    private static void Seal(byte[] data, int block, int checksumAt, Range covered)
    {
        var span = data.AsSpan(block * BlockSize, BlockSize);
        var (sum, inverse) = Checksum(span[covered]);
        WriteUInt16BigEndian(span[checksumAt..], sum);
        WriteUInt16BigEndian(span[(checksumAt + 2)..], inverse);
    }

    private static (ushort Sum, ushort Inverse) Checksum(ReadOnlySpan<byte> covered)
    {
        ushort sum = 0, inverse = 0;
        for (var i = 0; i < covered.Length; i += 2)
        {
            var value = ReadUInt16BigEndian(covered[i..]);
            sum += value;
            inverse += (ushort)~value;
        }

        return (sum == 0xFFFF ? (ushort)0 : sum, inverse == 0xFFFF ? (ushort)0 : inverse);
    }
}

internal sealed class MemoryCardFile(MemoryCard card, int entryAt, int[] blocks)
{
    public byte[] Read() => card.Read(blocks);

    public byte[] Write(ReadOnlySpan<byte> file) => card.Write(entryAt, blocks, file);
}
