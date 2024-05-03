using PKHeX.Core;

namespace PKHeX.Facade;

public class Trainer
{
    private readonly Game _game;
    
    public Trainer(Game game)
    {
        _game = game;

        Id = new TrainerId(_game.SaveFile.TID16, _game.SaveFile.SID16);
        Money = new Money(_game);
        Inventories = new Inventories(_game);
        Party = new PokemonParty(_game);
    }

    public TrainerId Id { get; init; }
    public string Name => _game.SaveFile.OT;
    public Gender Gender => Gender.FromByte(_game.SaveFile.Gender);
    public Money Money { get; init; }
    public Inventories Inventories { get; private set; }
    public PokemonParty Party { get; private set; }

    public string? RivalName => _game.SaveFile switch
    {
        SAV1 gen1 => gen1.Rival,
        SAV2 gen2 => gen2.Rival,
        SAV3FRLG gen3 => gen3.RivalName,
        SAV4 gen4 => gen4.Rival,
        SAV5B2W2 gen5 => gen5.Rival,
        SAV7b gen7 => gen7.Misc.Rival,
        SAV8BS gen8 => gen8.Rival,
        _ => null
    };
}

public record TrainerId(ushort TID, ushort SID)
{
    public override string ToString() => $"{TID}/{SID}";
}
