using System.Text.Json;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Tests;

internal static class EngineCalls
{
    internal static Session Loaded(string saveFile)
    {
        var session = new Session();
        session.Load(Game.LoadFrom(saveFile), saveFile);
        return session;
    }

    // Let's Go keeps party members in box storage, so their box slots alias party slots. The Eevee save has no other box Pokémon.
    internal static (PokemonHandle At, int Index)? FirstBoxPokemon(Game game)
    {
        var all = game.Trainer.PokemonBox.All;
        var index = Enumerable.Range(0, all.Count).FirstOrDefault(i => all[i].Pkm.Species != 0 && !InParty(game, i), -1);
        var slots = game.SaveFile.BoxSlotCount;
        return index < 0 ? null : (PokemonHandle.InBox(index / slots, index % slots), index);
    }

    private static bool InParty(Game game, int boxIndex) =>
        game.SaveFile is SAV7b { Blocks.Storage: var storage } && storage.IsParty(boxIndex);

    internal static (ItemHandle At, int MaxCount)? AddableItem(Game game) => game.Trainer.Inventories.InventoryItems.Values
        .OrderBy(inventory => inventory.Type, StringComparer.Ordinal)
        .Where(inventory => inventory.Items.Any(item => item.IsNone))
        .SelectMany(inventory => inventory.CurrentSupportedItems
            .Select(item => (At: new ItemHandle(inventory.Type, item.Id), MaxCount: inventory.MaxCountOf(item.Id))))
        .Where(item => item.MaxCount > 1)
        .Cast<(ItemHandle, int)?>()
        .FirstOrDefault();

    internal static ItemHandle? OwnedItem(Game game) => game.Trainer.Inventories.InventoryItems.Values
        .SelectMany(inventory => inventory.AllExceptNone().Select(item => new ItemHandle(inventory.Type, item.Id)))
        .FirstOrDefault();

    internal static string Args(params object[] args) => JsonSerializer.Serialize(args.Select(Arg));

    private static object Arg(object arg) => arg switch
    {
        PokemonHandle at => new { source = at.Source == SlotSource.Party ? "party" : "box", slot = at.Slot, box = at.Box },
        ItemHandle at => new { pouch = at.Pouch, itemId = at.ItemId },
        _ => arg,
    };
}
