using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public class GameVersionRepository
{
    public static readonly GameVersionRepository Instance = new();
    
    private readonly Dictionary<int, GameVersionDefinition> _abilities;

    private GameVersionRepository()
    {
        _abilities = GameInfo.Strings.gamelist
            .Select((version, id) => (id, version))
            .ToDictionary(x => x.id, x => new GameVersionDefinition(x.id, x.version));
    }
    
    public GameVersionDefinition Get(int id) => _abilities[id];
    public GameVersionDefinition Get(GameVersion version) => Get((int)version);
}

public record GameVersionDefinition(int Id, string Name);