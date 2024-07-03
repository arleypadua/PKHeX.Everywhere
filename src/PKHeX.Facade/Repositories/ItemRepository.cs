using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public class ItemRepository
{
    private readonly Dictionary<ushort, ItemDefinition> _items;
    private readonly ISet<ItemDefinition> _allBalls;

    public ItemRepository(SaveFile saveFile)
    {
        var saveSpecificDataSource = new FilteredGameDataSource(saveFile, GameInfo.Sources);

        _items = GameInfo.Strings.GetItemStrings(saveFile.Context, saveFile.Version)
            .Select((itemName, id) => (id: Convert.ToUInt16(id), itemName))
            .ToDictionary(x => Convert.ToUInt16(x.id), x => new ItemDefinition(Convert.ToUInt16(x.id), x.itemName));

        _allBalls = saveSpecificDataSource.Balls
            .Select(d => GetItem(Convert.ToUInt16(d.Value)))
            // for whatever reason some random items are also returned, like potions and heals
            .Where(b => b.Name.EndsWith("ball", StringComparison.InvariantCultureIgnoreCase))
            .ToImmutableSortedSet(ItemDefinitionNameComparer.Instance);
    }

    public ISet<ItemDefinition> All => _items.Values.ToHashSet();

    public ItemDefinition GetItem(ushort id) => _items[id];

    public ISet<ItemDefinition> AllBalls() => _allBalls;

    private class ItemDefinitionNameComparer : IComparer<ItemDefinition>
    {
        public static ItemDefinitionNameComparer Instance { get; } = new();
        private ItemDefinitionNameComparer() { }
        
        public int Compare(ItemDefinition? x, ItemDefinition? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (y is null) return 1;
            if (x is null) return -1;
            return string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

public record ItemDefinition(ushort Id, string Name)
{
    public static int None = 0;

    public bool IsNone => Id == None;
}

public static class ItemRepositoryExtensions
{
    public static ItemDefinition GetItem(this ItemRepository repository, int id) =>
        repository.GetItem(Convert.ToUInt16(id));
}