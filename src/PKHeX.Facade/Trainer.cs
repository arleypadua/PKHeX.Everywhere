using PKHeX.Facade;
using PKHeX.Core;

namespace PKHeX.Facade;

public class Trainer
{
    private readonly Game _game;
    private readonly SaveFile _saveFile;

    public Trainer(Game game, SaveFile saveFile)
    {
        _game = game;
        _saveFile = saveFile;

        Id = new TrainerId(_saveFile.TID16, _saveFile.SID16);
        Money = new Money(_saveFile);
        Inventories = new Inventories(_game, _saveFile);
    }

    public TrainerId Id { get; init; }
    public string Name => _saveFile.OT;
    public Gender Gender => Gender.FromByte(_saveFile.Gender);
    public Money Money { get; init; }
    public Inventories Inventories { get; private set; }

    public string? RivalName => _saveFile switch
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
