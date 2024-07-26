using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Plugins.AutoLegality;

public class MakeLegalOnChange(AutoLegalityModePlugin settings) : IRunOnPokemonChange
{
    public string Description => "Try to make your pokemon legal on every change.";
    public async Task<Outcome> OnPokemonChange(Pokemon pokemon)
    {
        settings.ConfigureAutoLegalityMode();
        await pokemon.ApplyLegalAsync();
        return Outcome.Notify(
            message: "Applied legality",
            type: Outcome.Notification.NotificationType.Info);
    }
}