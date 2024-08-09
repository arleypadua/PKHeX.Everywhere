using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Plugins.Nuzlocking;

public class SetAllEVsToZero : IPokemonStatsEditAction
{
    public string Description => "Adds a button to zero all EVs when editing a Pokemon";
    public string Label => "Zero all EVs";
    
    public Task<Outcome> OnActionRequested(Pokemon pokemon)
    {
        pokemon.EVs.Attack = 0;
        pokemon.EVs.Defense = 0;
        pokemon.EVs.Health = 0;
        pokemon.EVs.Speed = 0;
        pokemon.EVs.SpecialAttack = 0;
        pokemon.EVs.SpecialDefense = 0;

        return Outcome.Notify(
            "All EVs set to 0",
            type: Outcome.Notification.NotificationType.Info).Completed();
    }
}