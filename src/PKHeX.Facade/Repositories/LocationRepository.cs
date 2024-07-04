using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public class LocationRepository
{
    private readonly Dictionary<ushort, LocationDefinition> _locations;
    private readonly Dictionary<ushort, LocationDefinition> _eggLocations;

    internal LocationRepository(Game game)
    {
        _locations = GetFrom(GameInfo.GetLocationList(game.SaveFile.Version, game.SaveFile.Context, egg: false));
        _eggLocations = GetFrom(GameInfo.GetLocationList(game.SaveFile.Version, game.SaveFile.Context, egg: true));
    }

    public IEnumerable<LocationDefinition> Locations => _locations.Values;
    public IEnumerable<LocationDefinition> EggLocations => _eggLocations.Values;
    
    public LocationDefinition GetBy(ushort id, bool egg = false) => egg ? _eggLocations[id] : _locations[id];

    private Dictionary<ushort, LocationDefinition> GetFrom(IEnumerable<ComboItem> locations) => locations
        .Select(l => new LocationDefinition((ushort)l.Value, l.Text))
        .ToDictionary(k => k.Id);
}

public record LocationDefinition(ushort Id, string Name);