using System.Security.Cryptography;
using AntDesign;
using PKHeX.Facade;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Web.Services;

[Obsolete("Use the plugin")]
public class AutoLegalityService(INotificationService notification)
{
    static AutoLegalityService()
    {
        AutoLegality.ApplyDefaultConfiguration(timeout: TimeSpan.FromSeconds(2));
    }

    public bool Enabled { get; set; } = true;

    public async Task ApplyIfEnabledAsync(Pokemon pokemon)
    {
        if (!Enabled) return;

        var before = SHA1.HashData(pokemon.Pkm.Data);
        await pokemon.ApplyLegalAsync();
        var after = SHA1.HashData(pokemon.Pkm.Data);

        var changed = !before.SequenceEqual(after);
        if (changed)
        {
            _ = notification.Open(new ()
            {
                Message = "Auto legality applied",
                Description = "Auto legality is enabled and some changes were applied to make the pokemon valid.",
                NotificationType = NotificationType.Success,
            });
        }
    }
}