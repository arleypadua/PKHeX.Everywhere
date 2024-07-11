using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Plugins.AutoLegality;

public class MakeLegalOnClick(AutoLegalityModePlugin settings) : IPokemonEditAction
{
    public string Description => "Adds a button to legalize a pokemon when editing it.";
    public string Label => "Legalize";
    
    public async Task OnActionRequested(Pokemon pokemon)
    {
        settings.ConfigureAutoLegalityMode();
        await pokemon.ApplyLegalAsync();
    }
}