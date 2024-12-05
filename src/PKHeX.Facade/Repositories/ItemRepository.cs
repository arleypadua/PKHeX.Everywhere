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
    public ItemDefinition GetGameItem(ushort id) => _gameItems.GetValueOrDefault(id)
                                                    // for whatever reason, some items are not in the game's item list
                                                    // and that ends up failing
                                                    // falling back to the all items list (is this even correct?)
                                                    ?? GetItem(id);
    public ItemDefinition? GetGameItemByName(string name) => _gameItems.Values
        .FirstOrDefault(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

    public static ItemDefinition GetItem(ushort id) => AllItemsById[id];
    public static ItemDefinition? GetItemByName(string name) => AllItemsById.Values
        .FirstOrDefault(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
    public static ISet<ItemDefinition> AllBalls() => AllBallsById.Values.ToHashSet();
    public static ItemDefinition? GetBall(Ball ball) => AllBallsById.GetValueOrDefault((ushort)ball);
}

public record ItemDefinition(ushort Id, string Name)
{
    public static readonly int None = 0;

    public bool IsNone => Id == None;
}