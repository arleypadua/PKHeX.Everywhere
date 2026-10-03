using System.Collections.Immutable;
using PKHeX.Core;
using PKHeX.Facade.Abstractions;

namespace PKHeX.Facade.Repositories;

public class ItemRepository
{
    private static readonly Dictionary<ushort, ItemDefinition> AllItemsById = GameInfo.Strings.Item
        .Select((itemName, id) => (id: Convert.ToUInt16(id), itemName))
        .ToDictionary(x => Convert.ToUInt16(x.id), x => new ItemDefinition(Convert.ToUInt16(x.id), x.itemName));
    
    private static readonly Dictionary<ushort, ItemDefinition> AllBallsById = GameInfo.Strings.balllist
        .Select((itemName, id) => (id: Convert.ToUInt16(id), itemName))
        .ToDictionary(x => Convert.ToUInt16(x.id), x => new ItemDefinition(Convert.ToUInt16(x.id), x.itemName));
    
    private readonly IGameDataSource _data;
    private readonly Dictionary<ushort, ItemDefinition> _gameItems;

    public ItemRepository(SaveFile saveFile) : this(new PKHeXGameData(saveFile))
    {
    }

    internal ItemRepository(IGameDataSource data)
    {
        _data = data;
        _gameItems = data.Items.ToDictionary(item => (ushort)item.Id, item => new ItemDefinition((ushort)item.Id, item.Name));
    }

    public ISet<ItemDefinition> GameItems => _gameItems.Values.ToHashSet();
    public ItemDefinition GetGameItem(ushort id) => _gameItems.GetValueOrDefault(id)
        ?? (_data.NameOf(GameDataKind.Item, id) is { } name ? new ItemDefinition(id, name) { IsUnknown = true } : ItemDefinition.Unknown(id));
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

    /// <summary>
    /// An item the save stores that PKHeX has no id for, named by the save's Game data source. Unlike <see cref="Unknown"/>, which only names an id missing from the game's list.
    /// </summary>
    public bool IsUnknown { get; init; }

    public static ItemDefinition Unknown(ushort id) => new(id, $"Unknown Item {id}");

    public static ItemDefinition Unmapped(ushort stored) => new(Convert.ToUInt16(None), $"Unknown item #{stored}");
}