using Microsoft.AspNetCore.Components;
using PKHeX.Facade;
using PKHeX.Facade.Pokemons;
using PKHeX.Web.Extensions;

namespace PKHeX.Web.Services;

public class LoadPokemonService(
    GameService gameService,
    NavigationManager navigation)
{
    public Pokemon? Pokemon { get; private set; }

    public void Load(byte[] data)
    {
        Pokemon = Pokemon.LoadFrom(data, gameService.Game);
        navigation.NavigateToLoadedPokemon();
    }
}