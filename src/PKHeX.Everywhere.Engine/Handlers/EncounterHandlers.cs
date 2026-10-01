using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class EncounterHandlers
{
    [Query("encounters.versions", Topics.Game)]
    public static EncounterVersions Versions(Game game) => new(
        game.AvailableVersions
            .Select(version => version.ToEntry())
            .ToArray(),
        game.GameVersionApproximation.Id);

    [Query("encounters.search", Topics.Game)]
    public static EncounterRow[] Search(Session session, Game game, int version, int species)
    {
        var gameVersion = game.AvailableVersions.FirstOrDefault(v => v.Id == version)
            ?? throw new EngineException(ErrorCodes.BadArguments, $"{game.SaveVersion.Name} has no encounters for version {version}.");
        var gameSpecies = game.SpeciesRepository.AllGameSpecies.FirstOrDefault(s => s.Id == species && SpeciesDefinition.IsSome(s))
            ?? throw new EngineException(ErrorCodes.BadArguments, $"{game.SaveVersion.Name} has no species {species}.");

        var encounters = game.PokemonRepository.FindEncounter(gameVersion.Version, gameSpecies.Species).ToList();
        session.Encounters = encounters;
        return encounters.Select((encounter, index) => encounter.ToRow(index)).ToArray();
    }
}
