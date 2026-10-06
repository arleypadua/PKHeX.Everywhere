using PKHeX.Core;

namespace PKHeX.Facade;

// Writing back an untouched Pokémon can still change its bytes, e.g. Let's Go recomputes the stats a box slot stores.
internal sealed class LoadedSlots(IEnumerable<PKM> slots)
{
    private readonly byte[][] _data = slots.Select(Bytes).ToArray();

    public bool HasChanged(int index, PKM pkm) => index >= _data.Length || !Bytes(pkm).SequenceEqual(_data[index]);

    // Gen 1 and 2 keep the names outside Data.
    private static byte[] Bytes(PKM pkm) =>
        [.. pkm.Data, .. pkm.NicknameTrash, .. pkm.OriginalTrainerTrash, .. pkm.HandlingTrainerTrash];
}
