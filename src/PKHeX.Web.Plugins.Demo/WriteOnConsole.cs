using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Plugins.Demo;

public class WriteOnConsole : IRunOnPokemonChange
{
    public string Description => "This simply writes on the console whenever a pokemon is changed";
    public Task OnPokemonChange(Pokemon pokemon)
    {
        Console.WriteLine($"{pokemon.Species.Name} changed.");
        return Task.CompletedTask;
    }
}