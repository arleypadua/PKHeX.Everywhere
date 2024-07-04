using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Repositories;

namespace PKHeX.Web.Services;

public class EncounterService(GameService gameService)
{
    private Game Game => gameService.Game ?? throw new NullReferenceException("Expected a game to be loaded");
    private PokemonRepository Repository => Game.PokemonRepository;

    public List<Encounter> Encounters { get; private set; } = [];
    public Species? SelectedSpecies { get; set; } = null;
    

    public void Search(Species species, bool? shiny = null, bool? egg = null)
    {
        Encounters = Repository.FindEncounter(species, shiny, egg).ToList();
    }
}