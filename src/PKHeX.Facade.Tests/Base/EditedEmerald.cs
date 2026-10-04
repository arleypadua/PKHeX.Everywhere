using PKHeX.Core;

namespace PKHeX.Facade.Tests.Base;

public static class EditedEmerald
{
    public static byte[] WithBoxSpecies(ushort speciesInternal) =>
        WithFirstBoxPokemon(pk => pk.SpeciesInternal = speciesInternal);

    public static byte[] WithBoxMove(ushort move) =>
        WithFirstBoxPokemon(pk => pk.Move1 = move);

    public static byte[] WithFirstBoxPokemon(Action<PK3> change) =>
        Emerald(save => ChangeBoxSlot(save, 0, change));

    public static byte[] WithEmptyBoxSlot(Action<PK3> change) =>
        Emerald(save => ChangeBoxSlot(save, Enumerable.Range(0, save.SlotCount).First(i => save.GetBoxSlotAtIndex(i).Species == 0), change));

    public static byte[] WithFirstPartyPokemon(Action<PK3> change) =>
        Emerald(save =>
        {
            var count = save.PartyCount;
            var pokemon = (PK3)save.GetPartySlotAtIndex(0);
            change(pokemon);
            save.SetPartySlotAtIndex(pokemon, 0, EntityImportSettings.None);
            save.LargeBlock.PartyCount = (byte)count;
        });

    private static void ChangeBoxSlot(SAV3E save, int index, Action<PK3> change)
    {
        var pokemon = (PK3)save.GetBoxSlotAtIndex(index);
        change(pokemon);
        save.SetBoxSlotAtIndex(pokemon, index, EntityImportSettings.None);
    }

    private static byte[] Emerald(Action<SAV3E> change)
    {
        var save = (SAV3E)SaveUtil.GetSaveFile(File.ReadAllBytes(SaveFilePath.Emerald))!;
        change(save);
        return save.Write().ToArray();
    }
}
