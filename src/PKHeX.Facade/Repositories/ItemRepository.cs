using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public class ItemRepository
{
    private static readonly Dictionary<ushort, ItemDefinition> AllItemsById = GameInfo.Strings.Item
        .Select((itemName, id) => (id: Convert.ToUInt16(id), itemName))
        .ToDictionary(x => Convert.ToUInt16(x.id), x => new ItemDefinition(Convert.ToUInt16(x.id), x.itemName));
    
    private static readonly Dictionary<ushort, ItemDefinition> AllBallsById = GameInfo.Strings.balllist
        .Select((itemName, id) => (id: Convert.ToUInt16(id), itemName))
        .ToDictionary(x => Convert.ToUInt16(x.id), x => new ItemDefinition(Convert.ToUInt16(x.id), x.itemName));
    
    private readonly Dictionary<ushort, ItemDefinition> _gameItems;


    public ItemRepository(SaveFile saveFile)
    {
        _gameItems = GameInfo.Strings.GetItemStrings(saveFile.Context, saveFile.Version)
            .Select((itemName, id) => (id: Convert.ToUInt16(id), itemName))
            .ToDictionary(x => Convert.ToUInt16(x.id), x => new ItemDefinition(Convert.ToUInt16(x.id), x.itemName));
    }

    public ISet<ItemDefinition> GameItems => _gameItems.Values.ToHashSet();

    public static ItemDefinition GetItem(ushort id) => AllItemsById[id];
    public static ISet<ItemDefinition> AllBalls() => AllBallsById.Values.ToHashSet();
    public static ItemDefinition? GetBall(Ball ball) => AllBallsById.GetValueOrDefault((ushort)ball);
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