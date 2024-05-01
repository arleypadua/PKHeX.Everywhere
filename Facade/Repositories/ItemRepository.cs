using PKHeX.Core;

namespace PkHex.CLI.Facade.Repositories;

public class ItemRepository
{
    private readonly Dictionary<ushort, ItemDefinition> _items;

    public ItemRepository(SaveFile saveFile)
    {
        _items = GameInfo.Strings.GetItemStrings(saveFile.Context, saveFile.Version)
            .Select((itemName, id) => (id: Convert.ToUInt16(id), itemName))
            .ToDictionary(x => Convert.ToUInt16(x.id), x => new ItemDefinition(Convert.ToUInt16(x.id), x.itemName));
    }

    public ItemDefinition GetItem(ushort id) => _items[id];
}

public record ItemDefinition(ushort Id, string Name)
{
    public static int None = 0;
};