using System.Collections.Immutable;
using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public class GameVersionRepository
{
    public static readonly GameVersionRepository Instance = new();
    
    private readonly Dictionary<GameVersion, GameVersionDefinition> _versions;

    private static readonly Dictionary<GameVersion, GameVersion> FirstOfPair = new()
    {
        [GameVersion.RS] = GameVersion.R,
        [GameVersion.FRLG] = GameVersion.FR,
        [GameVersion.DP] = GameVersion.D,
        [GameVersion.HGSS] = GameVersion.HG,
        [GameVersion.GS] = GameVersion.GD,
        [GameVersion.RB] = GameVersion.RD,
        [GameVersion.BW] = GameVersion.B,
        [GameVersion.B2W2] = GameVersion.B2,
    };

    private GameVersionRepository()
    {
        _versions = Enum.GetValues<GameVersion>()
            .Select(v => new GameVersionDefinition(v, GameVersionDefinition.SideGameNames.GetValueOrDefault(v) ?? GameInfo.GetVersionName(v)))
            .ToDictionary(x => x.Version, x => x);
    }

    public IImmutableList<GameVersionDefinition> All => _versions.Values.ToImmutableList();
    
    public IImmutableList<GameVersionDefinition> Blank => _versions.Values
        .Where(HasBlank)
        .OrderBy(v => v.Name)
        .ToImmutableList();

    public GameVersionDefinition? FindBlank(int id) =>
        _versions.GetValueOrDefault((GameVersion)id) is { } version && HasBlank(version) ? version : null;

    // Colosseum and XD each get their own blank in place of the one they share.
    private static bool HasBlank(GameVersionDefinition version) => !version.Aggregated && version.Version is not GameVersion.CXD;

    public GameVersionDefinition Get(int id) => _versions[(GameVersion)id];
    public GameVersionDefinition Get(GameVersion version) => Get((int)version);

    internal GameVersionDefinition? FindFirstOfPair(GameVersion combined) =>
        FirstOfPair.TryGetValue(combined, out var first) ? Get(first) : null;

    public IImmutableList<GameVersionDefinition> GetAvailableFor(EntityContext generation, GameVersion version) => GameUtil
        .GetVersionsInGeneration(generation, version)
        .Select(Get)
        .ToImmutableList();
}

public record GameVersionDefinition(GameVersion Version, string Name)
{
    // PKHeX saves these games under versions it counts as origins rather than saved versions, and names none of them.
    internal static readonly IReadOnlyDictionary<GameVersion, string> SideGameNames =
        GameCubeSaves.Names.Concat(StadiumSaves.Names).ToDictionary();

    public int Id => (int)Version;
    
    /**
     * PKHeX aggregates some versions, like soul silver and heart gold are loaded as HGSS
     */
    public bool Aggregated => !Version.IsValidSavedVersion() && !SideGameNames.ContainsKey(Version);
}