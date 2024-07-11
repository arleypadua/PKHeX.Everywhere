using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Plugins.AutoLegality;

public class MakeLegalOnSave(AutoLegalityModePlugin settings) : IRunOnPokemonChange
{
    public string Description => "Try to make your pokemon legal whenever a pokemon is saved.";
    public async Task OnPokemonChange(Pokemon pokemon)
    {
        settings.ConfigureAutoLegalityMode();
        await pokemon.ApplyLegalAsync();
    }
}