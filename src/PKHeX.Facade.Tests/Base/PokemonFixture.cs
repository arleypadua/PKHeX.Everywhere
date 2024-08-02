using PKHeX.Core;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Tests.Base;

public static class PokemonFile
{
    public const string Venusaur = "./data/pokemon/0003 - Venusaur - 8C46F4C83DE9.pb7";
    public const string Golduck = "./data/pokemon/0055 - Golduck - 03FB7AC476C6.pk6";

    public static string For(GameVersion version) => version switch
    {
        GameVersion.GG or GameVersion.GP or GameVersion.GE => Venusaur,
        GameVersion.SV or GameVersion.SL or GameVersion.VL => Golduck,
        _ => throw new InvalidOperationException($"{version} not yet supported on tests"),
    };

    public static Pokemon LoadFor(GameVersion version, Game? game = null) => Pokemon.LoadFrom(File.ReadAllBytes(For(version)), game);
}