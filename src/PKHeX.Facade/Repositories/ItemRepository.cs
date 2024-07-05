using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public class ItemRepository
{
    private readonly Dictionary<ushort, ItemDefinition> _allItems;
    private readonly Dictionary<ushort, ItemDefinition> _allBalls;
    
    private readonly Dictionary<ushort, ItemDefinition> _gameItems;


    public ItemRepository(SaveFile saveFile)
    {
        _allItems = GameInfo.Strings.Item
            .Select((itemName, id) => (id: Convert.ToUInt16(id), itemName))
            .ToDictionary(x => Convert.ToUInt16(x.id), x => new ItemDefinition(Convert.ToUInt16(x.id), x.itemName));
        
        _allBalls = GameInfo.Strings.balllist
            .Select((itemName, id) => (id: Convert.ToUInt16(id), itemName))
            .ToDictionary(x => Convert.ToUInt16(x.id), x => new ItemDefinition(Convert.ToUInt16(x.id), x.itemName));
            
        _gameItems = GameInfo.Strings.GetItemStrings(saveFile.Context, saveFile.Version)
            .Select((itemName, id) => (id: Convert.ToUInt16(id), itemName))
            .ToDictionary(x => Convert.ToUInt16(x.id), x => new ItemDefinition(Convert.ToUInt16(x.id), x.itemName));
    }

    public ISet<ItemDefinition> GameItems => _gameItems.Values.ToHashSet();

    public ItemDefinition GetItem(ushort id) => _allItems[id];

    public ISet<ItemDefinition> AllBalls() => _allBalls.Values.ToHashSet();

    public ItemDefinition? GetBall(Ball ball) => _allBalls.GetValueOrDefault((ushort)ball);
}

public record ItemDefinition(ushort Id, string Name)
{
    public static readonly int None = 0;

    public bool IsNone => Id == None;
}

public static class ItemRepositoryExtensions
{
    public static ItemDefinition GetItem(this ItemRepository repository, int id) =>
        repository.GetItem(Convert.ToUInt16(id));
}