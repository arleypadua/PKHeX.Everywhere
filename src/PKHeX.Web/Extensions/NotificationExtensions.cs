using AntDesign;
using PKHeX.Core;
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

    public static void NotifyConversionFailed(this INotificationService notification, Pokemon pokemon, Game game, EntityConverterResult result) =>
        _ = notification.Open(new()
        {
            Message = $"Can't convert {pokemon.Species.Name} to {game.SaveVersion.Name} ({result}).",
            NotificationType = NotificationType.Error,
        });
}
