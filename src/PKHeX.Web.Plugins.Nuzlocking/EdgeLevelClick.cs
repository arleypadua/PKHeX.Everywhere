using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Plugins.Nuzlocking;

public class EdgeLevelClick(NuzlockingPlugin settings) : IPokemonEditAction
{
    public string Description => settings.GetBoolean(NuzlockingPlugin.EdgeOnPreviousLevel)
        ? "Will edge a pokemon to the previous level"
        : "Will edge a pokemon to the current level";

    public string Label => "Edge";

    public Task<Outcome> OnActionRequested(Pokemon pokemon)
    {
        var edgeOnPrevious = settings.GetBoolean(NuzlockingPlugin.EdgeOnPreviousLevel);
        var amountOfExpToEdge = Convert.ToUInt32(settings.GetInteger(NuzlockingPlugin.AmountOfExperienceToEdge));

        var previousExp = pokemon.Experience;

        if (edgeOnPrevious)
        {
            // go to the beginning of current level
            pokemon.ChangeLevel(pokemon.Level - 1);
            pokemon.ChangeLevel(pokemon.Level + 1);

            // subtract the amount from current level (edge on previous level)
            pokemon.Experience -= amountOfExpToEdge;
        }
        else
        {
            if (pokemon.Level == 100) return Outcome.Void.Completed();

            // go to the beginning of the next level
            pokemon.ChangeLevel(pokemon.Level + 1);

            // subtract the amount from next level (edge on current level)
            pokemon.Experience -= amountOfExpToEdge;
        }

        var currentExperience = pokemon.Experience;

        return Outcome.Notify(
            "Pokemon experience changed", 
            $"From {previousExp} to {currentExperience}",
            Outcome.Notification.NotificationType.Success).Completed();
    }
}