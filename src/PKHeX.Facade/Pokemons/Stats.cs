using PKHeX.Core;

namespace PKHeX.Facade.Pokemons;

public record Stats(int Attack, int Defense, int SpecialAttack, int SpecialDefense, int Health, int Speed)
{
    public int Total => Attack + Defense + SpecialAttack + SpecialDefense + Health + Speed;
        
    public static Stats EvFrom(PKM pokemon) => new(pokemon.EV_ATK, pokemon.EV_DEF, pokemon.EV_SPA, pokemon.EV_SPD,
        pokemon.EV_HP, pokemon.EV_SPD);

    public static Stats IvFrom(PKM pokemon) => new(pokemon.IV_ATK, pokemon.IV_DEF, pokemon.IV_SPA, pokemon.IV_SPD,
        pokemon.IV_HP, pokemon.IV_SPD);

    public static Stats BaseFrom(PKM pokemon) =>
        new(pokemon.Stat_ATK, pokemon.Stat_DEF, pokemon.Stat_SPA, pokemon.Stat_SPD, pokemon.Stat_HPMax,
            pokemon.Stat_SPD);
}