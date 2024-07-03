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
        set => pokemon.CurrentFriendship = (byte)Math.Clamp(value ?? 0, MinCounter, MaxCounter);
    }
    
    public int MaxCounter => EggStateLegality.GetMaximumEggHatchCycles(pokemon);
    public int MinCounter => EggStateLegality.GetMinimumEggHatchCycles(pokemon);
    
    public EggMetConditions MetConditions => new (pokemon);
}