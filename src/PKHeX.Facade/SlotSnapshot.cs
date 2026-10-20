using PKHeX.Core;

namespace PKHeX.Facade;

// Writing back an untouched Pokémon can still change its bytes, e.g. Let's Go recomputes the stats a box slot stores.
internal sealed class SlotSnapshot(IEnumerable<PKM> slots)
{
    private readonly byte[][] _data = slots.Select(Bytes).ToArray();

    public bool HasChanged(int index, PKM pkm) => index >= _data.Length || !Bytes(pkm).SequenceEqual(_data[index]);

    public bool AnyChanged(IList<PKM> slots) => Enumerable.Range(0, slots.Count).Any(i => HasChanged(i, slots[i]));

    // Gen 1 and 2 keep the names, Gen 2 the egg flag, and XD the shadow state outside Data.
    private static byte[] Bytes(PKM pkm) =>
        [.. pkm.Data, .. pkm.NicknameTrash, .. pkm.OriginalTrainerTrash, .. pkm.HandlingTrainerTrash, (byte)(pkm.IsEgg ? 1 : 0), .. ShadowState(pkm)];

    private static byte[] ShadowState(PKM pkm) =>
        pkm is XK3 xk3 ? [.. BitConverter.GetBytes(xk3.Purification), (byte)(xk3.IsShadow ? 1 : 0)] : [];
}
