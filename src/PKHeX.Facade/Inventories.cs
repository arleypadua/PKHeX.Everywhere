using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade;

public class Inventories
{
    private readonly Game _game;

    public Inventories(Game game)
    {
        _game = game;

        var sharedBag = game.SaveFile.Inventory;
        InventoryTypes = GetInventoryTypes(sharedBag);
        InventoryItems = GetInventories(sharedBag);
    }

    public Inventory this[string key]
    {
        get
        {
            if (InventoryItems.ContainsKey(key))
            {
                return InventoryItems[key];
            }
            else
            {
                throw new KeyNotFoundException($"The inventory '{key}' does not exist.");
            }
        }
    }


    public ImmutableHashSet<string> InventoryTypes { get; init; }
    public ImmutableDictionary<string, Inventory> InventoryItems { get; init; }

    private static ImmutableHashSet<string> GetInventoryTypes(PlayerBag bag)
        => bag.Pouches.Select(i => i.Type.ToString()).ToImmutableHashSet();

    private ImmutableDictionary<string, Inventory> GetInventories(PlayerBag bag) => InventoryTypes.ToImmutableDictionary(
        type => type,
        type => new Inventory(type, _game, bag)
    );
}

public static class InventoriesExtensions
{
    public static IEnumerable<Inventory.Item> AllExceptNone(this Inventory inventory)
    {
        return inventory.Where(i => !i.IsNone);
    }
}