using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class EncounterHandlers
{
    [Query("encounters.versions", Topics.Game)]
    public static EncounterVersions Versions(Game game) => new(
        GameVersionRepository.Instance
            .GetAvailableFor(game.Generation, game.SaveVersion.Version)
            .Select(version => version.ToEntry())
            .ToArray(),
        game.GameVersionApproximation.Id);
}
