using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public class GameVersionRepository
{
    public static readonly GameVersionRepository Instance = new();
    
    private readonly Dictionary<GameVersion, GameVersionDefinition> _versions;

    private GameVersionRepository()
    {
        _versions = Enum.GetValues<GameVersion>()
            .Select(v => new GameVersionDefinition(v, GameInfo.VersionDataSource.FirstOrDefault(s => s.Value == (int)v)?.Text ?? v.ToString()))
            .ToDictionary(x => x.Version, x => x);
    }

    public IImmutableList<GameVersionDefinition> All => _versions.Values.ToImmutableList();
    
    public GameVersionDefinition Get(int id) => _versions[(GameVersion)id];
    public GameVersionDefinition Get(GameVersion version) => Get((int)version);

    public IImmutableList<GameVersionDefinition> GetAvailableFor(EntityContext generation, GameVersion version) => GameUtil
        .GetVersionsInGeneration((byte)generation, version)
        .Select(Get)
        .ToImmutableList();
}

public record GameVersionDefinition(GameVersion Version, string Name)
{
    public int Id => (int)Version;
    
    /**
     * PKHeX aggregates some versions, like soul silver and heart gold are loaded as HGSS
     */
    public bool Aggregated => !Version.IsValidSavedVersion();
}