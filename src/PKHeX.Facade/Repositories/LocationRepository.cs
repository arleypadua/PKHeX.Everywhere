using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public class LocationRepository
{
    private readonly Dictionary<ushort, LocationDefinition> _locations;
    private readonly Dictionary<ushort, LocationDefinition> _eggLocations;

    internal LocationRepository(Game game)
    {
        _locations = GetFrom(GameInfo.GetLocationList(game.GameVersionApproximation.Version, game.SaveFile.Context, egg: false));
        _eggLocations = GetFrom(GameInfo.GetLocationList(game.GameVersionApproximation.Version, game.SaveFile.Context, egg: true));
    }

    public IEnumerable<LocationDefinition> Locations => _locations.Values;
    public IEnumerable<LocationDefinition> EggLocations => _eggLocations.Values;

    public LocationDefinition GetBy(ushort id, bool egg = false) =>
        (egg ? _eggLocations.GetValueOrDefault(id) : _locations.GetValueOrDefault(id)) ?? LocationDefinition.Unknown;

    private Dictionary<ushort, LocationDefinition> GetFrom(IEnumerable<ComboItem> locations) => locations
        .Select(l => new LocationDefinition((ushort)l.Value, l.Text))
        .DistinctBy(l => l.Id)
        .ToDictionary(k => k.Id);
}

public record LocationDefinition(ushort Id, string Name)
{
    public static LocationDefinition Unknown = new(ushort.MaxValue, "(Unknown)");
}