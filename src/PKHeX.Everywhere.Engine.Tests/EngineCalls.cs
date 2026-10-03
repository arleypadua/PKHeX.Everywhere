using System.Text.Json;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Tests.Base;
using Pokemon = PKHeX.Facade.Pokemons.Pokemon;

namespace PKHeX.Everywhere.Engine.Tests;

internal static class EngineCalls
{
    internal static Session Loaded(string saveFile)
    {
        var session = new Session();
        session.Load(SaveFilePath.Load(saveFile), saveFile);
        return session;
    }

    internal static List<PlugIns.PlugInRan> PlugInRuns(Session session)
    {
        var ran = new List<PlugIns.PlugInRan>();
        session.Published += engineEvent =>
        {
            if (engineEvent is PlugIns.PlugInRan run) ran.Add(run);
        };
        return ran;
    }

    internal static string Dispatch(Session session, string call, string args) =>
        Dispatcher.Dispatch(session, call, args).GetAwaiter().GetResult();

    internal static (PokemonHandle At, int Index)? FirstBoxPokemon(Game game)
    {
        var all = game.Trainer.PokemonBox.All;
        var index = Enumerable.Range(0, all.Count).FirstOrDefault(i => all[i].Pkm.Species != 0, -1);
        return index < 0 ? null : (BoxHandle(game, index), index);
    }

    internal static IEnumerable<(PokemonHandle At, Pokemon Pokemon)> BoxedPokemon(Game game) =>
        game.Trainer.PokemonBox.Boxed().Select(p => (BoxHandle(game, p.Index), p.Pokemon));

    internal static (PokemonHandle At, int Index) BoxSlotOfPartyMember(Game game, int partySlot)
    {
        var index = game.Trainer.Party.BoxIndexOf(partySlot)!.Value;
        return (BoxHandle(game, index), index);
    }

    private static PokemonHandle BoxHandle(Game game, int index) =>
        PokemonHandle.InBox(index / game.SaveFile.BoxSlotCount, index % game.SaveFile.BoxSlotCount);

    internal static (ItemHandle At, int MaxCount)? AddableItem(Game game) => game.Trainer.Inventories.InventoryItems.Values
        .OrderBy(inventory => inventory.Type, StringComparer.Ordinal)
        .Where(inventory => inventory.Items.Any(item => item.IsNone))
        .SelectMany(inventory => inventory.CurrentSupportedItems
            .Select(item => (At: new ItemHandle(inventory.Type, item.Id), MaxCount: inventory.MaxCountOf(item.Id))))
        .Where(item => item.MaxCount > 1)
        .Cast<(ItemHandle, int)?>()
        .FirstOrDefault();

    internal static ItemHandle? OwnedItem(Game game) => game.Trainer.Inventories.InventoryItems.Values
        .OrderBy(inventory => inventory.Type, StringComparer.Ordinal)
        .SelectMany(inventory => inventory.AllExceptNone()
            .Where(item => inventory.Supports(item.Definition))
            .Select(item => new ItemHandle(inventory.Type, item.Id)))
        .FirstOrDefault();

    internal static string Args(params object[] args) => JsonSerializer.Serialize(args.Select(Arg));

    private static object Arg(object arg) => arg switch
    {
        PokemonHandle at => new { source = at.Source.ToString().ToLowerInvariant(), slot = at.Slot, box = at.Box },
        ItemHandle at => new { pouch = at.Pouch, itemId = at.ItemId },
        _ => arg,
    };
}
