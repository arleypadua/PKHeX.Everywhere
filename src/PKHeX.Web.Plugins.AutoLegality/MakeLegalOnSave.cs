using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Plugins.AutoLegality;

public class MakeLegalOnSave(AutoLegalityModePlugin settings) : IRunOnPokemonSave
{
    public string Description => "Try to make your pokemon legal whenever a pokemon is saved.";
    public async Task OnPokemonSaved(Pokemon pokemon)
    {
        settings.ConfigureAutoLegalityMode();
        await pokemon.ApplyLegalAsync();
    }
}