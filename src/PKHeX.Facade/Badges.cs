using PKHeX.Core;

namespace PKHeX.Facade;

public record Badge(string Name, bool Earned);

public class Badges
{
    private static readonly string[] Kanto = ["Boulder", "Cascade", "Thunder", "Rainbow", "Soul", "Marsh", "Volcano", "Earth"];
    private static readonly string[] Johto = ["Zephyr", "Hive", "Plain", "Fog", "Storm", "Mineral", "Glacier", "Rising"];
    private static readonly string[] Hoenn = ["Stone", "Knuckle", "Dynamo", "Heat", "Balance", "Feather", "Mind", "Rain"];

    private readonly string[] _names;
    private readonly Func<int> _get;
    private readonly Action<int> _set;

    private Badges(string[] names, Func<int> get, Action<int> set)
    {
        _names = names;
        _get = get;
        _set = set;
    }

    public IReadOnlyList<Badge> All
    {
        get
        {
            var earned = _get();
            return _names.Select((name, i) => new Badge(name, (earned & (1 << i)) != 0)).ToList();
        }
    }

    public void Set(IReadOnlyList<bool> earned)
    {
        if (earned.Count != _names.Length)
            throw new ArgumentOutOfRangeException(nameof(earned), $"This save has {_names.Length} badges, got {earned.Count}.");

        _set(earned.Select((e, i) => e ? 1 << i : 0).Sum());
    }

    internal static Badges? Of(Game game) => game.Format is not null ? null : game.SaveFile switch
    {
        SAV1 gen1 => new Badges(Kanto, () => gen1.Badges, value => gen1.Badges = value),
        SAV2 gen2 => new Badges([..Johto, ..Kanto], () => gen2.Badges, value => gen2.Badges = value),
        SAV3FRLG frlg => new Badges(Kanto, () => frlg.Badges, value => frlg.Badges = value),
        SAV3 gen3 => new Badges(Hoenn, () => gen3.Badges, value => gen3.Badges = value),
        _ => null,
    };
}
