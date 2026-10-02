using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade;

public class Inventories
{
    private readonly Game _game;
    // A pouch commits the whole bag, so every pouch shares one bag to keep edits to the others.
    private readonly Lazy<PlayerBag> _bag;

    public Inventories(Game game)
    {
        _game = game;
        _bag = new Lazy<PlayerBag>(() => game.SaveFile.Inventory);

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
            return _bag.Value.Pouches.Select(i => i.Type.ToString()).ToImmutableHashSet();
        }
        catch (ArgumentOutOfRangeException) when (_game.SaveFile is SAV8LA)
        {
            // Blank Legends: Arceus saves have an item block with an unset type code, so PKHeX.Core throws reading it.
            return ImmutableHashSet<string>.Empty;
        }
    }

    private ImmutableDictionary<string, Inventory> GetInventories() => InventoryTypes.ToImmutableDictionary(
        type => type,
        type => new Inventory(type, _game, _bag.Value)
    );
}

public static class InventoriesExtensions
{
    public static IEnumerable<Inventory.Item> AllExceptNone(this Inventory inventory)
    {
        return inventory.Where(i => !i.IsNone);
    }
}