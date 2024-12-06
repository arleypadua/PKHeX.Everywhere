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

        if (saveFile.Version == GameVersion.C)
        {
            // for whatever reason, Pokemon Crystal has this last item
            _gameItems[255] = new ItemDefinition(255, "Collapsible bike");
        }
    }

    public ISet<ItemDefinition> GameItems => _gameItems.Values.ToHashSet();
    public ItemDefinition GetGameItem(ushort id) => _gameItems.GetValueOrDefault(id) ?? ItemDefinition.Unknown(id);
    public ItemDefinition? GetGameItemByName(string name) => _gameItems.Values
        .FirstOrDefault(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

    public static ItemDefinition GetItem(ushort id) => AllItemsById.GetValueOrDefault(id) ?? ItemDefinition.Unknown(id);
    public static ItemDefinition? GetItemByName(string name) => AllItemsById.Values
        .FirstOrDefault(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
    public static ISet<ItemDefinition> AllBalls() => AllBallsById.Values.ToHashSet();
    public static ItemDefinition? GetBall(Ball ball) => AllBallsById.GetValueOrDefault((ushort)ball);
}

public record ItemDefinition(ushort Id, string Name)
{
    public static readonly int None = 0;

    public bool IsNone => Id == None;
    
    public static ItemDefinition Unknown(ushort id) => new(id, $"Unknown Item {id}");
}