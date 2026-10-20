using System.Runtime.CompilerServices;
using PKHeX.Core;

namespace PKHeX.Facade;

// XD keeps a shadow Pokémon's purification and purified flag in the save's shadow table, which PKHeX keeps private.
// SAV3XD.SetPKM copies the purification there but not the flag, and the Facade's commits skip SetPKM altogether.
internal static class XDShadows
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "ShadowInfo")]
    private static extern ref ShadowInfoTableXD Of(SAV3XD save);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_IsShadow")]
    private static extern void SetIsShadow(XK3 pokemon, bool value);

    public static void Purify(XK3 pokemon)
    {
        pokemon.Purification = 0;
        SetIsShadow(pokemon, false);
    }

    public static void Write(SaveFile save, PKM pokemon)
    {
        if (save is not SAV3XD xd || pokemon is not XK3 { ShadowID: var id } xk3 || id == 0 || id >= xd.MaxShadowID) return;

        var entry = Of(xd)[id];
        entry.Purification = xk3.Purification;
        entry.IsPurified = !xk3.IsShadow;
    }
}
