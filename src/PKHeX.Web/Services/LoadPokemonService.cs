using AntDesign;
using Microsoft.AspNetCore.Components;
using PKHeX.Facade;
using PKHeX.Facade.Pokemons;
using PKHeX.Web.Extensions;

namespace PKHeX.Web.Services;

public class LoadPokemonService(
    GameService gameService,
    NavigationManager navigation,
    INotificationService notification)
{
    public Pokemon? Pokemon { get; private set; }

    public void Load(byte[] data)
    {
        var game = gameService.Game;
        var pokemon = Pokemon.LoadFrom(data, game);
        if (game is not null && !game.IsAwareOf(pokemon))
        {
            Pokemon = null;
            notification.NotifyNotInGame(pokemon, game);
            return;
        }

        Pokemon = pokemon;
        navigation.NavigateToLoadedPokemon();
    }
}