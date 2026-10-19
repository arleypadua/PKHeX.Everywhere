using System.Collections.Frozen;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Abstractions;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Everywhere.RomHacks.Expansion.HeartAndSoul;

// Heart & Soul keeps Emerald's two slots of 14 sectors, so it's told apart by the magic its SaveBlock1 carries.
public sealed class HeartAndSoulFormat : ISaveFormat
{
    private const int MagicOffset = 0x10E0;
    private const uint Magic = 0xE8F828BC;
    private const ushort SupportedSaveVersion = 5;

    public string Id => "heart-and-soul";
    public string Name => "Pokémon Heart & Soul";
    public GameVersion BaseGame => GameVersion.E;
    public IReadOnlySet<Capability> Capabilities => FrozenSet<Capability>.Empty;

    public IGameDataSource GameData(SaveFile save) =>
        new ExpansionGameData(save, HeartAndSoulPokemon.Map, HeartAndSoulPokemon.Items, HeartAndSoulMoveTable.Map, HeartAndSoulPokemon.Balls);

    public SaveFormatMatch Detect(ReadOnlySpan<byte> data)
    {
        if (!Gen3SaveSize.IsSupported(data.Length)) return SaveFormatMatch.No;

        Span<int> sectors = stackalloc int[HeartAndSoulSave.SectorsPerSlot];
        if (!HeartAndSoulSave.TryReadNewerSlot(data, sectors)) return SaveFormatMatch.No;

        var magic = data[HeartAndSoulSave.SaveBlock1Offset(sectors, MagicOffset)..];
        return ReadUInt32LittleEndian(magic) == Magic ? SaveFormatMatch.Certain : SaveFormatMatch.No;
    }

    // Another release's save may have moved things, so only the version this was written from loads.
    public SaveFile Load(byte[] data) =>
        new HeartAndSoulSave(data) is { SaveVersion: SupportedSaveVersion } save ? save : throw new GameNotLoadedException();
}
