using System.Collections.Immutable;

namespace PKHeX.Facade;

public class Inventories
{
    private readonly Game _game;

    public Inventories(Game game)
    {
        _game = game;

        InventoryTypes = GetInventoryTypes();
        InventoryItems = GetInventories();
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

    private ImmutableHashSet<string> GetInventoryTypes()
    {
        try
        {
            return _game.SaveFile.Inventory.Pouches.Select(i => i.Type.ToString()).ToImmutableHashSet();
        }
        catch (ArgumentOutOfRangeException)
        {
            // Blank Legends: Arceus saves have an item block with an unset type code, so PKHeX.Core throws reading it.
            return ImmutableHashSet<string>.Empty;
        }
    }

    private ImmutableDictionary<string, Inventory> GetInventories() => InventoryTypes.ToImmutableDictionary(
        type => type,
        type => new Inventory(type, _game)
    );
}

public static class InventoriesExtensions
{
    public static IEnumerable<Inventory.Item> AllExceptNone(this Inventory inventory)
    {
        return inventory.Where(i => !i.IsNone);
    }
}