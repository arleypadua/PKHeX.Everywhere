using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Plugins.AutoLegality;

public class MakeLegalOnChange(AutoLegalityModePlugin settings) : IRunOnPokemonChange
{
    public string Description => "Try to make your pokemon legal on every change.";
    public async Task OnPokemonChange(Pokemon pokemon)
    {
        settings.ConfigureAutoLegalityMode();
        await pokemon.ApplyLegalAsync();
    }
}