using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Plugins.AutoLegality;

public class MakeLegalOnSave(AutoLegalityModePlugin settings) : IRunOnPokemonSave
{
    public string Description => "Try to make your pokemon legal whenever a pokemon is saved.";
    public async Task<Outcome> OnPokemonSaved(Pokemon pokemon)
    {
        settings.ConfigureAutoLegalityMode();
        await pokemon.ApplyLegalAsync();
        
        return Outcome.Notify(
            message: "Applied legality",
            type: Outcome.Notification.NotificationType.Info);
    }
}