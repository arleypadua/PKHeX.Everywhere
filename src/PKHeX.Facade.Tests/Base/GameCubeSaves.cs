using System.Text;
using PKHeX.Core;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade.Tests.Base;

// No real card or save can be committed, so these lay them out the way the games and Dolphin do.
public static class GameCubeSaves
{
    public const string ColosseumCode = "GC6E";
    public const string XDCode = "GXXE";
    public const string BoxCode = "GPXE";
    public const string OtherGameCode = "GZLE";

    public const string StoredNickname = "PIKACHU";

    private static readonly ushort[] XDBlockLengths = [0x88, 0x978, 0xBC50, 0x4A, 0xCC, 0x1774, 0x364, 0x2400, 0x6B4, 0xB, 0x400, 0x800, 0x1000, 0x2000, 0x2298, 0xC800];
    private static readonly int[] XDBlockOffsets = [0x0, 0x90, 0xA10, 0xC660, 0xC6B0, 0xC780, 0xDF00, 0xE270, 0x10670, 0x10D30, 0x10D40, 0x11140, 0x11940, 0x12940, 0x14940, 0x16BE0];

    public static byte[] File(string gameCode) => gameCode switch
    {
        ColosseumCode => ColosseumFile(1, 2, 3),
        XDCode => XDFile(1, 2),
        BoxCode => BoxFile(),
        _ => throw new ArgumentOutOfRangeException(nameof(gameCode)),
    };

    public static byte[] ColosseumFile(params int[] counters)
    {
        var file = UnsavedColosseumFile();
        for (var i = 0; i < ColoCrypto.SLOT_COUNT; i++)
        {
            var slot = ColoCrypto.GetSlot(file, i).Span;
            WriteInt32BigEndian(slot[4..], counters[i]);
            ColoCrypto.SetChecksums(slot);
            ColoCrypto.Encrypt(slot);
        }

        return WithPokemon(new SAV3Colosseum(file), new CK3());
    }

    public static byte[] UnsavedColosseumFile()
    {
        var file = new byte[SaveUtil.SIZE_G3COLO];
        WriteComment(file, "POKéMON COLOSSEUM", "SAVE FILE");
        for (var i = 0; i < ColoCrypto.SLOT_COUNT; i++)
            WriteUInt32LittleEndian(ColoCrypto.GetSlot(file, i).Span, 0x101);
        return file;
    }

    public static byte[] XDFile(params int[] counters)
    {
        var file = UnsavedXDFile();
        for (var i = 0; i < counters.Length; i++)
        {
            var slot = XDCrypto.GetSlot(file, i).Span;
            WriteInt32BigEndian(slot[4..], counters[i]);
            for (var block = 0; block < XDBlockLengths.Length; block++)
            {
                WriteUInt16BigEndian(slot[(0x20 + (2 * block))..], XDBlockLengths[block]);
                WriteUInt16BigEndian(slot[(0x40 + (4 * block))..], (ushort)XDBlockOffsets[block]);
                WriteUInt16BigEndian(slot[(0x42 + (4 * block))..], (ushort)(XDBlockOffsets[block] >> 16));
            }

            XDCrypto.SetChecksums(slot, XDBlockOffsets[0]);
            XDCrypto.EncryptSlot(slot);
        }

        return WithPokemon(new SAV3XD(file), new XK3());
    }

    public static byte[] UnsavedXDFile()
    {
        var file = new byte[SaveUtil.SIZE_G3XD];
        WriteComment(file, "Pokémon XD", "GALE OF DARKNESS");
        for (var i = 0; i < 2; i++)
            WriteUInt32LittleEndian(XDCrypto.GetSlot(file, i).Span, 0x101);
        return file;
    }

    public static byte[] BoxFile()
    {
        var file = new byte[SaveUtil.SIZE_G3BOX];
        WriteComment(file, "Pokémon Box", "Ruby & Sapphire");
        return WithPokemon(new SAV3RSBox(file), new PK3());
    }

    public static byte[] Gci(string gameCode, byte[] file)
    {
        var entry = new byte[0x40];
        Encoding.ASCII.GetBytes(gameCode).CopyTo(entry, 0);
        WriteUInt16BigEndian(entry.AsSpan(0x38), (ushort)(file.Length / MemoryCardBuilder.BlockSize));
        return [.. entry, .. file];
    }

    private static byte[] WithPokemon(SaveFile save, PKM pokemon)
    {
        pokemon.Species = (ushort)Species.Pikachu;
        pokemon.Language = (int)LanguageID.English;
        pokemon.CurrentLevel = 5;
        pokemon.OriginalTrainerName = "TESTER";
        pokemon.Nickname = StoredNickname;
        save.SetBoxSlotAtIndex(pokemon, 0);
        return save.Write().ToArray();
    }

    private static void WriteComment(byte[] file, string game, string description)
    {
        Encoding.Latin1.GetBytes(game).CopyTo(file, 0);
        Encoding.Latin1.GetBytes(description).CopyTo(file, 0x20);
    }
}

public sealed record BuiltCard(byte[] Bytes, IReadOnlyDictionary<string, ushort[]> Blocks, IReadOnlyDictionary<string, int> Entries)
{
    public const int ActiveDirectoryBlock = 2;
}

public sealed class MemoryCardBuilder
{
    public const int BlockSize = 0x2000;
    private const int CardSize = 0x200000;
    private const int EntrySize = 0x40;

    private readonly List<(string GameCode, byte[] Data, ushort[] Blocks)> _files = [];
    private ushort _next = 5;

    public MemoryCardBuilder Add(string gameCode, byte[] data)
    {
        _files.Add((gameCode, data, Allocate(data.Length / BlockSize)));
        return this;
    }

    // A file the game saved after another one took the blocks in its way, as a card that's been used for a while has.
    public MemoryCardBuilder AddAroundAnotherFile(string gameCode, byte[] data)
    {
        var count = data.Length / BlockSize;
        var head = Allocate(count / 2);
        Add(GameCubeSaves.OtherGameCode, Enumerable.Repeat((byte)0x5A, BlockSize).ToArray());
        _files.Add((gameCode, data, [.. head, .. Allocate(count - head.Length)]));
        return this;
    }

    private ushort[] Allocate(int count)
    {
        var blocks = Enumerable.Range(_next, count).Select(block => (ushort)block).ToArray();
        _next += (ushort)count;
        return blocks;
    }

    public BuiltCard Build()
    {
        var card = new byte[CardSize];
        card.AsSpan().Fill(0xFF);

        var header = Block(card, 0);
        for (var i = 0; i < 12; i++) header[i] = (byte)(0xA0 + i);
        WriteUInt64BigEndian(header[0x0C..], 0x0123_4567_89AB_CDEF);
        WriteUInt32BigEndian(header[0x14..], 0);
        WriteUInt32BigEndian(header[0x18..], 0);
        WriteUInt32BigEndian(header[0x1C..], 0);
        WriteUInt16BigEndian(header[0x20..], 0);
        WriteUInt16BigEndian(header[0x22..], CardSize / BlockSize / 0x10);
        WriteUInt16BigEndian(header[0x24..], 0);
        Seal(header, 0x1FC, header[..0x1FC]);

        // The directory and block map the card last wrote are the backups, so the active copy isn't the first one.
        WriteDirectory(Block(card, 1), 6, withFiles: false);
        WriteDirectory(Block(card, BuiltCard.ActiveDirectoryBlock), 7, withFiles: true);
        WriteBlockMap(Block(card, 3), 2, withFiles: false);
        WriteBlockMap(Block(card, 4), 3, withFiles: true);

        foreach (var (_, data, blocks) in _files)
            for (var i = 0; i < blocks.Length; i++)
                data.AsSpan(i * BlockSize, BlockSize).CopyTo(Block(card, blocks[i]));

        return new BuiltCard(
            card,
            _files.ToDictionary(file => file.GameCode, file => file.Blocks),
            _files.Select((file, index) => (file.GameCode, index)).ToDictionary(file => file.GameCode, file => file.index));
    }

    private void WriteDirectory(Span<byte> directory, ushort counter, bool withFiles)
    {
        if (withFiles)
        {
            for (var i = 0; i < _files.Count; i++)
            {
                var (gameCode, _, blocks) = _files[i];
                var entry = directory.Slice(i * EntrySize, EntrySize);
                Encoding.ASCII.GetBytes(gameCode + "01").CopyTo(entry);
                entry[7] = 0x02;
                entry[8..0x28].Clear();
                Encoding.ASCII.GetBytes($"{gameCode.ToLowerInvariant()}_save").CopyTo(entry[8..]);
                WriteUInt32BigEndian(entry[0x28..], 0x2000_0000);
                WriteUInt32BigEndian(entry[0x2C..], 0x40);
                WriteUInt16BigEndian(entry[0x30..], 0x0002);
                WriteUInt16BigEndian(entry[0x32..], 0x0003);
                entry[0x34] = 0x04;
                entry[0x35] = 0x00;
                WriteUInt16BigEndian(entry[0x36..], blocks[0]);
                WriteUInt16BigEndian(entry[0x38..], (ushort)blocks.Length);
                WriteUInt32BigEndian(entry[0x3C..], 0);
            }
        }

        WriteUInt16BigEndian(directory[0x1FFA..], counter);
        Seal(directory, 0x1FFC, directory[..0x1FFC]);
    }

    private void WriteBlockMap(Span<byte> map, ushort counter, bool withFiles)
    {
        map[4..].Clear();
        WriteUInt16BigEndian(map[4..], counter);
        var used = withFiles ? _files.Sum(file => file.Blocks.Length) : 0;
        WriteUInt16BigEndian(map[6..], (ushort)(CardSize / BlockSize - 5 - used));
        WriteUInt16BigEndian(map[8..], (ushort)(withFiles ? _next - 1 : 4));
        if (withFiles)
        {
            foreach (var (_, _, blocks) in _files)
                for (var i = 0; i < blocks.Length; i++)
                    WriteUInt16BigEndian(map[(0x0A + ((blocks[i] - 5) * 2))..], i == blocks.Length - 1 ? (ushort)0xFFFF : blocks[i + 1]);
        }

        Seal(map, 0, map[4..]);
    }

    private static Span<byte> Block(byte[] card, int block) => card.AsSpan(block * BlockSize, BlockSize);

    private static void Seal(Span<byte> block, int at, ReadOnlySpan<byte> covered)
    {
        var (sum, inverse) = Checksum(covered);
        WriteUInt16BigEndian(block[at..], sum);
        WriteUInt16BigEndian(block[(at + 2)..], inverse);
    }

    public static (ushort Sum, ushort Inverse) Checksum(ReadOnlySpan<byte> covered)
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

    public static bool ChecksumsPass(byte[] card)
    {
        return Passes(0, 0x1FC, 0, 0x1FC) && Passes(1, 0x1FFC, 0, 0x1FFC) && Passes(2, 0x1FFC, 0, 0x1FFC) && Passes(3, 0, 4, 0x1FFC) && Passes(4, 0, 4, 0x1FFC);

        bool Passes(int block, int at, int start, int length)
        {
            var span = Block(card, block);
            return Checksum(span.Slice(start, length)) == (ReadUInt16BigEndian(span[at..]), ReadUInt16BigEndian(span[(at + 2)..]));
        }
    }
}
