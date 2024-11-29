using PKHeX.Core;

namespace PKHeX.Facade.Pokemons;

public class Owner(PKM pokemon)
{
    public uint TID
    {
        get => pokemon.TrainerTID7;
        set => pokemon.DisplayTID = value;
    }

    public uint SID
    {
        get => pokemon.DisplaySID;
        set => pokemon.DisplaySID = value;
    }

    public string Name
    {
        get => pokemon.OriginalTrainerName;
        set => pokemon.OriginalTrainerName = value;
    }

    public Gender Gender
    {
        get => Gender.FromByte(pokemon.OriginalTrainerGender);
        set => pokemon.OriginalTrainerGender = value.ToByte();
    }

    public string HandlingTrainerName
    {
        get => pokemon.HandlingTrainerName;
        set => pokemon.HandlingTrainerName = value;
    }

    public Gender HandlingTrainerGender
    {
        get => Gender.FromByte(pokemon.HandlingTrainerGender);
        set => pokemon.HandlingTrainerGender = value.ToByte();
    }

    public Handler CurrentHandler
    {
        get => (Handler)pokemon.CurrentHandler;
        set => pokemon.CurrentHandler = (byte)value;
    }

    public string GetName() => CurrentHandler switch
    {
        Handler.OriginalTrainer => Name,
        Handler.SomeoneElse => HandlingTrainerName,
        _ => string.Empty,
    };

    public enum Handler : byte
    {
        OriginalTrainer = 0,
        SomeoneElse = 1,
    }
}