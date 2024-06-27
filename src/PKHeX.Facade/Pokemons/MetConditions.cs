using PKHeX.Core;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Pokemons;

interface IMetConditions
{
    public GameVersionDefinition Version { get; }
    public Location Location { get; }
    public DateOnly? Date { get; set; }
}

public class EggMetConditions(PKM pokemon) : IMetConditions
{
    public GameVersionDefinition Version => GameVersionRepository.Instance.Get(pokemon.Version);
    public Location Location => new(pokemon.EggLocation, pokemon.GetLocationString(true));
    public DateOnly? Date
    {
        get => pokemon.EggMetDate;
        set => pokemon.EggMetDate = value;
    }
}

public class MetConditions(PKM pokemon) : IMetConditions
{
    public GameVersionDefinition Version => GameVersionRepository.Instance.Get(pokemon.Version);
    public Location Location
    {
        get => new(pokemon.MetLocation, pokemon.GetLocationString(false));
        set => pokemon.MetLocation = value.Id;
    }

    public DateOnly? Date
    {
        get => pokemon.MetDate;
        set => pokemon.MetDate = value;
    }

    public int Level
    {
        get => pokemon.MetLevel;
        set
        {
            var clamped = Math.Clamp(value, 1, 100);
            pokemon.MetLevel = (byte)clamped;
        }
    }
    
    public bool FatefulEncounter
    {
        get => pokemon.FatefulEncounter;
        set => pokemon.FatefulEncounter = value;
    }
}