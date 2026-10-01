using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class EncounterHandlers
{
    [Query("encounters.versions", Topics.Game)]
    public static EncounterVersions Versions(Game game) => new(
        game.AvailableVersions
            .Select(version => version.ToEntry())
            .ToArray(),
        game.GameVersionApproximation.Id);
}
