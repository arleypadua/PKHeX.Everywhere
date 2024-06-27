using PKHeX.Core;

namespace PKHeX.Facade.Pokemons;

public class Egg(PKM pokemon)
{
    public bool IsEgg
    {
        get => pokemon.IsEgg;
        set => pokemon.IsEgg = value;
    }

    public int? HatchCounter
    {
        get => pokemon.CurrentFriendship;
        set
        {
            var min = EggStateLegality.GetMinimumEggHatchCycles(pokemon);
            var max = EggStateLegality.GetMaximumEggHatchCycles(pokemon);

            // hatch counter is the same as friendship
            pokemon.CurrentFriendship = (byte)Math.Clamp(value ?? 0, min, max);
        }
    }
    
    public EggMetConditions MetConditions => new (pokemon);
}