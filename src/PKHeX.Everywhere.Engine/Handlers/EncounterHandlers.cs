using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class EncounterHandlers
{
    [Query("encounters.versions", Topics.Game)]
    public static EncounterVersions Versions(Game game) => new(
        game.PokemonRepository.EncounterVersions()
            .Select(version => version.ToEntry())
            .ToArray(),
        game.GameVersionApproximation.Id);

    /// <summary>
    /// Lists the event Pokémon the loaded save can hold, whatever game each names as its origin, so a FireRed save lists the WISHMKR Jirachi from Ruby.
    /// A Gen 2 save also lists the Gen 1 events that reach it through the Time Capsule. Wonder Cards go through <c>mysteryGift</c> instead.
    /// Like <c>encounters.search</c>, it replaces the rows <c>box.addEncounter</c> picks from.
    /// </summary>
    /// <param name="species">A species' National Pokédex number, to list only its events.</param>
    [Query("encounters.events", Topics.Game)]
    public static EventRow[] Events(Session session, Game game, int? species = null)
    {
        var gameSpecies = species is { } id
            ? game.SpeciesRepository.AllGameSpecies.FirstOrDefault(s => s.Id == id && SpeciesDefinition.IsSome(s))
                ?? throw new EngineException(ErrorCodes.BadArguments, $"{game.SaveVersion.Name} has no species {id}.")
            : null;

        var events = game.PokemonRepository.FindEvents(gameSpecies?.Species).ToList();
        session.Encounters = events;
        return events.Select((gift, index) => gift.ToEventRow(index)).ToArray();
    }

    [Query("encounters.search", Topics.Game)]
    public static EncounterRow[] Search(Session session, Game game, int version, int species)
    {
        var gameVersion = game.PokemonRepository.EncounterVersions().FirstOrDefault(v => v.Id == version)
            ?? throw new EngineException(ErrorCodes.BadArguments, $"{game.SaveVersion.Name} has no encounters for version {version}.");
        var gameSpecies = game.SpeciesRepository.AllGameSpecies.FirstOrDefault(s => s.Id == species && SpeciesDefinition.IsSome(s))
            ?? throw new EngineException(ErrorCodes.BadArguments, $"{game.SaveVersion.Name} has no species {species}.");

        var encounters = game.PokemonRepository.FindEncounter(gameVersion.Version, gameSpecies.Species).ToList();
        session.Encounters = encounters;
        return encounters.Select((encounter, index) => encounter.ToRow(index)).ToArray();
    }
}
