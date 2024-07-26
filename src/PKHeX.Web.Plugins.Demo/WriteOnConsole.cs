using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Plugins.Demo;

public class WriteOnConsole : IRunOnPokemonChange
{
    public string Description => "This simply writes on the console whenever a pokemon is changed";

    public Task<Outcome> OnPokemonChange(Pokemon pokemon)
    {
        Console.WriteLine($"{pokemon.Species.Name} changed.");
        return Outcome.Notify(
            "Logged to the console", 
            "Succeeded at logging to the console",
            Outcome.Notification.NotificationType.Success).Completed();
    }
}