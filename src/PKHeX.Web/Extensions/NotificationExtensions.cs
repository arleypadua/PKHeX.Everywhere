using AntDesign;
using PKHeX.Facade;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Extensions;

public static class NotificationExtensions
{
    public static void NotifyNotInGame(this INotificationService notification, Pokemon pokemon, Game game) =>
        _ = notification.Open(new()
        {
            Message = $"{pokemon.Species.Name} doesn't exist in {game.SaveVersion.Name}.",
            NotificationType = NotificationType.Error,
        });
}
