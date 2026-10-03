using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Repositories;

public class LocationRepository
{
    private readonly IGameDataSource _data;
    private readonly Dictionary<ushort, LocationDefinition> _locations;
    private readonly Dictionary<ushort, LocationDefinition> _eggLocations;

    internal LocationRepository(Game game)
    {
        _data = game.GameData;
        _locations = GetFrom(game.GameData.MetLocations(game.GameVersionApproximation.Version, egg: false));
        _eggLocations = GetFrom(game.GameData.MetLocations(game.GameVersionApproximation.Version, egg: true));
    }

    public IEnumerable<LocationDefinition> Locations => _locations.Values;
    public IEnumerable<LocationDefinition> EggLocations => _eggLocations.Values;

    public LocationDefinition GetBy(ushort id, bool egg = false) =>
        (egg ? _eggLocations.GetValueOrDefault(id) : _locations.GetValueOrDefault(id))
        ?? (_data.NameOf(GameDataKind.MetLocation, id) is { } name ? new LocationDefinition(id, name) : LocationDefinition.Unknown);

    private static Dictionary<ushort, LocationDefinition> GetFrom(IEnumerable<Choice> locations) => locations
        .Select(l => new LocationDefinition((ushort)l.Id, l.Name))
        .DistinctBy(l => l.Id)
        .ToDictionary(k => k.Id);
}

public record LocationDefinition(ushort Id, string Name)
{
    public static LocationDefinition Unknown = new(ushort.MaxValue, "(Unknown)");
}